using UnityEngine;
using System.Collections;

public class OculusReticle : MonoBehaviour {

	public Transform looker;

	public float maxDistance = 1000.0f;

	private Transform currentTarget = null;

	// Use this for initialization
	void Start () {
		
	}

	public bool HasTarget() {
		return currentTarget != null;
	}

	public Transform Target {
		get {
			return currentTarget;
		}
	}
	
	// Update is called once per frame
	void Update () {

		// RayCast logic. Sets the current target based on raycast,
		// then moves the reticle to the target.
		RaycastHit hit;
		int layerMask = 1 << 8;
		layerMask = ~layerMask;
		if (Physics.Raycast (looker.position, looker.forward, out hit, maxDistance, layerMask)) {
			currentTarget = hit.transform;
		} else {
			currentTarget = null;
		}
		this.transform.position = looker.position + looker.forward * hit.distance;
	}
}
