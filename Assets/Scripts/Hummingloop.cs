using UnityEngine;
using System.Collections;

public class Hummingloop : MonoBehaviour {

	// OBJECT HOOKS
	public Kami kami;
	public Transform leftWing;
	public Transform rightWing;

	// MOVEMENT VARIABLES
	public float speed = 0.1f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving

	// BEAT TRACKING / LOOPING VARIABLES
	public int survivalTime = 24;
	public int beatsToLoop = 4;
	private double nextBeatTime;
	private double prevBeat = 0.0; //time at which previous note was played
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;

	// FOCUS VARIABLES
	public float focusSpeed = 0.5f;
	public int focusState = 0; // -1 = unfocus, 0 = default, 1 = focus
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
		AudioClip clip = (AudioClip)Resources.Load ("Audio/hum_output" + idx);
		print ("Loaded " + idx);
		// Get AudioSource components (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
			print (source.clip);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************

		// Get a new target every beat
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;
			
			if (!leaving) // Only refresh target if not leaving
				refreshTarget = true;
			
			survivalTime -= 1;
		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		if (!leaving) {
			// Get new target if necessary
			if (refreshTarget) {
				target = kami.getRandomTarget ("hummingloop");
				refreshTarget = false;
			}
		}

		// Smoothly rotate to target
		// Slerp to facing
		transform.rotation = Quaternion.Slerp(transform.rotation,
		                                      Quaternion.LookRotation (target - transform.position),
		                                      rotSpeed);

		// Move forward at speed
		transform.position += transform.forward * speed;

		// After a certain number of beats, set leaving
		if (survivalTime <= 0 && !leaving) {
			leaving = true;
			// Get a target far away
			target = Random.onUnitSphere * 200;
			print ("Bye!");
		}

		// After even more beats, just disappear
		if (survivalTime <= -18) {
			Destroy (this.gameObject);
			print ("Bye for real.");
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
			print ("Playing sound");
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
	}
}
