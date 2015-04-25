using UnityEngine;
using System.Collections;

/// Model for how all movement classes should look.
public class Movement : MonoBehaviour {

	void Update () {
		// movement (self or whirlwind)
		if (shouldBeFree()) {
			GetComponent<CritterController> ().exitWhirlwind();
		}
		if (GetComponent<CritterController> ().shouldMove ()) {
			move ();
		} else {
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}

		// death
		if (shouldDie ()) {
			Destroy (this);
		}
	}

	// movement of the critter when not in the whirlwind
	void move() {
	}

	// returns true if critter has outlived its usefullness
	bool shouldDie() {
		return false;
	}

	// returns true if critter should exit whirlwind now
	bool shouldBeFree() {
		if (!GetComponent<CritterController> ().isCaptured ()) {
			// not applicable, not in whirlwind
			return false;
		}
		return false;
	}
}
