using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class Oscilloop : Critter {

	/// <summary>
	/// This aligns the movement of the Oscilloop
	/// with the movement of its audio. Don't touch
	/// unless you know what you're doing!
	/// </summary>
	public float timeConstant = 2f;

	// OBJECT HOOKS	
	public Transform inner1;
	public Transform inner2;
	public Transform inner3;
	public Transform inner4;
	public Transform inner5;
	public Transform inner6;
	public Transform inner7;
	public Transform inner8;
	public Transform[] innerRings;
	public Transform masterRing;
	public Transform egg;

	// OSCILLATION VARIABLES
	// normalization constant (divisor)
	private float norm_const = 0;
	private float[] freqs = new float[16];
	private float[] amps = new float[16];
	
	// MOVEMENT VARIABLES
	public float speed = 0.02f; // Movement speed towards target
	public float rotSpeed = 0.05f; // Rotation speed towards target
	private Vector3 target; // target destination
	public Transform travelTransform;
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 128;
	private bool dying = false;

	public override void CritterStart() {
		// Configure radius for grabbing,
		// as oscilloops are rather large.
		critterRadius = 3.0f;

		
		// Ring oscillation configuration initialization.
		innerRings = new Transform[] {inner1, inner2, inner3, inner4, inner5, inner6, inner7, inner8};
	}
	
	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 10));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/osci_output" + idx);
		
		// Oscillation configuration text parsing
		TextAsset config = (TextAsset)Resources.Load ("Audio/osci_output" + idx + "_config");
		print (config.text);
		var result = Regex.Split(config.text, "\r\n|\r|\n");
		int i = 0;
		foreach (string line in result) {
			if (!string.IsNullOrEmpty(line)) {
				if (!line.Contains (" ")) {
					// Normalization constant (first line of file)
					norm_const = float.Parse (line);
				}
				else {
					// Format: "{freq} {amp}\n"
					string[] elems = line.Split (' ');
					freqs[i] = float.Parse (elems[0]);
					amps[i] = float.Parse (elems[1]);
					i++;
				}
			}
		}
		// Apply normalization constant (divisor) to amplitudes
		for (i = 0; i < 16; i++) {
			amps[i] /= norm_const;
		}

		// Now return the clip
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 64;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
		refreshTarget = true;
	}
	
	public override void PostCritterUpdate() {
		
		// ****************
		// RING OSCILLATION
		// ****************
		
		for (int i = 0; i < 8; i++) {
			Transform ring = innerRings[i];
			
			// There are 16 frequency oscillations and 8 rings,
			// so here we sum 2 frequencies per ring.
			Vector3 newPosition = new Vector3(0, Mathf.Sin(Mathf.PI * (float)LivingTime * timeConstant * freqs[i*2]), 0) * amps[i*2];
			newPosition += new Vector3(0, Mathf.Sin (Mathf.PI * (float)LivingTime * timeConstant * freqs[i*2 + 1]), 0) * amps[i*2 + 1];
			
			ring.localPosition = newPosition;
		}
		
		masterRing.localPosition = new Vector3 (0, Mathf.Sin (0.025f * (float)LivingTime * timeConstant), 0);

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		// Modifed from Hummingloop
		
		if (BeingPulled || Captured) {
			// Movement logic while captured or being pulled.
			leaving = false;
			
		} else {
			// Movement logic while not captured.
			
			// While not leaving, get new target whenever
			// we must refresh it
			if (!leaving) {
				if (refreshTarget) {
					target = getRandomSpawnLocation();
					refreshTarget = false;
				}
			}
			
			// If too close to player, turn around
			if (DistanceFromKami <= kami.turnaroundRad && !BeingPulled && survivalTime > 0) {
				target = (transform.position - kami.transform.position) + transform.position;
			}
			
			// Leave if survivalTime is below zero
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
				// Get a target far away
				target = Random.onUnitSphere * 200;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}
			
			// Smoothly rotate travelTransform to target facing
			travelTransform.rotation = Quaternion.Slerp(travelTransform.rotation,
			                                      Quaternion.LookRotation (target - travelTransform.position),
			                                      rotSpeed);
			// Move in travelTransform's forward at speed
			transform.position += travelTransform.forward * speed;
			// Move travelTransform's position along with our transform's
			travelTransform.position = transform.position;

		}
		
		// **************
		// DEATH BEHAVIOR
		// **************
		
		// For now, just die immediately when dying
		if (dying) {
			Destroy(this.gameObject);
		}

	}
	
	public override Vector3 getRandomSpawnLocation() {
		float maxSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * maxSpawnRad;
		while (rPos.sqrMagnitude < (kami.turnaroundRad+1) * (kami.turnaroundRad+1)) {
			rPos = Random.insideUnitSphere * maxSpawnRad; // try again
		}
		return rPos;
	}
}
