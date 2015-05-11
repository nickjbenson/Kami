using UnityEngine;
using System.Collections;

public class Angel : Critter {
	
	// ANIMATION VARIABLES
	bool active = false;
	public float angularSpeed;
	private float rotatedSoFar = 0;
	private Critter.SparseConfig config;
	private int angel_idx = 0;
	
	// MOVEMENT VARIABLES
	public float speed = 0.001f; // Movement speed towards target
	public float rotSpeed = 0.1f;
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the mine is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;
	
	public override void CritterStart () {
	}
	
	public override AudioClip GetCritterAudio() {
//		int idx = (int) Mathf.Ceil(Random.Range (1, 11));
		int idx = 2;
		AudioClip clip = kami.GetAngelAudio(idx);
		config = kami.GetAngelConfig (idx);
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 4;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
	}

	public override void OnCritterSixteenth(){
		if (StartedPlaying) {
			if (config.hits [angel_idx]) {
				active = true;
			}
			angel_idx = (angel_idx + 1) % config.hits.Length;
		}
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

			// Leave if survivalTime is below zero
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
				speed = 0.05f;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}
			
			// Move forward at speed
			transform.position += Vector3.up * speed;
		}

		// *****************
		// SPINNING BEHAVIOR
		// *****************
		
		if (active) {
			transform.Rotate (0.0f, angularSpeed, 0.0f, Space.Self);
			rotatedSoFar += angularSpeed;
			if (rotatedSoFar >= 360){
				active = false;
				rotatedSoFar = 0;
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
}
