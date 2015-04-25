using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float angularSpeed;
	public float focusSpeed;
	private Transform focusTarget;
	private bool focusing = false;
	private Vector3 origin;

	void Start() {
		origin = transform.position;
	}

	void Update() {
		if (focusing) {
			MoveTowardsTarget (focusTarget);
		}
	}

	void FixedUpdate() {
		float y = Input.GetAxis ("Horizontal");
		float x = -Input.GetAxis ("Vertical");

		transform.Rotate(x * angularSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
		transform.Rotate (0.0f, y * angularSpeed * Time.deltaTime, 0.0f, Space.World);
	}

	//returns true if focus successful
	public bool focusOn(Transform target){
		if (!focusing) {
			focusTarget = target;
			focusing = true;
			focusSpeed = Vector3.Distance (transform.position, target.position);
			return true;
		}
		return false;
	}

	public void defocus(){
		focusing = false;
		transform.position = origin;
	}

	void MoveTowardsTarget(Transform target){
		if (Vector3.Distance (transform.position, target.position) > 2) {
			transform.position = Vector3.MoveTowards (transform.position, target.position, focusSpeed * Time.deltaTime);
		}
	}
}
