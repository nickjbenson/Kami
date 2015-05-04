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

	public string createHummingloopKey = "1"; // Key to press to spawn a Hummingloop
	public string createBoxwormKey = "2"; // Key to press to spawn a Hummingloop
	public string createMaracawKey = "3"; // Key to press to spawn a Hummingloop
	public string createMineKey = "4"; // Key to press to spawn a Hummingloop
	public string createOscilloopKey = "5"; // Key to press to spawn a Hummingloop
	public string pullKey = "c"; // Key for pulling on critters
	public string pushKey = "x"; // Key for pushing on critters

	public Transform hummingloop; // hummingloop prefab
	public Transform boxworm; // boxworm prefab
	public Transform maracaw; // maracaw prefab
	public Transform mine; // mine prefab
	public Transform oscilloop; // oscilloop prefab

	private float nextBeat; //time in seconds at which next note should be played

	// CRITTER ACTION RADII
	// No-Go / turnaround radius
	public float turnaroundRad = 9f;
	// Capture radius
	public float captureRadius = 7f;
	// Grab radius
	public float grabRadius = 3f;
	// Death radius (creatures start dying beyond this radius)
	public float deathRadius = 45f;

	// Focused transform (critter)
	private Critter focus = null;
	private bool focusActive = false;

	// Oculus Reticle
	public OculusReticle reticle;

	// Pushing and Pulling
	private bool pulling = false;
	private bool pushing = false;

	// Leap Control
	public LeapControl leapControl;
	public bool leapPull = false;
	public bool leapPush = false;

	// Oculus Toggle
	public bool oculusEnabled = false;

	void Start() {
		nextBeat = (float) AudioSettings.dspTime + globalTempo;
	}

	void Update(){
		OculusUpdate ();
	}

	void OculusUpdate () {

		// Leap Motion checks
		if (oculusEnabled) {
			if (leapControl.ForceMagnitude <= -0.5) {
				// Hand (palm) mostly facing towards the reticle (target object)
				// interpreted as focus.
				leapPush = true;
				leapPull = false;
			} else if (leapControl.ForceMagnitude >= 0.5) {
				// Hand (palm) mostly facing away from the reticle (target object)
				// interpreted as capture + focus.
				leapPush = true;
				leapPull = true;
			} else {
				leapPush = false;
				leapPull = false;
			}
			
			print ("ForceMagnitude: " + leapControl.ForceMagnitude);
		}

		if (Input.GetKeyDown (createHummingloopKey)) {
			spawnCritter("hummingloop");
		}
		if (Input.GetKeyDown (createBoxwormKey)) {
			spawnCritter("boxworm");
		}
		if (Input.GetKeyDown (createMaracawKey)) {
			spawnCritter("maracaw");
		}
		if (Input.GetKeyDown (createMineKey)) {
			spawnCritter("mine");
		}
		if (Input.GetKeyDown (createOscilloopKey)) {
			spawnCritter("oscilloop");
		}

		// Pulling
		if (Input.GetKey (pullKey) || leapPull) {
			if (focus != null) {
				pulling = true;
				print ("Pulling on something.");
			}
		} else {
			pulling = false;
		}

		// Pushing
		if (Input.GetKey (pushKey) || leapPush) {
			if (focus != null) {
				pushing = true;
				print ("Pushing on something.");
			}
		} else {
			pushing = false;
		}
	}

	public void spawnCritter(string critterName) {

		Transform type;
		Vector3 location;
		Quaternion rotation;
		Transform t;

		if (critterName == "hummingloop") {
			// Spawn Hummingloop
			type = hummingloop;
			location = new Vector3(90, 90, 90); // temporary
			rotation = new Quaternion (Random.value, Random.value, Random.value, Random.value);
			t = Instantiate (type, location, rotation) as Transform;
			t.GetComponent<Critter>().kami = this;
			t.position = t.GetComponent<Hummingloop>().getRandomSpawnLocation(); // set position
			t.parent = transform;
		} else if (critterName == "boxworm") {
			// Spawn Boxworm
			type = boxworm;
			location = new Vector3(90, 90, 90); // temporary
			rotation = Quaternion.Euler (0, 90, 0);
			t = Instantiate (type, location, rotation) as Transform;
			t.GetComponent<Critter>().kami = this;
			t.parent = transform;
			print ("BOXWORM POSITIONS NOT SET AFTER SPAWN.");
		} else if (critterName == "maracaw") {
			print ("Spawn maracaw not implemented");
		} else if (critterName == "mine") {
			print ("Spawn mine not implemented");
		} else if (critterName == "oscilloop") {
			print ("Spawn oscilloop not implemented");
		}

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
	public Vector3 getRandomTargetzzzzDELETESOON(string critterType) {
		if (critterType == "hummingloop") {
			var hummingMoveRad = 0f; // just to get rid of errors
			Vector3 rPos = Random.insideUnitSphere * hummingMoveRad;
			while (rPos.sqrMagnitude < turnaroundRad * turnaroundRad) {
				rPos = Random.insideUnitSphere * hummingMoveRad; // try again
			}
			return rPos;
		} else {
			// boxworm
			var hummingMoveRad = 0f; // just to get rid of errors
			float boxwormSpawnRad = this.deathRadius - 5f;
			Vector3 rPos = Random.insideUnitSphere * boxwormSpawnRad;
			while (rPos.sqrMagnitude < turnaroundRad * turnaroundRad) {
				rPos = Random.insideUnitSphere * hummingMoveRad; // try again
			}
			return rPos;
		}
	}
	
	/// <summary>
	/// Used by critters to ask whether or not they
	/// are being pushed or pulled right now.
	/// </summary>
	public int getPushPullState(Critter critter) {
		if (focus == null) {
			return 0;
		} else
			return (focus == critter) ? 1 : -1;
	}

	// deprecated. use getPushPullState.
	// backwards-compatibility reasons only, delete soon.
	public int getFocusState(Critter critter) {
		return getPushPullState (critter);
	}

	public void RegisterCapture(Critter critter) {
		// k thats nice
	}

	public void DeregisterCapture(Critter critter) {
		// mm hmm ok
	}
	
}
