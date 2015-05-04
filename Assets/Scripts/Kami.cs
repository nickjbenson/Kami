using UnityEngine;
using System.Collections;

/// The parent (and creator) of all critters in the world. Kami is the one that instantiates new critters into the
/// world.
/// Also knows the global tempo and chord, and affects the rest of the critters.
/// Critters get globalTempo and globalKey from Kami.
/// When critter is captured, it gets whirlwindSpeed and whirlwindRadius from Kami.
public class Kami : MonoBehaviour {

	// Global music configuration
	public float globalTempo; //number of seconds until next beat
	public int globalKey;

	public string createKey = "q"; // Key to press to spawn things at random
	public string captureKey = "c"; // Key for capturing critters (also used to dismiss)
	public string focusKey = "f"; // Key for focusing on the target critter

	public Transform hummingloop; // hummingloop prefab
	public Transform boxworm; // boxworm prefab
	public Transform puffer;

	private float nextBeat; //time in seconds at which next note should be played

	// Hummingloop spawn/movement radius
	public float hummingMoveRad = 30;

	// Boxworm spawn distance
	public Vector3 boxwormSpawnDist = new Vector3(0, 0, 30);
	// Boxworm spawn radius
	public float boxwormSpawnRad = 30;

	// No-go radius around player (for critters)
	public float noGoRad = 2;

	// Focused transform (critter)
	private Critter focus = null;
	private bool focusActive = false;

	// Oculus Reticle
	public OculusReticle reticle;

	// Capturing (new system)
	public float captureMinRad = 4f;
	public float captureMaxRad = 7f;
	private Critter[] capturedCritters;
	private int captureIdx = 0;
	public int maxCapturedCritters = 6;

	// Releasing manually (new system)
	public float releaseCommandTime = 3f;
	private float releaseTimer = 0;
	private bool releaseTimerEnabled = false;
	private Critter releaseTimerCritter = null;
	private bool captureKeyNotHeldSinceRelease = true;

	// Leap Control
	public LeapControl leapControl;
	public bool leapCapture = false;
	public bool leapFocus = false;

	void Start() {
		nextBeat = (float) AudioSettings.dspTime + globalTempo;

		capturedCritters = new Critter[maxCapturedCritters];
	}

	void BeginRelease() {
		releaseTimerCritter = focus;
		releaseTimerEnabled = true;
		releaseTimer = releaseCommandTime;
		captureKeyNotHeldSinceRelease = false;
	}

	void FinishRelease(bool successful) {
		if (successful) {
			releaseTimerCritter.Release ();
			print ("Release successful.");
		}
		releaseTimerEnabled = false;
		releaseTimer = 0;
		releaseTimerCritter = null;
	}

	void Update(){
//		OculusUpdate ();
	}

	void OculusUpdate () {

		// Leap Motion checks
		if (leapControl.ForceMagnitude <= -0.5) {
			// Hand (palm) mostly facing towards the reticle (target object)
			// interpreted as focus.
			leapFocus = true;
			leapCapture = false;
		} else if (leapControl.ForceMagnitude >= 0.5) {
			// Hand (palm) mostly facing away from the reticle (target object)
			// interpreted as capture + focus.
			leapFocus = true;
			leapCapture = true;
		} else {
			leapFocus = false;
			leapCapture = false;
		}

		print ("ForceMagnitude: " + leapControl.ForceMagnitude);

		if (Input.GetKey (createKey)) {
			spawnRandomCritter ();
		}

		if (Input.GetKey (captureKey) || leapCapture) {
			if (focus != null) {
				if (!focus.Captured && captureKeyNotHeldSinceRelease) {
					// Start capturing!
					focus.BeginCapturing ();
				}
				else if (!releaseTimerEnabled) {
					// Start the release timer
					// and set relevant release variables.
					BeginRelease();
				}
			}
		} else { // capture key not pressed
			captureKeyNotHeldSinceRelease = true;
			if (focus != null && focus.BeingCaptured) {
				// Cancel capturing.
				focus.StopCapturing();
			}
			// Reset the release timer if need be
			if (releaseTimerEnabled) {
				// failed release attempt
				FinishRelease(false);
			}
		}

		// Focus
		if (Input.GetKey (focusKey) || leapFocus) {
			// Start focusing.
			focusActive = true;
		} else {
			// Cancel focusing.
			focusActive = false;
		}

		// Releasing: Hold C on a captured critter.
		// If you hold it longer than the release time
		// set publically (a few seconds), the critter
		// will be released.
		if (releaseTimer > 0 && releaseTimerEnabled) {
			releaseTimer -= Time.deltaTime;
			// Make sure the player kept the focus on
			// the desired critter the whole time.
			if (releaseTimerCritter != focus) {
				// failed release attempt
				FinishRelease(false);
			}
		}
		if (releaseTimer < 0 && releaseTimerEnabled) {
			// successful release
			FinishRelease (true);
		}
		
		// Update focus based on reticle
		Transform reticleTarget = reticle.Target;
		if (reticleTarget != null) {
			Critter possibleCritter = reticleTarget.GetComponentInParent<Critter> ();
			if (possibleCritter != null) {
				focus = possibleCritter;
			}
		} else {
			focus = null;
		}
		print ("Focus: " + focus);
	}

