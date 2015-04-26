using UnityEngine;
using System.Collections;

public class Hummingloop : MonoBehaviour {

	public Kami kami;
	public Transform leftWing;
	public Transform rightWing;
	public float speed = 0.1f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	public int survivalTime = 24;

	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private double nextBeatTime;
	private bool leaving = false; // whether or not the hummingloop is leaving

	// Use this for initialization
	void Start () {
		nextBeatTime = kami.getNextBeat ();
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
	}
}
