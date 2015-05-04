using UnityEngine;
using System.Collections;

public class Hummingloop : Critter {

	// OBJECT HOOKS
	public Transform leftWing; // unused, to be used for animation
	public Transform rightWing; // unused, to be used for animation

	// MOVEMENT VARIABLES
	public float speed = 0.05f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving

	// BEAT TRACKING / LOOPING VARIABLES
	public int survivalTime = 24;
	public int beatsToLoop = 8;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;

	// FOCUS VARIABLES
	public float focusSpeed = 0.5f;
	private int focusState = 0; // -1 = unfocus, 0 = default, 1 = focus
	private float targetVolume = 1.0f;
	private float target3DBlend = 1.0f;
	private float actualVolume = 1.0f;
	private float actualBlend = 1.0f;

	// CAPTURE VARIABLES
	private Vector3 targetBeforeCapture;

	// Use this for initialization
	void Start () {
		nextBeatTime = kami.getNextBeat ();

		// AUDIO INITIALIZATION
		// Find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (3, 29));
		// Load audio clip
		AudioClip clip = (AudioClip)Resources.Load ("Audio/hum_output" + idx);
		// Get AudioSource components (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}
		// Start looping on the next available beat
		beatsSinceLastPlay = beatsToLoop - 1;
	}

	// Critter handles most of the logistics of capture. Here
	// we just need to do some additional target logic so the
	// hummingloop knows where to go when it's being "captured."
	public override void OnStartCapture() {
		targetBeforeCapture = target;
		target = kami.transform.position;
		if (survivalTime < 0)
			// Reset survival clock because capture indicates player interest
			survivalTime = 0;
	}

	// As above.
	public override void OnStopCapture() {
		target = targetBeforeCapture;
	}
	
	// Update is called once per frame
	void Update () {

		// *********************
		// INFORMATION GATHERING
		// *********************

		float distanceToKami = Vector3.Distance (transform.position, kami.transform.position);

		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************

		// Get a new target every beat
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;

			// target-refresh behavior only applies if
			// the hummingloop is not leaving, or if
			// it's currently captured
			if (!leaving || Captured) 
				refreshTarget = true;

			survivalTime -= 1;

			if (Captured) {
				// Leave on release, but survive for a while
				survivalTime = 0;
			}
		}

		// *****************
		// CAPTURE BEHAVIOR
		// *****************

		if (distanceToKami <= kami.captureRadius && !Captured && BeingCaptured) {
			FinalizeCapture();
		}

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		if (Captured) {
			// Movement logic while captured.

			// Get a new target inside the capture shell
			// if refreshTarget was set to true (every few beats or so)
			if (refreshTarget) {
				target = transform.position;
				refreshTarget = false;
			}

			// If too close to player, move away
			if (distanceToKami <= kami.turnaroundRad) {
				target = (transform.position - kami.transform.position) + transform.position;
			}

		} else {
			// Movement logic while not captured.

			// While not leaving but uncaptured, get new target
			// if refreshTarget was set to true (every few beats or so)
			if (!leaving) {
				if (refreshTarget && !BeingCaptured) {
					target = getRandomSpawnLocation();
					refreshTarget = false;
				}
			}
			
			// If too close to player, move away
			if (distanceToKami <= kami.turnaroundRad && !BeingCaptured) {
				target = (transform.position - kami.transform.position) + transform.position;
			}
			
			// After a certain number of beats, set leaving
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
				// Get a target far away
				target = Random.onUnitSphere * 200;
			}
			
			// After even more beats, just disappear
			if (survivalTime <= -18) {
				Destroy (this.gameObject);
			}
		}
		
		// Smoothly rotate to target
		// Slerp to facing
		transform.rotation = Quaternion.Slerp(transform.rotation,
		                                      Quaternion.LookRotation (target - transform.position),
		                                      rotSpeed);
		// Move forward at speed
		transform.position += transform.forward * speed;

		// *****************
		// FOCUSING BEHAVIOR
		// *****************

		// Determine focus state. -1 is unfocused, 0 is default, 1 is focused
		focusState = kami.getFocusState(this);

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

	public Vector3 getRandomSpawnLocation() {
		float maxSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * maxSpawnRad;
		while (rPos.sqrMagnitude < (kami.turnaroundRad+1) * (kami.turnaroundRad+1)) {
			rPos = Random.insideUnitSphere * maxSpawnRad; // try again
		}
		return rPos;
	}
}
