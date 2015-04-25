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
	private float nextBeat; //time in seconds at which next note should be played

	void Start() {
		nextBeat = (float) AudioSettings.dspTime + globalTempo;
	}

	void Update () {
		if (Input.GetKey (createKey)) {
			Transform type;
//			int idx = Random.Range(0, num_critters - 1);
// add more here when we have more types
			type = hummingloop;

			Vector3 location = new Vector3 (Random.Range (-50, 50), Random.Range (-50, 50), Random.Range (-50, 50));
			Quaternion rotation = new Quaternion(Random.value, Random.value, Random.value, Random.value);

			Transform t = Instantiate (type, location, rotation) as Transform;
			GameObject critter = t.gameObject;
			critter.transform.parent = transform;
		}
	}

	public float getNextBeat() {
		if (nextBeat <= AudioSettings.dspTime) {
			nextBeat += globalTempo;
		}
		return nextBeat;
	}
	
}
