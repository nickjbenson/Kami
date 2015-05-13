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
	protected float critterRadius = 1.0f;

	// MOVEMENT VARIABLES
	private bool beingPulled = false;
	private bool beingPushed = false;
	private float distanceFromKami = 0f;
	private float grabbedDistanceFromKami = 0f;

	// BEAT TRACKING / AUDIO LOOPING VARIABLES
	public double initTime = 0;
	public double myTime = 0;
	public double nextMeasure;
	public double nextBeat;
	public double nextSixteenth;
	public int sixteenthCount = -1;
	public int beatCount = -1;
	public int beatCountForLooping = 0;
	private bool startPlayingScheduled = false;
	private bool startedPlaying = false;
	public int beatsToLoop;
	protected double loopTime;
	public float timeSinceLastLoop = 0;

	// AVOIDING LEVEL OBSTACLES

	private Vector3 avoidanceAccelVector = Vector3.zero;
	private Vector3 avoidanceVelocity = Vector3.zero;
	private float maxAvoidanceSpeed = 5f;
	private float avoidObstacleForce = 50f;
	private float velocityDecay = 0.25f;
	private Vector3 curAvoidObstacleAccelVect = Vector3.zero;
	
	AudioSource[] sources;
	private int soundIndex = 0;
	
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
			return distanceFromKami - critterRadius;
		}
	}
	public float CenterDistanceFromKami {
		get {
			return distanceFromKami;
		}
	}
	
	public double LivingTime {
		get {
			return myTime;
		}
	}
	public double TimeSinceLoop {
		get {
			return LivingTime - loopTime;
		}
	}
	public double KamiTime {
		get {
			return kami.DSPTime;
		}
	}
	public double MeasureLength {
		get {
			return kami.globalTempo * 4;
		}
	}
	public double BeatLength {
		get {
			return kami.globalTempo;
		}
	}
	public double SixteenthLength {
		get {
			return kami.globalTempo / 8.0;
		}
	}
	public bool StartedPlaying {
		get {
			return startedPlaying;
		}
	}

	void Start() {

		// Animation time initialization.
		initTime = (float)AudioSettings.dspTime;

		// ***************************
		// AUDIO TIMING INITIALIZATION
		// ***************************
		
		initTime = KamiTime;
		
		nextBeat = kami.NextBeat - initTime;
		nextSixteenth = kami.NextSixteenth - initTime;
		nextMeasure = kami.NextMeasure - initTime;
		
		int numSixteenthsLeft = (int)(nextBeat / SixteenthLength);
		//		print ("This implies there are " + numSixteenthsLeft + " sixteenths left until the beat.");
		sixteenthCount = 7 - numSixteenthsLeft;
		int numBeatsLeft = (int)(nextMeasure / BeatLength);
		//		print ("It also implies that there are " + numBeatsLeft + " beats left until the measure.");
		beatCount = 3 - numBeatsLeft;

		// ********************
		// AUDIO INITIALIZATION
		// ********************

		// Beat initialization
		beatsToLoop = GetCritterBeatsToLoop ();

		// Get AudioClip from subclass
		AudioClip clip = GetCritterAudio ();
		
		// Get AudioSources (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}
		
		// **************
		// SELECTION HALO
		// **************
		halo = transform.FindChild("Halo").GetComponent<Light>();

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

		myTime = KamiTime - initTime;
		
		if (myTime >= nextSixteenth) {
			nextSixteenth += SixteenthLength;
			sixteenthCount += 1;

			// Sixteenth event.
			OnCritterSixteenth();
			
			if (!startPlayingScheduled
			    	&& ((beatCount + 1) % 4 == 0) // the beat before the next measure
			    	&& ((sixteenthCount + 4) % 8 == 0)) { // halfway into the beat

				sources[soundIndex].PlayScheduled((nextMeasure + initTime));
				soundIndex = (soundIndex + 1) % sources.Length;
				startPlayingScheduled = true;

				}

			if (startedPlaying && (sixteenthCount + 4) % 8 == 0) {

				// Halfway into the beat, check scheduling for loop.

				if ((beatCountForLooping + 1) % beatsToLoop == 0) {

					// Schedule next audio loop.

					sources[soundIndex].PlayScheduled((nextBeat + initTime));
					soundIndex = (soundIndex + 1) % sources.Length;
				}

			}
			
			if (sixteenthCount % 8 == 0) { // Beat.
				nextBeat += BeatLength;
				sixteenthCount = 0;
				beatCount += 1;
				if (startedPlaying) {
					beatCountForLooping += 1;
				}

				// Beat Event.
				OnCritterBeat();

				if (beatCount % 4 == 0) { // Measure.
					nextMeasure += MeasureLength;
					beatCount = 0;

					if (startPlayingScheduled && !StartedPlaying) {
						// Critters start on the measure it this one
						// was scheduled to start here, so set the boolean
						startedPlaying = true;
					}

					// Measure Event.
					OnCritterMeasure();
				}

				if (startedPlaying && beatCountForLooping % beatsToLoop == 0) { // Loop.
					beatCountForLooping = 0;
					loopTime = myTime;

					// Loop event.
					OnCritterLoop ();
				}
			}
		}

		// *****************
		// CAPTURE BEHAVIOR
		// *****************

		if (DistanceFromKami <= kami.captureRadius && beingPulled) {
			if (captured == false) {
				OnCritterCapture ();
			}
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
				grabbedDistanceFromKami = DistanceFromKami + critterRadius;
			}
		} else if (beingPushed) {
			// Move away from the player. (again, scales movement with distance from the player)
			transform.position += (transform.position - kami.transform.position) * kami.pushPullForce * Time.deltaTime;
			grabbed = false;
		}

		// ****************
		// GRABBED BEHAVIOR
		// ****************

		if (grabbed) {
			transform.position = kami.reticle.looker.transform.forward * grabbedDistanceFromKami;
		}
		
		// **************
		// SELECTION HALO
		// **************
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

		// ***************
		// STAGE AVOIDANCE
		// ***************
		
		// Decay avoidanceVelocity over time if not avoiding stage
		if (curAvoidObstacleAccelVect == Vector3.zero) {
			avoidanceVelocity *= velocityDecay * Time.deltaTime;
		}

		// Add avoidance force to acceleration vector
		avoidanceAccelVector = curAvoidObstacleAccelVect;
		// (And reset the avoidance force)
		curAvoidObstacleAccelVect = Vector3.zero;

		// Change velocity by acceleration vector * time
		avoidanceVelocity += avoidanceAccelVector * Time.deltaTime;
		
		// Cap velocity if necessary
		if (avoidanceVelocity.sqrMagnitude > maxAvoidanceSpeed * maxAvoidanceSpeed) {
			avoidanceVelocity = avoidanceVelocity.normalized * maxAvoidanceSpeed;
		}
		
		// Change position by velocity vector * time
		transform.position += avoidanceVelocity * Time.deltaTime;
		
		// ***************
		// SUBCLASS UPDATE
		// ***************

		PostCritterUpdate();

	}

	/// <summary>
	/// Called right after Critter's Start() method.
	/// </summary>
	public virtual void CritterStart() { }

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
	/// Called once every sixteenth note.
	/// </summary>
	public virtual void OnCritterSixteenth() { }
	
	/// <summary>
	/// To be overloaded by implemented critters.
	/// Called once per beat.
	/// </summary>
	public virtual void OnCritterBeat() { }
	
	/// <summary>
	/// Called once per measure.
	/// </summary>
	public virtual void OnCritterMeasure() { }

	/// <summary>
	/// Called every time the creature's audio
	/// loops around to play again.
	/// </summary>
	public virtual void OnCritterLoop() { }
	
	public void OnTriggerEnter(Collider collider) {
		OnCritterTooCloseToObject(collider);
	}
	/// <summary>
	/// Called when the critter is too close to a colliding object.
	/// </summary>
	/// <param name="collider">The collider of the object.</param>
	public virtual void OnCritterTooCloseToObject(Collider collider) {
		// Be pushed away from the offending object
		curAvoidObstacleAccelVect = (transform.position - collider.transform.position).normalized * avoidObstacleForce;
	}

	public void OnTriggerStay(Collider collider) {
		OnCritterStillTooCloseToObject (collider);
	}
	/// <summary>
	/// Called when the critter stays too close to a colliding object.
	/// </summary>
	/// <param name="collider">The collider of the object.</param>
	public virtual void OnCritterStillTooCloseToObject(Collider collider) {
		// Continue to be pushed away from the offending object
		curAvoidObstacleAccelVect = (transform.position - collider.transform.position).normalized * avoidObstacleForce;
	}

	/// <summary>
	/// Called when the critter is captured.
	/// </summary>
	public virtual void OnCritterCapture() { }

	/// <summary>
	/// To be overloaded by implemented critters.
	/// </summary>
	public virtual void PostCritterUpdate() { }
	
	/// <summary>
	/// Gets a random spawn location for this critter.
	/// </summary>
	/// <returns>The random spawn location.</returns>
	public abstract Vector3 getRandomSpawnLocation();

	public class SparseConfig {

		public bool[] hits; //true iff hit
		public int pitch = 0; //only used for some critters
	}
}
