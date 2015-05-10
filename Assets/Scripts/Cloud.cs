using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour {

	public Vector3 velocity = new Vector3(0.1f, 0, 0);
	public Vector3 limits = new Vector3(100, 100, 100);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 pos = transform.position;

		// Update position based on speed
		transform.position += velocity;

		// Reset if gone too far
		if (pos.x > limits.x) {
			transform.position = new Vector3(-limits.x, pos.y, pos.z);
		}
		if (pos.z > limits.z) {
			transform.position = new Vector3(pos.x, pos.y, -limits.z);
		}
		if (pos.y > limits.y) {
			transform.position = new Vector3(pos.x, -limits.y, pos.z);
		}

	}
}
