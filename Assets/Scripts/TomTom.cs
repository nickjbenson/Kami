﻿using UnityEngine;
using System.Collections;

public class TomTom : Critter {

	// ANIMATION VARIABLES
	private bool active = false;
	public float angularSpeed;
	private float rotatedSoFar = 0;
	private Critter.SparseConfig config;
	private int tomtom_idx = 5;
	
	// MOVEMENT VARIABLES
	public float speed = 0.01f; // Movement speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the mine is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;
	
	public override void CritterStart () {
	}
	
	public override AudioClip GetCritterAudio() {
//		int idx = (int) Mathf.Ceil(Random.Range (1, 8));
		int idx = 1;
		AudioClip clip = kami.GetTomTomAudio(idx);
		config = kami.GetTomTomConfig (idx);
		//TODO: map color to config.pitch
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
			if (config.hits [tomtom_idx]) {
				active = true;
			}
			tomtom_idx = (tomtom_idx + 1) % config.hits.Length;
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
