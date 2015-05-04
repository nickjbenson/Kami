using UnityEngine;
using System.Collections;

public class Boxworm : Critter {

	// OBJECT HOOKS
	public Transform box1; // unused, to be used for animation
	public Transform box2; // unused, to be used for animation
	public Transform box3; // unused, to be used for animation
	public Transform box4; // unused, to be used for animation

	// MOVEMENT VARIABLES
	public float speed = 0.1f; // Movement speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the boxworm is leaving

	// BEAT TRACKING / LOOPING VARIABLES
	public int beatsToTurnAround = 16;
	private int beatsSinceTurnAround = 0;

	// LIFE/DEATH VARIABLES
	public int survivalTime = 32;
	private bool dying = false;

	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (3, 29));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/box_output" + idx);
		return clip;
	}

	public override int GetCritterBeatsToLoop() {
		return 8;
	}
	
	// Called once per beat.
	public override void OnCritterBeat() {
		beatsSinceTurnAround += 1;
		survivalTime -= 1;
	}

	public override void PostCritterUpdate() {

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************
		
		if (Captured || BeingPulled) {
			// Don't think anything is necessary here??
			
		} else {
			// If not being captured, move back and forth.
			// Movement while not captured and movement while captured
			// is basically the same, except captured boxworms don't care
			// how close to the player they get.
			
			// Move forward at speed
			transform.position += transform.forward * speed;
			
			// If too close to player, turn around
			if (DistanceFromKami <= kami.turnaroundRad) {
				// If moving forward would bring us closer to the player
				if (Vector3.Distance(transform.position + transform.forward,
				                     kami.transform.position)
				    < Vector3.Distance (transform.position - transform.forward,
				                        kami.transform.position)) {
					// turn around.
					beatsSinceTurnAround = beatsToTurnAround;
				}
			}
			
			// Turn around every X beats, only if not leaving
			if (beatsSinceTurnAround >= beatsToTurnAround && (!leaving)) {
				// Turn around
				Vector3 eulerA = transform.rotation.eulerAngles;
				transform.rotation = Quaternion.Euler (eulerA.x, eulerA.y+180, eulerA.z);
				// Reset tracker
				beatsSinceTurnAround = 0;
			}
			
			// After a certain number of beats, set leaving
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}
			
			// **************
			// DEATH BEHAVIOR
			// **************
			
			// For now, just die immediately when dying
			if (dying) {
				Destroy(this.gameObject);
			}
			
		}
	}

	public override Vector3 getRandomSpawnLocation() {
		float boxwormSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * boxwormSpawnRad;
		while (rPos.sqrMagnitude < kami.turnaroundRad * kami.turnaroundRad) {
			rPos = Random.insideUnitSphere * boxwormSpawnRad; // try again
		}
		return rPos;
	}
	
}
