using UnityEngine;
using System.Collections;

public class FishMovement : Movement {

	public float speed;
	void Start () {
		///speed = Random.Range (100, 500);
	}

	void Update () {
		// movement (self or whirlwind)
		if (!GetComponent<CritterController> ().shouldMove ()) {
			move ();
		} else {
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	// movement of the critter when not in the whirlwind
	void move() {
		GetComponent<Rigidbody> ().AddRelativeForce (Vector3.forward * speed * Time.deltaTime);
	}
}
