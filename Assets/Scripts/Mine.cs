using UnityEngine;
using System.Collections;

public class Mine : Critter {

	float tempo; //time per beat

	bool expanding = true; //if false, contracting
	Vector3 minSphere;
	Vector3 maxSphere;
	double prevTime; //the previous time some change in state happened
	
	// BEAT TRACKING / LOOPING VARIABLES
	public int survivalTime = 24;
	public int beatsToLoop = 8;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;

	void Start () {
		tempo = kami.globalTempo; //for now, this is constant, so can be set once

		float minR = 1;
		float maxR = 1.2f;
		minSphere = new Vector3 (minR, minR, minR);
		maxSphere = new Vector3 (maxR, maxR, maxR);

		nextBeatTime = kami.getNextBeat ();
		prevTime = nextBeatTime - tempo * beatsToLoop;
	}
	
	void Update () {
		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat ();
			beatsSinceLastPlay += 1;
		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

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

		// Play music
		playSound();
	}

	void playSound() {
		if (beatsSinceLastPlay >= beatsToLoop) {
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
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
		print ("not yet implemented");
	}
	
	public override void PostCritterUpdate() {
		print ("not yet implemented");
	}
	
	public override Vector3 getRandomSpawnLocation() {
		print ("you just spawned a mine right on top of yourself. good job");
		return Vector3.zero;
	}
}
