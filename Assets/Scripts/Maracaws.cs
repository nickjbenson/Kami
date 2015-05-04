using UnityEngine;
using System.Collections;

public class Maracaws : Critter {

	public double angularSpeed;
	
	// BEAT TRACKING / LOOPING VARIABLES
	public int survivalTime = 24;
	public int beatsToLoop = 8;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;

	void Start () {
		nextBeatTime = kami.getNextBeat ();

		// AUDIO INITIALIZATION
		// Find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (1, 27));
		// Load audio clip
		AudioClip clip = (AudioClip)Resources.Load ("Audio/maracaws_output" + idx);
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
		
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat ();
			beatsSinceLastPlay += 1;
		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		transform.Rotate (0.0f, 0.0f, (float) angularSpeed);

		if (beatsSinceLastPlay >= beatsToLoop) {
			beatsSinceLastPlay = 0;
		}

		Light halo = transform.FindChild("Halo").gameObject.GetComponent<Light>();

		if (Input.GetKey ("q")){
			halo.enabled = !halo.enabled;
		}

		if (Input.GetKey("z")){
			halo.color = Color.red;
		}

		// Play music
//		playSound();
	}

	void playSound() {
		if (beatsSinceLastPlay >= beatsToLoop) {
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
	}

	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 27));
		AudioClip clip = (AudioClip)Resources.Load ("Audio/maracaws_output" + idx);
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
		print ("you just spawned a maracaw right on top of yourself. good job");
		return Vector3.zero;
	}
}
