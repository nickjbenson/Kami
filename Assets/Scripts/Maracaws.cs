using UnityEngine;
using System.Collections;

public class Maracaws : Critter {

	// OBJECT HOOKS
	public Transform end1;
	public Transform end2;

	// ANIMATION VARIABLES
	public float angularSpeed;
//	private MaracawsConfig config;
//	private int maracaws_idx = 0;

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
		// Configure radius for grabbing
		critterRadius = 2.0f;
	}

	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 30));
		AudioClip clip = kami.GetMaracawsAudio(idx);

		// Animation configuration text parsing
//		config = kami.GetMaracawsConfig (idx);

		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 4;
	}
	
	// Called once per beat
	public override void OnCritterBeat() {
		survivalTime -= 1;
		refreshTarget = true;
	}

//	public override void OnCritterSixteenth() {
//		
//		if (StartedPlaying) {
//			if (config.onoff [maracaws_idx] != -1) {
//				Color prev_color = end1.GetComponent<Renderer> ().material.color;
//				Color color = new Color(1 - prev_color.r, 1 - prev_color.g, 1 - prev_color.b);
//				end1.GetComponent<Renderer> ().material.color = color;
//				end2.GetComponent<Renderer> ().material.color = color;
//			}
//			maracaws_idx = (maracaws_idx + 1) % config.onoff.Length;
//		}
//	}
	
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

			// Move forward at speed
			transform.position += transform.forward * speed;
		}

		// *****************
		// INTERNAL MOVEMENT
		// *****************

		transform.Rotate (0.0f, 0.0f, angularSpeed);

		// **************
		// DEATH BEHAVIOR
		// **************
		
		// For now, just die immediately when dying
		if (dying) {
			Destroy(this.gameObject);
		}
	}

	void twitch(){

	}
	
	public override Vector3 getRandomSpawnLocation() {
		float maxSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * maxSpawnRad;
		while (rPos.sqrMagnitude < (kami.turnaroundRad+1) * (kami.turnaroundRad+1)) {
			rPos = Random.insideUnitSphere * maxSpawnRad; // try again
		}
		return rPos;
	}

	public class MaracawsConfig {
		
		public int[] onoff;

		public int size(){
			return onoff.Length;
		}
		
	}
}
