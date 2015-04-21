using UnityEngine;
using System.Collections;

public class Whirlwind : MonoBehaviour {

	public float globalTempo;
	void Update () {
		transform.Rotate(new Vector3(0.0f, globalTempo, 0.0f) * Time.deltaTime);
	}
}
