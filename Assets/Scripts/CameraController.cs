using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float angularSpeed;

	void Start() {
		Cursor.visible = false;
	}

	void FixedUpdate() {
		float y = Input.GetAxis ("Mouse X");
		float x = -Input.GetAxis ("Mouse Y");

		transform.Rotate (x * angularSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
		transform.Rotate (0.0f, y * angularSpeed * Time.deltaTime, 0.0f, Space.World);
	}

}
