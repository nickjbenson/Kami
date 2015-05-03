using UnityEngine;
using System.Collections;

//Like a buffer fish, blows air in and out (with the tempo).
public class Puffer : Critter {

	public float tempo;

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
		//tempo = GetComponentInParent<Kami> ().globalTempo; //for now, this is constant, so can be set once
		prevTime = AudioSettings.dspTime;
		float minR = 1;
		float maxR = 1.2f;
		minSphere = new Vector3 (minR, minR, minR);
		maxSphere = new Vector3 (maxR, maxR, maxR);

//		nextBeatTime = kami.getNextBeat ();

		// AUDIO INITIALIZATION
		// Find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (1, 9));
		// Load audio clip
		AudioClip clip = (AudioClip)Resources.Load ("Audio/puffer_output" + idx);
		// Get AudioSource components (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}
		// Start looping on the next available beat
		beatsSinceLastPlay = beatsToLoop - 1;
	}
	
	void Update () {
		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		
		// Get a new target every beat
//		if (nextBeatTime <= AudioSettings.dspTime) {
//			nextBeatTime = kami.getNextBeat ();
//			beatsSinceLastPlay += 1;
//		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		double dt = (AudioSettings.dspTime - prevTime);
		float frac = (float)dt / tempo;
		if (expanding) {
			transform.localScale = Vector3.Lerp (minSphere, maxSphere, frac);
		} else {
			transform.localScale = Vector3.Lerp (maxSphere, minSphere, frac);
		}

		if (dt >= tempo) {
			expanding = !expanding;
			prevTime += tempo;
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
}
