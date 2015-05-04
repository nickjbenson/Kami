using UnityEngine;
using System.Collections;

public class Orbiter : MonoBehaviour {

	public Transform rotationOrigin;

	public float angularSpeed = 1f;

	public Vector3 orbitAxis = Vector3.up;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		transform.RotateAround (rotationOrigin.transform.position, orbitAxis, angularSpeed * Time.deltaTime);
	
	}
}
