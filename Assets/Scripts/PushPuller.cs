using UnityEngine;
using System.Collections;

public class PushPuller : MonoBehaviour {

	public HandController handController;
	public OculusReticle reticle;
	public float maxMovementDelta = 0.05f;
	public float minDistance = 1.0f;

	private HandModel[] hands;
	private float force_magnitude = 0.0f;	

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		hands = handController.GetAllGraphicsHands ();

		force_magnitude = 0.0f;
		foreach (HandModel hand in hands){
			// Force magnitude becomes more negative the more hands you have
			// facing the object. Hands toward object = push away, and vice-versa.
			force_magnitude -= Vector3.Dot (hand.GetPalmNormal(), reticle.looker.forward);
		}

		if (reticle.HasTarget ()) {
			Transform target = reticle.Target;
			float distance = Vector3.Distance (reticle.Target.position, this.transform.position);
			if (distance >= minDistance || force_magnitude < 0) {
				target.position = Vector3.MoveTowards (target.position, this.transform.position,
			                                              	force_magnitude * maxMovementDelta);
			}
		}
	
	}

	public float ForceMagnitude {
		get {
			return force_magnitude;
		}
	}
}
