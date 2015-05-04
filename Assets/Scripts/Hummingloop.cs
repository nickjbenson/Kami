using UnityEngine;
using System.Collections;

public class Hummingloop : Critter {

	// OBJECT HOOKS
	public Transform leftWing; // unused, to be used for animation
	public Transform rightWing; // unused, to be used for animation

	// MOVEMENT VARIABLES
	public float speed = 0.05f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving

	// DEATH VARIABLES
	private bool dying = false;

	// LIFE VARIABLES
	public int survivalTime = 24;

	public override AudioClip GetCritterAudio() {
		int idx = (int)Mathf.Ceil (Random.Range (3, 29));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/hum_output" + idx);
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
			// TODO: Need anything here?

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
			if (DistanceFromKami <= kami.turnaroundRad) {
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
		}
		
		// Smoothly rotate to target
		// Slerp to facing
		transform.rotation = Quaternion.Slerp(transform.rotation,
		                                      Quaternion.LookRotation (target - transform.position),
		                                      rotSpeed);
		// Move forward at speed
		transform.position += transform.forward * speed;

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
