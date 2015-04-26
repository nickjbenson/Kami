using UnityEngine;
using System.Collections;

public class HummingloopMovement : Movement {

	//for regular movement
	private float speed = 0.1f;

	//for movement within circle
	public int switchFreq;
	private float radius = 1;
	private double prev; //time at which previous location was chosen
	private Vector3 center;
	private Vector3 origin;
	private Vector3 target;

	//the time at which critter should start to leave
	private float killTime; 

	void Start () {
		killTime = (float) AudioSettings.dspTime + 20 * GetComponentInParent<Kami> ().globalTempo;

		//find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (0, 6));
		if (idx == 0) {
			idx = 1;
		}
		AudioClip clip = (AudioClip) Resources.Load ("humoutput"+idx);
		//prefab already has audiosources
		AudioSource[] sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}

		//movement
		center = transform.position;
		newLocation (AudioSettings.dspTime);
	}
	
	void Update () {
		if (GetComponent<CritterController> ().shouldMove ()) {
			move ();
		} else {
			center = transform.position; //so that whenever it's released, it has the correct position
			origin = center;
			target = center;
		}

		// death
		if (shouldDie ()) {
			Destroy (this.gameObject);
		}
	}

	void move() {
		if (AudioSettings.dspTime > killTime) {
			center.y += 5 * Time.deltaTime;
		} else {
			//move in horizontal tangent to player
			Vector3 inwardRadius = -transform.position;
			Vector3 tangent = Vector3.Cross (inwardRadius, Vector3.up);
			center += tangent * speed * Time.deltaTime;
//			transform.position = center;
		}
		twitch ();
	}

	void twitch() { //move around in own circle;
		double now = AudioSettings.dspTime;
		double dt = now - prev;
		if (switchFreq == 0) {
			switchFreq = 1;
		}
		float switchTime = GetComponentInParent<Kami> ().globalTempo/switchFreq;

		//moves towards desired location
		float frac = (float)dt / switchTime; //clamped to [0,1]
		transform.position = Vector3.Lerp (origin, target, frac);

		//on each beat, it gets a new location to move towards
		if (dt >= switchTime) {
			newLocation (now);
		}
	}

	void newLocation(double now) {
		origin = transform.position;
		Vector3 disp = new Vector3 (Random.Range (-radius, radius), Random.Range (-radius, radius), Random.Range (-radius, radius));
		target = center + disp;
		prev = now;
//		transform.position = center + target;
	}

	bool shouldDie(){
		return (transform.position.y >= 90.0);
	}
}
