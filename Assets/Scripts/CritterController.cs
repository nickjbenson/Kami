using UnityEngine;
using System.Collections;

public class CritterController : MonoBehaviour {
	
	public string captureKey;
	public GameObject whirlwind;
	private bool captured = false;

	public string soundKey;

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (captureKey)) {
			if (captured == false){
			transform.parent = whirlwind.transform;
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
