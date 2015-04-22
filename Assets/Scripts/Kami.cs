using UnityEngine;
using System.Collections;

/// The parent (creator) of all critters in the world. Kami is the one that instantiates new critters into the
/// world.
/// Also knows the global tempo and chord, and affects the rest of the critters somehow.
/// When critter is captured, it becomes a child of Whirlwind (which is a child of Kami), and begins to spin
/// around Kami/the player/the center of the world (all the same place)
public class Kami : MonoBehaviour {

	public float globalTempo;
	public ArrayList globalChord;
	public string createKey;
	public Transform fish;
	public Transform pickup;

	void Update () {
		if (Input.GetKey (createKey)) {
			Transform type;
			float rand = Random.value;
			if (rand <= 0.5) {
				type = pickup;
			} else {
				type = fish;
			}

			Vector3 location = new Vector3 (Random.Range (-50, 50), Random.Range (-50, 50), Random.Range (-50, 50));
			Quaternion rotation = new Quaternion(Random.value, Random.value, Random.value, Random.value);

			GameObject critter = Instantiate (type, location, rotation) as GameObject;
			critter.transform.parent = transform;
		}
	}
}
