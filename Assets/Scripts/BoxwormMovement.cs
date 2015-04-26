using UnityEngine;
using System.Collections;

public class BoxwormMovement : MonoBehaviour {

	// movement speed
	private float speed = 0.1f;

	void Start () {
		//find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (0, 1));
		if (idx == 0) {
			idx = 1;
		}
		AudioClip clip = (AudioClip) Resources.Load ("output"+idx);
		//prefab already has audiosources
		AudioSource[] sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}	
	}
	
	void Update () {
		if (GetComponent<CritterController> ().shouldMove ()) {
			move ();
		}

		// death
		if (shouldDie ()) {
			Destroy (this.gameObject);
		}
	}

	void move() {
		// crawl forward
		transform.position += transform.right * speed;
	}

	bool shouldDie() {
		// delete when they get out too far
		return (transform.position.z <= -90.0);
	}

}
