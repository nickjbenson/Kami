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
	public int survivalTime = 32;
	public int beatsToLoop = 16;
	public int beatsToTurnAround = 16;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;
	private int beatsSinceTurnAround = 0;
	
	// FOCUS VARIABLES
	public float focusSpeed = 0.5f;
	private int focusState = 0; // -1 = unfocus, 0 = default, 1 = focus
	private float targetVolume = 1.0f;
	private float target3DBlend = 1.0f;
	private float actualVolume = 1.0f;
	private float actualBlend = 1.0f;

	// Use this for initialization
	void Start () {
		nextBeatTime = kami.getNextBeat ();
		
		// AUDIO INITIALIZATION
		// Find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (3, 29));
		// Load audio clip
		AudioClip clip = (AudioClip)Resources.Load ("Audio/box_output" + idx);
		print ("Loaded " + clip);
		// Get AudioSource components (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
			print (source.clip);
		}
		// Start looping on the next available beat
		beatsSinceLastPlay = beatsToLoop - 1;
	}
	
	// Update is called once per frame
	void Update () {

		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		
		// Track beats
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;
			beatsSinceTurnAround += 1;
			survivalTime -= 1;
		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		// Move forward at speed
		transform.position += transform.forward * speed;

		// Turn around every X beats, only if not leaving
		if (beatsSinceTurnAround >= beatsToTurnAround && !leaving) {
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
		
		// After even more beats, just disappear
		if (survivalTime <= -18) {
			Destroy (this.gameObject);
		}
	
		// *****************
		// FOCUSING BEHAVIOR
		// *****************
		
		// Determine focus state. -1 is unfocused, 0 is default, 1 is focused
		focusState = kami.getFocusState(this.transform);
		
		if (focusState < 0) {
			targetVolume = 0.1f;
			target3DBlend = 1f; // fully 3D
		} else if (focusState == 0) {
			targetVolume = 1f;
			target3DBlend = 1f; // fully 3D
		} else {
			targetVolume = 1f;
			target3DBlend = 0f; // fully 2D (ignore spatial volume dropoff)
		}
		
		// Lerp volume and 3D blend.
		actualVolume = Mathf.Lerp (actualVolume, targetVolume, focusSpeed);
		actualBlend = Mathf.Lerp (actualBlend, target3DBlend, focusSpeed);
		
		// Actually set volume and blend.
		foreach (AudioSource source in sources) {
			source.volume = actualVolume;
			source.spatialBlend = actualBlend;
		}
		
		// Finally, play beautiful sounds!
		playSound ();
	}
	
	void playSound() {
		if (beatsSinceLastPlay >= beatsToLoop) {
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
	}
}
