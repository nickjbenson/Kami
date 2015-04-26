using UnityEngine;
using System.Collections;

public class Hummingloop : MonoBehaviour {

	public Kami kami;
	public Transform leftWing;
	public Transform rightWing;
	public float speed = 0.1f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	public int survivalTime = 24;
	public int beatsToLoop = 4;

	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private double nextBeatTime;
	private bool leaving = false; // whether or not the hummingloop is leaving
	private double prevBeat = 0.0; //time at which previous note was played
	AudioSource[] sources;
	private int soundIndex = 0;
	private int beatsSinceLastPlay = 0;

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
		// Finally, start playing immediately:
		beatsSinceLastPlay = beatsToLoop;
	}
	
	// Update is called once per frame
	void Update () {

		if (!leaving) {
			// Get new target if necessary
			if (refreshTarget) {
				target = kami.getRandomTarget ("hummingloop");
				refreshTarget = false;
			}
		}

		// Get a new target every beat
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;

			if (!leaving) // Only refresh target if not leaving
				refreshTarget = true;
			
			survivalTime -= 1;
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
