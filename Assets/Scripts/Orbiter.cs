using UnityEngine;
using System.Collections;

public class Orbiter : MonoBehaviour {

	public Transform rotationOrigin;
	
	public bool useRandomAngularSpeed = true;

	public float minAngularSpeed = -2.5f;
	public float maxAngularSpeed = 2.5f;

	public float angularSpeed = 1f;

	public Vector3 orbitAxis = Vector3.up;

	// Use this for initialization
	void Start () {
		if (useRandomAngularSpeed) {
			angularSpeed = Random.Range (minAngularSpeed, maxAngularSpeed);
		}
	}
	
	// Update is called once per frame
	void Update () {

		transform.RotateAround (rotationOrigin.transform.position, orbitAxis, angularSpeed * Time.deltaTime);
	
	}
}
