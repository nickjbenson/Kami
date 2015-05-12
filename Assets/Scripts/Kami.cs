using UnityEngine;
using System.Collections;

/// The parent (and creator) of all critters in the world. Kami is the one that instantiates new critters into the
/// world.
/// Critters get globalTempo and globalKey from Kami.
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
	public string createBoxwormKey = "2";
	public string createMaracawKey = "3"; 
	public string createMineKey = "4"; 
	public string createOscilloopKey = "5";
	public string createShawarmaKey = "6";
	public string createTomTomKey = "7";
	public string createClangKey = "8";
	public string createAngelKey = "9";
	public string pullKey = "c"; // Key for pulling on critters
	public string pushKey = "x"; // Key for pushing on critters

	public Transform hummingloop; // hummingloop prefab
	public Transform boxworm; // boxworm prefab
	public Transform maracaw; // maracaw prefab
	public Transform mine; // mine prefab
	public Transform oscilloop; // oscilloop prefab
	public Transform tomtom;
	public Transform clang;
	public Transform angel;

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
	// Boxworm
	private AudioClip[] boxwormAudio;
	private Boxworm.BoxwormConfig[] boxwormConfigs;
	// Maracaws
	private AudioClip[] maracawsAudio;
	private Maracaws.MaracawsConfig[] maracawsConfigs;
	// Mine
	private AudioClip[] mineAudio;
	// Shawarma
	private AudioClip[] shawarmaAudio;
	// TomTom
	private AudioClip[] tomtomAudio;
	private Critter.SparseConfig[] tomtomConfigs;
	// Clang
	private AudioClip[] clangAudio;
	private Critter.SparseConfig[] clangConfigs;
	// Angel
	private AudioClip[] angelAudio;
	private Critter.SparseConfig[] angelConfigs;

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

	// Critter Spawns
	public Transform[] spawns;
	public Transform[] crossSpawns;

	void Start() {

		// ********************
		// AUDIO INITIALIZATION
		// ********************
		
		initDspTime = AudioSettings.dspTime;

		// Audio Latency calculation
		int bufferLength = 0, numBuffers = 0;
		AudioSettings.GetDSPBufferSize (out bufferLength, out numBuffers);
		maxLatency = (bufferLength * numBuffers) / 44100.0;

		// ************************************
		// LOADING AUDIO & CONFIGURATION ASSETS
		// ************************************

		// HUMMINGLOOPS (aka Honkyloops)

		// Hummingloop audio
		hummingloopAudio = new AudioClip[30];
		for (int i = 3; i < 29; i++) {
			hummingloopAudio [i] = (AudioClip)Resources.Load ("Audio/hum_output" + i);
		}

		// Hummingloop config
		hummingloopConfigs = new Hummingloop.HummingloopConfig[30];
		for (int i = 3; i < 29; i++) {
			Hummingloop.HummingloopConfig config = new Hummingloop.HummingloopConfig ();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/hum_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			int highestPitch = 0;
			int lowestPitch = 200;
			config.pitches = new int[result.Length];
			foreach (string pitchStr in result) {
				config.pitches [j] = int.Parse (pitchStr);
				if (config.pitches [j] < lowestPitch && config.pitches [j] > 0) {
					lowestPitch = config.pitches [j];
				}
				if (config.pitches [j] > highestPitch) {
					highestPitch = config.pitches [j];
				}
				j++;
			}
			config.middlePitch = (highestPitch + lowestPitch) / 2;
			config.pitchRadius = highestPitch - config.middlePitch;
			hummingloopConfigs [i] = config;
		}

		// BOXWORMS (aka Bevelworms)

		// Boxworm audio
		boxwormAudio = new AudioClip[30];
		for (int i = 3; i < 29; i++) {
			boxwormAudio [i] = (AudioClip)Resources.Load ("Audio/box_output" + i);
		}

		// Boxworm config
		boxwormConfigs = new Boxworm.BoxwormConfig[30];
		for (int i = 3; i < 29; i++) {
			Boxworm.BoxwormConfig config = new Boxworm.BoxwormConfig ();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/box_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			int hit = 0;
			config.hits = new int[result.Length];
			foreach (string pitchStr in result) {
				hit = int.Parse (pitchStr);
				// possible hits: {-1:-1, 48:50, 45:47, 42:-1, 35:36}
				int hitType = 0;
				switch (hit) {
				case 48:
					hitType = 1;
					break;
				case 50:
					hitType = 1;
					break;
				case 45:
					hitType = 2;
					break;
				case 47:
					hitType = 2;
					break;
				case 42:
					hitType = 3;
					break;
				case 35:
					hitType = 4;
					break;
				case 36:
					hitType = 4;
					break;
				default:
					hitType = -1;
					break;
				}
				config.hits [j] = hitType;
				j++;
			}
			boxwormConfigs [i] = config;
		}

		// MARACAWS
		
		// Maracaws audio
		maracawsAudio = new AudioClip[31];
		for (int i = 1; i <= 30; i++) {
			maracawsAudio [i] = (AudioClip)Resources.Load ("Audio/maracaws_output" + i);
		}

		// Maracaws config
		maracawsConfigs = new Maracaws.MaracawsConfig[31];
		for (int i = 1; i <= 30; i++) {
			Maracaws.MaracawsConfig config = new Maracaws.MaracawsConfig ();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/maracaws_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			config.onoff = new int[result.Length];
			foreach (string pitchStr in result) {
				config.onoff [j] = int.Parse (pitchStr);
				j++;
			}
			maracawsConfigs [i] = config;
		}

		// MINE
		
		// Mine audio
		mineAudio = new AudioClip[9];
		for (int i = 1; i <= 8; i++) {
			mineAudio [i] = (AudioClip)Resources.Load ("Audio/mine_output" + i);
		}

		// TOMTOM

		// TomTom audio
		tomtomAudio = new AudioClip[2];
		for (int i = 1; i <= 1; i++) {
			tomtomAudio [i] = (AudioClip)Resources.Load ("Audio/ring_output" + i);
		}

		// TomTom config
		tomtomConfigs = new Critter.SparseConfig[2];
		for (int i = 1; i <= 1; i++) {
			Critter.SparseConfig config = new Critter.SparseConfig ();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/ring_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			config.hits = new bool[result.Length];
			foreach (string pitchStr in result) {
				if (pitchStr.Equals("0")){
					config.hits[j] = false;
				} else {
					config.hits [j] = true;
					config.pitch = int.Parse (pitchStr);
				}
				j++;
			}
			tomtomConfigs [i] = config;
		}

		// ANGEL
		
		// Angel audio
		angelAudio = new AudioClip[11];
		for (int i = 1; i <= 10; i++) {
			angelAudio [i] = (AudioClip)Resources.Load ("Audio/uplift_output" + i);
		}
		
		// Angel config
		angelConfigs = new Critter.SparseConfig[11];
		for (int i = 1; i <= 10; i++) {
			Critter.SparseConfig config = new Critter.SparseConfig ();
			TextAsset textConfig = (TextAsset)Resources.Load ("Audio/uplift_output" + i + "_config");
			var result = textConfig.text.Split (' ');
			int j = 0;
			config.hits = new bool[result.Length];
			foreach (string pitchStr in result) {
				if (pitchStr.Equals("-1")){
					config.hits[j] = false;
				} else {
					config.hits [j] = true;
				}
				j++;
			}
			angelConfigs [i] = config;
		}

		// ********************
		// SPAWN INITIALIZATION
		// ********************
		
		GameObject[] spawnsObj = GameObject.FindGameObjectsWithTag ("CritterSpawn");
		Transform[] spawnsInit = new Transform[spawnsObj.Length];
		for (int i = 0; i < spawnsInit.Length; i++) {
			spawnsInit[i] = spawnsObj[i].transform;
		}
		spawns = spawnsInit;
		
		spawnsObj = GameObject.FindGameObjectsWithTag ("CrossSpawn");
		spawnsInit = new Transform[spawnsObj.Length];
		for (int i = 0; i < spawnsInit.Length; i++) {
			spawnsInit[i] = spawnsObj[i].transform;
		}
		crossSpawns = spawnsInit;

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
			print ("Spawning Bevelworm");
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
		if (Input.GetKeyDown (createTomTomKey)) {
			spawnCritter ("tomtom");
		}
		if (Input.GetKeyDown (createClangKey)) {
			spawnCritter ("clang");
		}
		if (Input.GetKeyDown (createAngelKey)) {
			spawnCritter ("angel");
		}

		// Set the focus based on reticle's target
		Transform target = reticle.Target;
		if (target != null) {
			focus = target.GetComponent<Critter>();
		} else {
			focus = null;
		}

		// Pulling
		// print (focus);
		if (Input.GetKey (pullKey) || leapPull) {
			if (focus != null) {
				pulling = true;
				pushPullForce = pushPullForceMultiplier;
			}
		} else {
			pulling = false;
		}

		// Pushing
		if (Input.GetKey (pushKey) || leapPush) {
			if (focus != null) {
				pushing = true;
				pushPullForce = pushPullForceMultiplier;
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

	public void spawnCritter(Critter critter, Vector3 location) {
		Quaternion rotation;
		Transform t;

		rotation = new Quaternion (Random.value, Random.value, Random.value, Random.value);

		if (critter.GetType() == hummingloop.GetComponent<Hummingloop>().GetType()) {
			// It was a hummingloop
		}
		if (critter.GetType () == boxworm.GetComponent<Boxworm>().GetType ()) {
			// It was a boxworm
			// rotation: face towards the center point, but with zero angle of attack to the horizon.
			rotation = Quaternion.LookRotation((new Vector3(
				transform.position.x, location.y, transform.position.z) - location));
		}
		if (critter.GetType () == maracaw.GetComponent<Maracaws>().GetType ()) {
			// It was a maracaw
		}

		t = Instantiate (critter.transform, location, rotation) as Transform;
		t.GetComponent<Critter>().kami = this;
		t.position = location;
		t.parent = transform;

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
			location = GetRandomSpawn ();
		} else if (critterName == "boxworm") {
			// Spawn Boxworm
			type = boxworm;
			location = GetRandomCrossSpawn ();
			// rotation: face towards the center point, but with zero angle of attack to the horizon.
			rotation = Quaternion.LookRotation ((new Vector3 (
				transform.position.x, location.y, transform.position.z) - location));
		} else if (critterName == "maracaw") {
			// Spawn Maracaw
			type = maracaw;
			location = GetRandomSpawn ();
		} else if (critterName == "mine") {
			// Spawn Mine
			type = mine;
			location = GetRandomSpawn ();
		} else if (critterName == "oscilloop") {
			// Spawn Oscilloop
			type = oscilloop;
			rotation = Quaternion.Euler (0, 0, 0);
			location = GetRandomSpawn ();
		} else if (critterName == "tomtom") {
			type = tomtom;
			location = GetRandomSpawn ();
		} else if (critterName == "clang") {
			type = clang;
			location = GetRandomSpawn ();
		} else if (critterName == "angel") {
			type = angel;
			rotation = Quaternion.Euler (0, 0, 0);
			location = GetRandomSpawn ();
		}
		
		t = Instantiate (type, location, rotation) as Transform;
		t.GetComponent<Critter>().kami = this;
		t.position = location;
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

	public Vector3 GetRandomSpawn() {
		Transform spawn = spawns [Random.Range (0, spawns.Length)];
		return spawn.position;
	}
	public Vector3 GetRandomCrossSpawn() {
		Transform spawn = crossSpawns [Random.Range (0, crossSpawns.Length)];
		return spawn.position;
	}

	public bool getFocused(Critter critter) {
		return critter == focus;
	}

	public AudioClip GetHummingloopAudio(int i) {
		return hummingloopAudio [i];
	}
	public Hummingloop.HummingloopConfig GetHummingloopConfig(int i) {
		return hummingloopConfigs [i];
	}

	public AudioClip GetBoxwormAudio(int i) {
		return boxwormAudio [i];
	}
	public Boxworm.BoxwormConfig GetBoxwormConfig(int i) {
		return boxwormConfigs [i];
	}
	public AudioClip GetMaracawsAudio(int i) {
		return maracawsAudio [i];
	}
	public Maracaws.MaracawsConfig GetMaracawsConfig(int i) {
		return maracawsConfigs [i];
	}
	public AudioClip GetMineAudio(int i) {
		return mineAudio [i];
	}
	public AudioClip GetTomTomAudio(int i){
		return tomtomAudio [i];
	}
	public Critter.SparseConfig GetTomTomConfig(int i){
		return tomtomConfigs [i];
	}
	public AudioClip GetClangAudio(int i){
		return clangAudio [i];
	}
	public Critter.SparseConfig GetClangConfig(int i){
		return clangConfigs [i];
	}
	public AudioClip GetAngelAudio(int i){
		return angelAudio [i];
	}
	public Critter.SparseConfig GetAngelConfig(int i){
		return angelConfigs [i];
	}
	
}
