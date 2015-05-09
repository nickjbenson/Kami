using UnityEngine;
using System.Collections;

/// The parent (and creator) of all critters in the world. Kami is the one that instantiates new critters into the
/// world.
/// Also knows the global tempo and chord, and affects the rest of the critters.
/// Critters get globalTempo and globalKey from Kami.
/// When critter is captured, it gets whirlwindSpeed and whirlwindRadius from Kami.
public class Kami : MonoBehaviour {

	// Global music configuration
	public float globalTempo; // seconds per beat
	public int globalKey;

	// Max scheduling latency
	public double maxLatency;

	private double dspTime = 0;
	private double initDspTime = 0;
	private double nextMeasure = 0;
	private double nextBeat = 0;
	private double nextSixteenth = 0;

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

	// LOADED AUDIO RESOURCES
	// Hummingloop
	private AudioClip[] hummingloopAudio;
	private Hummingloop.HummingloopConfig[] hummingloopConfigs;

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
		initDspTime = AudioSettings.dspTime;

		// Audio Latency calculation
		int bufferLength = 0, numBuffers = 0;
		AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
		maxLatency = (bufferLength * numBuffers) / 44100.0;

		// ************************************
		// LOADING AUDIO & CONFIGURATION ASSETS
		// ************************************

		// Hummingloop audio
		print ("Loading hummingloop audio.");
		hummingloopAudio = new AudioClip[30];
		for (int i = 3; i < 29; i++) {
			hummingloopAudio[i] = (AudioClip)Resources.Load ("Audio/hum_output" + i);
		}
		print ("Done loading hummingloop audio.");

		// Hummingloop config
		print ("Loading hummingloop configs.");
		hummingloopConfigs = new Hummingloop.HummingloopConfig[30];
		for (int i = 3; i < 29; i++) {
			Hummingloop.HummingloopConfig config = new Hummingloop.HummingloopConfig();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/hum_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			int highestPitch = 0;
			int lowestPitch = 200;
			config.pitches = new int[result.Length];
			foreach (string pitchStr in result) {
				config.pitches[j] = int.Parse (pitchStr);
				if (config.pitches[j] < lowestPitch && config.pitches[j] > 0) {
					lowestPitch = config.pitches[j];
				}
				if (config.pitches[j] > highestPitch) {
					highestPitch = config.pitches[j];
				}
				j++;
			}
			config.middlePitch = (highestPitch + lowestPitch) / 2;
			config.pitchRadius = highestPitch - config.middlePitch;
			hummingloopConfigs[i] = config;
		}
		print ("Done loading hummingloop configs.");
	}

	void FixedUpdate(){
		PlayerUpdate ();
		TimeUpdate ();
	}

	void PlayerUpdate () {

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
			print ("Spawning Honkyloop");
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
			if (target.parent != null) {
				focus = target.parent.GetComponent<Critter>();
			}
		} else {
			focus = null;
		}

		// Pulling
		// print (focus);
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

	void TimeUpdate() {

		dspTime = AudioSettings.dspTime - initDspTime;

		nextMeasure = (dspTime - (dspTime % (globalTempo * 4))) + globalTempo * 4;
		nextBeat = (dspTime - (dspTime % globalTempo)) + globalTempo;
		nextSixteenth = (dspTime - (dspTime % (globalTempo / 8))) + globalTempo / 8;

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

	public float DSPTime {
		get {
			return (float)dspTime;
		}
	}
	public float NextMeasure {
		get {
			return (float)nextMeasure;
		}
	}
	public float NextBeat {
		get {
			return (float)nextBeat;
		}
	}
	// TODO: Theoretically this should be 1/4 of a beat
	// but for some reason it's actually 1/8th?
	public float NextSixteenth {
		get {
			return (float)nextSixteenth;
		}
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

	public AudioClip GetHummingloopAudio(int i) {
		return hummingloopAudio [i];
	}
	public Hummingloop.HummingloopConfig GetHummingloopConfig(int i) {
		return hummingloopConfigs [i];
	}
	
}
