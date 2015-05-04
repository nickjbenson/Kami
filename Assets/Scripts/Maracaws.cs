using UnityEngine;
using System.Collections;

public class Maracaws : Critter {

	public double angularSpeed;
	
	// MOVEMENT VARIABLES
	public float speed = 0.05f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;
	
	public override void CritterStart() {
		// Nothing needed.
	}

	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 27));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/maracaws_output" + idx);
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 8;
	}
	
	// Called once per beat
	public override void OnCritterBeat() {
		survivalTime -= 1;
		refreshTarget = true;
	}
	
	// Update is called once per frame
	public override void PostCritterUpdate () {
		
		// *****************
		// MOVEMENT BEHAVIOR
		// *****************
		
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
			
			// Smoothly rotate to target
			// Slerp to facing
			transform.rotation = Quaternion.Slerp(transform.rotation,
			                                      Quaternion.LookRotation (target - transform.position),
			                                      rotSpeed);
			// Move forward at speed
			transform.position += transform.forward * speed;
		}

		// Rotate on axis
		// TODO: This rotation is overriden when the creature
		// isn't captured. Either this logic or the logic
		// above needs to be changed so the maracaws rotates
		// even when not captured.
		transform.Rotate (0.0f, 0.0f, (float) angularSpeed);
		
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
