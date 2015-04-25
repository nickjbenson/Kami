using UnityEngine;
using System.Collections;

public class MicController : MonoBehaviour {

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
