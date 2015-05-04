using UnityEngine;
using System.Collections;

public abstract class Critter : MonoBehaviour {

	// OBJECT HOOKS
	public Kami kami;

	// HALO
	private Light halo;

	// CAPTURE/GRAB VARIABLES
	private bool captured = false;
	private bool grabbed = false;

	// MOVEMENT VARIABLES
	private bool beingPulled = false;
	private bool beingPushed = false;
	private float distanceFromKami = 0f;
	private float grabbedDistanceFromKami = 0f;

	// BEAT TRACKING / AUDIO LOOPING VARIABLES
	public int beatsToLoop = 0;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;
	
	public bool Captured {
		get {
			return captured;
		}
		protected set {
			captured = value;
		}
	}
	public bool Grabbed {
		get {
			return grabbed;
		}
		protected set {
			grabbed = value;
		}
	}
	public bool BeingPulled {
		get {
			return beingPulled;
		}
	}
	public bool BeingPushed {
		get {
			return beingPushed;
		}
	}
	public float DistanceFromKami {
		get {
			return distanceFromKami;
		}
	}

	void Start() {

		// ********************
		// AUDIO INITIALIZATION
		// ********************

		// Beat initialization
		nextBeatTime = kami.getNextBeat ();
		beatsToLoop = GetCritterBeatsToLoop ();
		
		// Get AudioClip from subclass
		AudioClip clip = GetCritterAudio ();
		
		// Get AudioSources (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}
		
		// Start looping on the next available beat
		beatsSinceLastPlay = beatsToLoop - 1;
		
		// **************
		// SELECTION HALO
		// **************
		halo = transform.FindChild("Halo").gameObject.GetComponent<Light>();

		CritterStart ();
	}

	void Update() {
		
		// *********************
		// INFORMATION GATHERING
		// *********************
		
		distanceFromKami = Vector3.Distance (transform.position, kami.transform.position);
		int pushPullState = kami.getPushPullState (this);
		beingPulled = pushPullState == 1;
		beingPushed = pushPullState == -1;
		
		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		
		// Get a new target every beat
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;
			OnCritterBeat();
		}

		// *****************
		// CAPTURE BEHAVIOR
		// *****************

		if (DistanceFromKami <= kami.captureRadius && beingPulled) {
			captured = true;
		} else if (DistanceFromKami > kami.captureRadius && beingPushed && Captured) {
			captured = false;
		}

		// ***********************
		// PUSH/PULL/GRAB BEHAVIOR
		// ***********************

		if (beingPulled) {
			// Move towards the player (unless within grab radius).
			if (DistanceFromKami > kami.grabRadius) {
				// this scales the movement with the distance from the player (closer = slower)
				transform.position += (kami.transform.position - transform.position) * kami.pushPullForce * Time.deltaTime;
			} else {
				grabbed = true;
			}
		} else if (beingPushed) {
			// Move away from the player. (again, scales movement with distance from the player)
			transform.position += (transform.position - kami.transform.position) * kami.pushPullForce * Time.deltaTime;
			grabbed = false;
		}

		// ****
		// HALO
		// ****
		if (kami.getFocused (this)) {
			halo.enabled = true;
		} else {
			halo.enabled = false;
		}
		if (grabbed) {
			halo.color = Color.red;
		} else if (captured) {
			halo.color = Color.yellow;
		} else {
			halo.color = Color.white;
		}

		// ****************
		// GRABBED BEHAVIOR
		// ****************

		if (grabbed) {
			grabbedDistanceFromKami = DistanceFromKami;
			transform.position = kami.reticle.looker.transform.forward * grabbedDistanceFromKami;
		}
		
		// *************
		// PLAYING AUDIO
		// *************

		playSound ();
		
		// ***************
		// SUBCLASS UPDATE
		// ***************

		PostCritterUpdate();

	}

	void playSound() {
		if (beatsSinceLastPlay >= beatsToLoop) {
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
	}

	/// <summary>
	/// Called right after Critter's Start() method.
	/// </summary>
	public abstract void CritterStart();

	/// <summary>
	/// Returns an AudioClip to be played by this critter.
	/// </summary>
	/// <returns>The critter audio.</returns>
	public abstract AudioClip GetCritterAudio();

	/// <summary>
	/// Gets the number of beats that this creature's audio
	/// is to play before it should loop.
	/// </summary>
	/// <returns>The critter beats to loop.</returns>
	public abstract int GetCritterBeatsToLoop();
	
	/// <summary>
	/// To be overloaded by implemented critters.
	/// Called once per beat.
	/// </summary>
	public abstract void OnCritterBeat();

	/// <summary>
	/// To be overloaded by implemented critters.
	/// </summary>
	public abstract void PostCritterUpdate();
	
	/// <summary>
	/// Gets a random spawn location for this critter.
	/// </summary>
	/// <returns>The random spawn location.</returns>
	public abstract Vector3 getRandomSpawnLocation();

}
