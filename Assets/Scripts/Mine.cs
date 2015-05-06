using UnityEngine;
using System.Collections;

public class Mine : Critter {

	int expanding = 2; //0|1 if expanding, 2|3 if contracting

	// MOVEMENT VARIABLES
	public float speed = 0.003f; // Movement speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the mine is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;

	public override void CritterStart () {
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
		expanding = (expanding + 1)%4; //because I want it to change every two beats
	}
	
	public override void PostCritterUpdate() {

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
			if (Vector3.Distance(transform.position, target) < 0.1){
				refreshTarget = true;
			}

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
				speed = 0.05f;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}

			// Move forward at speed
			transform.position += transform.forward * speed;
		}

		// ******************
		// EXPANSION BEHAVIOR
		// ******************

		if (expanding == 0 || expanding == 1) {
			transform.localScale += Vector3.one/200.0f;
		} else {
			transform.localScale -= Vector3.one/200.0f;
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
