using UnityEngine;
using System.Collections;

/// The parent (and creator) of all critters in the world. Kami is the one that instantiates new critters into the
/// world.
/// Also knows the global tempo and chord, and affects the rest of the critters.
/// Critters get globalTempo and globalKey from Kami.
/// When critter is captured, it gets whirlwindSpeed and whirlwindRadius from Kami.
public class Kami : MonoBehaviour {
	
	public float globalTempo; //number of seconds until next beat
	public int globalKey;
	public float whirlwindRadius;
	public float whirlwindSpeed;
	public float whirlwindHeight;
	public string createKey;
	public Transform hummingloop;
	public Transform boxworm;
	private float nextBeat; //time in seconds at which next note should be played
	public Camera mic;

	void Start() {
		nextBeat = (float) AudioSettings.dspTime + globalTempo;
		mic = GameObject.Find ("Mic").GetComponent<Camera>();
		print (mic);
	}

	void Update () {
		if (Input.GetKey (createKey)) {
			spawnRandomCritter();
		}
	}

	public void spawnRandomCritter() {

		Transform type;
		Vector3 location;
		Quaternion rotation;

		if (Random.value < 0.5) {
			// Spawn Hummingloop
			type = hummingloop;
			location = new Vector3 (Random.Range (-50, 50), Random.Range (-50, 50), Random.Range (-50, 50));
			rotation = new Quaternion (Random.value, Random.value, Random.value, Random.value);
		} else {
			// Spawn Boxworm
			type = boxworm;
			location = new Vector3 (0, 0, 60) + Random.onUnitSphere * 5 + Random.insideUnitSphere * 30;
			rotation = Quaternion.Euler (0, 90, 0);
		}
		
		Transform t = Instantiate (type, location, rotation) as Transform;
		t.parent = transform;

	}

	public float getNextBeat() {
		if (nextBeat <= AudioSettings.dspTime) {
			nextBeat += globalTempo;
		}
		return nextBeat;
	}
	
}
