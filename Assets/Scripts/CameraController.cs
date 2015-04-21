using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float angularSpeed;

	void FixedUpdate() {
		float y = Input.GetAxis ("Horizontal");
		float x = -Input.GetAxis ("Vertical");

		transform.Rotate(x * angularSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
		transform.Rotate (0.0f, y * angularSpeed * Time.deltaTime, 0.0f, Space.World);
	}
}
