using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

	public Vector3 rotationAxis = Vector3.up;
	public float angularSpeed = 1f;

	void Update () {
		transform.Rotate (rotationAxis, angularSpeed * Time.deltaTime);
	}
}
