using UnityEngine;
using System.Collections;

public class Shawarmaa : MonoBehaviour {

	public float angularSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		for (int i=0; i < transform.childCount; i++){
			Transform child = transform.GetChild(i);
			child.Rotate(Vector3.left * angularSpeed);
		}
	}
}
