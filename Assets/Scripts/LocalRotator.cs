using UnityEngine;
using System.Collections;

public class LocalRotator : MonoBehaviour {

	public Vector3 localRotationAxis = Vector3.up;

	public bool useRandomAngularSpeed = true;
	public float minAngularSpeed = -3.5f;
	public float maxAngularSpeed = 3.5f;
	public float angularSpeed = 1f;

	void Start() {
		if (useRandomAngularSpeed) {
			angularSpeed = Random.Range(minAngularSpeed, maxAngularSpeed);
		}
	}
	
	void Update () {
		transform.Rotate(localRotationAxis, angularSpeed * Time.deltaTime, Space.Self);
	}
}
