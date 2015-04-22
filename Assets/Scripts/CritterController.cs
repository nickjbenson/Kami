using UnityEngine;
using System.Collections;

public class CritterController : MonoBehaviour {
	
	public string captureKey;
	private bool captured = false;

	public string soundKey;

	// Update is called once per frame
	void Update () {
		if (captured) {
			float angularSpeed = GetComponentInParent<Kami>().globalTempo;
			transform.RotateAround(Vector3.zero, Vector3.up, angularSpeed * Time.deltaTime);
		}

		if (Input.GetKey (captureKey)) {
			if (captured){
				captured = false;
			}

			else {
				captured = true;
			}
		}
		if (Input.GetKey (soundKey)) {
			var audio = GetComponent<AudioSource>();
			if (!audio.isPlaying) {
				audio.Play();
			}
		}
	}
}
