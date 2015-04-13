using UnityEngine;
using System.Collections;

public class FishMovement : MonoBehaviour {

	public float speed;

	// Use this for initialization
	void Start () {
		///speed = Random.Range (100, 500);
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Rigidbody> ().AddRelativeForce (Vector3.forward * speed * Time.deltaTime);
	}
}
