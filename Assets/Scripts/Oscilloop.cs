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

	// OSCILLATION CONFIG VARIABLES
	// normalization constant (divisor)
	private float norm_const = 0;
	private float[] freqs = new float[16];
	private float[] amps = new float[16];

	// ANIMATION VARIABLES
	public double animationTimeOffset = 2;
	
	// MOVEMENT VARIABLES
	public float minDesiredDistanceFromPlayer = 10f;
	public float maxDesiredDistanceFromPlayer = 30f;
	public float desiredDistanceFromPlayer = 0f;

	private Vector3 accelVector = Vector3.zero; // assume mass is unity
	private int direction = 1;
	public float thrust = 3f;
	public float orbitalInclination = 10f;
	private int orbitalConst = 1;
	private Vector3 velocity = Vector3.zero;
	public float maxSpeed = 1.5f;
	public bool leaving = false; // whether or not the oscilloop is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 128;
	private bool dying = false;

	public override void CritterStart() {
		// Configure radius for grabbing,
		// as oscilloops are rather large.
		critterRadius = 3.0f;

		// Set desired distance from player
		desiredDistanceFromPlayer = Random.Range (minDesiredDistanceFromPlayer, maxDesiredDistanceFromPlayer);
		
		// Ring oscillation configuration initialization.
		innerRings = new Transform[] {inner1, inner2, inner3, inner4, inner5, inner6, inner7, inner8};
	}
	
	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 10));
		AudioClip clip = kami.GetOscilloopAudio(idx);

		OscilloopConfig config = kami.GetOscilloopConfig(idx);
		norm_const = config.norm_const;
		freqs = config.freqs;
		amps = config.amps;

		// Now return the clip
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 64;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
	}

	public override void OnCritterMeasure() {
		// Maybe turn around (50/50 chance)
		if (Random.value > 0.50f) {
			orbitalConst *= -1;
		}
	}

	public double AnimationTime {
		get {
			return (myTime + animationTimeOffset) * timeConstant;
		}
	}
	
	public override void PostCritterUpdate() {
		
		// ****************
		// RING OSCILLATION
		// ****************
		
		for (int i = 0; i < 8; i++) {
			Transform ring = innerRings[i];
			
			// There are 16 frequency oscillations and 8 rings,
			// so here we sum 2 frequencies per ring.
			Vector3 newPosition = new Vector3(0, Mathf.Sin(Mathf.PI * (float)AnimationTime * freqs[i*2]), 0) * amps[i*2];
			newPosition += new Vector3(0, Mathf.Sin (Mathf.PI * (float)AnimationTime * freqs[i*2 + 1]), 0) * amps[i*2 + 1];
			
			ring.localPosition = newPosition;
		}
		
		masterRing.localPosition = new Vector3 (0, Mathf.Sin (0.025f * (float)LivingTime * timeConstant), 0);

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************
		
		// Set accel vector to be towards the player
		accelVector = (kami.transform.position - transform.position).normalized;
		
		if (BeingPulled || Captured) {

			// Do nothing while captured
			
		} else {
			// Movement logic while not captured.

			// If not leaving, head toward desired distance from player
			if (!leaving) {

				
				// If not close enough to player, move toward
				if (CenterDistanceFromKami > desiredDistanceFromPlayer) {
					direction = 1;
				}
				
				// If too close to player, move away
				if (CenterDistanceFromKami < desiredDistanceFromPlayer) {
					direction = -1;
				}

			} else {
				// If we're leaving and not captured, move away from player
				direction = -1;
			}

			// Apply direction to acceleration vector
			accelVector *= direction;

			// Apply orbital inclination to acceleration vector
			accelVector = Quaternion.AngleAxis (orbitalInclination * direction * orbitalConst, Vector3.up) * accelVector;

			// Multiply acceleration vector by thrust
			accelVector *= thrust;

			// Change velocity by acceleration vector * time
			velocity += accelVector * Time.deltaTime;

			// Cap velocity if necessary
			if (velocity.sqrMagnitude > maxSpeed * maxSpeed) {
				velocity = velocity.normalized * maxSpeed;
			}

			// Change position by velocity vector * time
			transform.position += velocity * Time.deltaTime;
			
			// Set leaving if survivalTime is below zero
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}

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

	public class OscilloopConfig {

		public float norm_const = 0;
		public float[] freqs = new float[16];
		public float[] amps = new float[16];

	}
}
