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

	// "Captured" whirlwind
	public float whirlwindRadius;
	public float whirlwindSpeed;
	public float whirlwindHeight;

	public string createKey; // Key to press to spawn things at random

	public Transform hummingloop; // hummingloop prefab
	public Transform boxworm; // boxworm prefab

	private float nextBeat; //time in seconds at which next note should be played

	public Camera mic; // The camera used as an audiolistener

	// Hummingloop movement radius
	public float hummingMoveRad = 30;

	// No-go radius around player (for critters)
	public float noGoRad = 2;

	void Start() {
		nextBeat = (float) AudioSettings.dspTime + globalTempo;
		mic = GameObject.Find ("Mic").GetComponent<Camera>();
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

	/// <returns>A (somewhat) random target for the critter to head towards.</returns>
	/// <param name="critterType">Critter type.</param>
	public Vector3 getRandomTarget(string critterType) {
		Vector3 rPos = Random.insideUnitSphere * hummingMoveRad;
		while (rPos.sqrMagnitude < noGoRad * noGoRad) {
			rPos = Random.insideUnitSphere * hummingMoveRad; // try again
		}
		return rPos;
	}
	
}
