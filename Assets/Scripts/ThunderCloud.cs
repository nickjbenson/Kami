using UnityEngine;
using System.Collections;

public class ThunderCloud : Critter {

	// ANIMATION VARIABLES
	private bool flashing = false;
	private Critter.SparseConfig config;
	private int cloud_idx = 5;
	private GameObject lightning;
	
	// MOVEMENT VARIABLES
	public float speed = 0.001f; // Movement speed towards target
	private bool leaving = false; // whether or not the mine is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;
	
	public override void CritterStart () {
		lightning = transform.FindChild ("Lightning").gameObject;
	}
	
	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 5));
		//TODO: actual audio
		AudioClip clip = kami.GetThunderCloudAudio(idx);
		config = kami.GetThunderCloudConfig (idx);
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
			if (config.hits [cloud_idx]) {
				flashing = true;
			}
			cloud_idx = (cloud_idx + 1) % config.hits.Length;
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
			transform.position += Vector3.left * speed; //all clouds move left, kind of gives impression of "wind" maybe?
		}
		
		// *****************
		// SPINNING BEHAVIOR
		// *****************
		
		if (flashing) {
			lightning.SetActive(true);
			flashing = false;
		} else {
			lightning.SetActive(false);
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
