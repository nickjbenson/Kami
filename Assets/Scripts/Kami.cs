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
	public float pushPullForceMultiplier = 1.0f;

	// Leap Control
	public LeapControl leapControl;
	public bool leapPull = false;
	public bool leapPush = false;
	public float pushPullForce = 0f;

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
			print ("HEY! SET THE PUSHPULL FORCE APPROPRIATELY HERE!");
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

		// Set the focus based on reticle's target
		Transform target = reticle.Target;
		if (target != null) {
			focus = target.parent.GetComponent<Critter>();
		} else {
			focus = null;
		}

		// Pulling
		print (focus);
		if (Input.GetKey (pullKey) || leapPull) {
			if (focus != null) {
				pulling = true;
				pushPullForce = pushPullForceMultiplier;
				print ("Pulling on something.");
			}
		} else {
			pulling = false;
		}

		// Pushing
		if (Input.GetKey (pushKey) || leapPush) {
			if (focus != null) {
				pushing = true;
				pushPullForce = pushPullForceMultiplier;
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

		// temporary/default assignments
		type = hummingloop;
		location = new Vector3(90, 90, 90);
		rotation = new Quaternion (Random.value, Random.value, Random.value, Random.value);
		
		if (critterName == "hummingloop") {
			// Spawn Hummingloop
			type = hummingloop;
		} else if (critterName == "boxworm") {
			// Spawn Boxworm
			type = boxworm;
			rotation = Quaternion.Euler (0, 90, 0);
		} else if (critterName == "maracaw") {
			// Spawn Maracaw
			type = maracaw;
		} else if (critterName == "mine") {
			// Spawn Mine
			type = mine;
		} else if (critterName == "oscilloop") {
			// Spawn Oscilloop
			type = oscilloop;
			rotation = Quaternion.Euler (0, 0, 0);
		}
		
		t = Instantiate (type, location, rotation) as Transform;
		t.GetComponent<Critter>().kami = this;
		t.position = t.GetComponent<Critter>().getRandomSpawnLocation(); // set position
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
	
	/// <summary>
	/// Used by critters to ask whether or not they
	/// are being pushed or pulled right now.
	/// </summary>
	public int getPushPullState(Critter critter) {
		if (focus == critter) {
			if (pulling) {
				return 1;
			}
			else if (pushing) {
				return -1;
			}
		}
		return 0;
	}

	public bool getFocused(Critter critter) {
		return critter == focus;
	}

	public void RegisterCapture(Critter critter) {
		// k thats nice
	}

	public void DeregisterCapture(Critter critter) {
		// mm hmm ok
	}
	
}