	public void spawnRandomCritter() {

		Transform type;
		Vector3 location;
		Quaternion rotation;
		Transform t;

		if (Random.value < 0.5) {
			// Spawn Hummingloop
			type = hummingloop;
			location = getRandomTarget ("hummingloop");
			rotation = new Quaternion (Random.value, Random.value, Random.value, Random.value);
			t = Instantiate (type, location, rotation) as Transform;
			t.GetComponent<Critter>().kami = this;
		} else {
			// Spawn Boxworm
			type = boxworm;
			location = getRandomTarget ("boxworm");
			rotation = Quaternion.Euler (0, 90, 0);
			t = Instantiate (type, location, rotation) as Transform;
			t.GetComponent<Critter>().kami = this;
		}

		t.parent = transform;

	}

	public float getNextBeat() {
		if (nextBeat == 0) {
			nextBeat = (float) AudioSettings.dspTime;
		}
		if (nextBeat <= AudioSettings.dspTime) {
			nextBeat += globalTempo;
		}
		return nextBeat;
	}

	/// <returns>A (somewhat) random target for the critter to head towards.</returns>
	/// <param name="critterType">Critter type.</param>
	public Vector3 getRandomTarget(string critterType) {
		if (critterType == "hummingloop") {
			Vector3 rPos = Random.insideUnitSphere * hummingMoveRad;
			while (rPos.sqrMagnitude < noGoRad * noGoRad) {
				rPos = Random.insideUnitSphere * hummingMoveRad; // try again
			}
			return rPos;
		} else {
			// boxworm
			Vector3 rPos = Random.insideUnitSphere * boxwormSpawnRad;
			while (rPos.sqrMagnitude < noGoRad * noGoRad) {
				rPos = Random.insideUnitSphere * hummingMoveRad; // try again
			}
			return boxwormSpawnDist + rPos;
		}
	}

	public Vector3 getCaptureSpaceTarget(string critterType) {
		if (critterType == "hummingloop") {
			return Random.onUnitSphere * Random.Range(captureMinRad, captureMaxRad);
		} else {
			print ("unsupported critter type: " + critterType);
			return Vector3.zero;
		}
	}
	
	/// <summary>
	/// Used by critters to ask whether or not they
	/// (or something else) is in focus.
	/// </summary>
	public int getFocusState(Critter critter) {
		if (!focusActive || focus == null) {
			return 0;
		} else
			return (focus == critter) ? 1 : -1;
	}

	public void RegisterCapture(Critter critter) {
		if (capturedCritters [captureIdx] != null) {
			capturedCritters[captureIdx].Release ();
		}
		capturedCritters [captureIdx] = critter;
		captureIdx += 1;
		captureIdx %= maxCapturedCritters;
		print ("Registered a critter.");
	}

	public void DeregisterCapture(Critter critter) {

	}
	
}
