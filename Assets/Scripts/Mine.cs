using UnityEngine;
using System.Collections;

public class Mine : Critter {

	float tempo; //time per beat

	bool expanding = true; //if false, contracting
	Vector3 minSphere;
	Vector3 maxSphere;
	double prevTime = 0; //the previous time some change in state happened
	
	// MOVEMENT VARIABLES (copied from hummingloop, for hummingloop-copied movement)
	public float speed = 0.05f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;

	public override void CritterStart () {
		tempo = kami.globalTempo; //for now, this is constant, so can be set once
		prevTime = AudioSettings.dspTime;

		float minR = 1;
		float maxR = 1.2f;
		minSphere = new Vector3 (minR, minR, minR);
		maxSphere = new Vector3 (maxR, maxR, maxR);
	}
	
	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 8));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/mine_output" + idx);
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 8;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
		refreshTarget = true;
	}
	
	public override void PostCritterUpdate() {

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		// COPIED FROM HUMMINGLOOP.
		
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

		// ******************
		// EXPANSION BEHAVIOR
		// ******************

		// print (prevTime)
		double dt = (AudioSettings.dspTime - prevTime);
		float frac = (float)dt / (tempo * beatsToLoop / 2.0f);
		if (expanding) {
			transform.localScale = Vector3.Lerp (minSphere, maxSphere, frac);
		} else {
			transform.localScale = Vector3.Lerp (maxSphere, minSphere, frac);
		}
		
		if (frac >= 1) {
			expanding = !expanding;
			prevTime += tempo * beatsToLoop / 2.0f;
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
