using UnityEngine;
using System.Collections;

public class BoxwormMovement : MonoBehaviour {

	// movement speed
	private float speed = 0.1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// crawl forward
		transform.position += transform.right * speed;

		// delete when they get out too far
		if (transform.position.z <= -90.0) {
			Destroy (this.gameObject);
		}
	
	}
}
