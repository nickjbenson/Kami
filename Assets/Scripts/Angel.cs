﻿using UnityEngine;
using System.Collections;

public class Angel : Critter {
	
	// ANIMATION VARIABLES
	bool active = false;
	public float angularSpeed;
	private float rotatedSoFar = 0;
	
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
		int idx = (int) Mathf.Ceil(Random.Range (1, 8));
		AudioClip clip = kami.GetMineAudio(idx);
		//TODO: replace with actual clip
		//TODO: map color to pitch, using config
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 4;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
		// TODO: check config to set active
	}
	
	public override void PostCritterUpdate() {
		
		// TODO: get rid of this once you've implemented beat matching
		if (Input.GetKey ("z")) {
			active = true;
		}
		
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
