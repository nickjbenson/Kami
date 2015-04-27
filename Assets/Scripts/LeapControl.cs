using UnityEngine;
using System.Collections;

public class LeapControl : MonoBehaviour {

	// Leap hand(s) object
	public HandController handController;

	// Oculus reticle
	public OculusReticle reticle;

	// Hands array (only populated when hands detected)
	private HandModel[] hands;

	// Force magnitude becomes more negative the more hands you have
	// facing the target (specified by the reticle).
	// Range: -2 to 2. (-1 to 1 per hand.)
	private float force_magnitude = 0.0f;

	// Update is called once per frame
	void Update () {

		hands = handController.GetAllGraphicsHands ();
		
		force_magnitude = 0.0f;
		foreach (HandModel hand in hands){
			// Force magnitude becomes more negative the more hands you have
			// facing the object. Hands toward object = push away, and vice-versa.
			force_magnitude -= Vector3.Dot (hand.GetPalmNormal(), reticle.looker.forward);
		}
	
	}
	
	public float ForceMagnitude {
		get {
			return force_magnitude;
		}
	}
}
