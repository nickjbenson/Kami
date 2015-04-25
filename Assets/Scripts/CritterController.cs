using UnityEngine;
using System.Collections;

public class CritterController : MonoBehaviour {

	private bool captured = false;
	private bool focused = false;
	private float prevBeat = 0.0f; //time at which previous note was played
	AudioSource[] sounds;
	private int soundIndex = 0;

	void Start() {
		//TODO: initialize the sounds for each critter
		sounds = GetComponents<AudioSource> ();
	}

	void OnMouseDown(){ //left click
		enterWhirlwind ();
	}
	void OnMouseOver(){ //right click
		if (Input.GetMouseButtonDown(1)){
			exitWhirlwind();
		}
	}

	void Update () {
		if (captured) {
			float angularSpeed = GetComponentInParent<Kami> ().whirlwindSpeed;
			transform.RotateAround (Vector3.zero, Vector3.up, angularSpeed * Time.deltaTime);
		}

		//sound
		if (sounds.Length > 0) {
			playSound ();
		}
	}
	
	void OnMouseEnter(){
		focus ();
	}
	void OnMouseExit(){
		defocus ();
	}

	//note: when focused, all other sound turns off completely (this can be configured)
	public void focus(){
		if (!captured) {
			focused = GetComponentInParent<Kami> ().mic.GetComponent<MicController>().focusOn (transform);
			AudioListener.volume = 0.0f;
			foreach (AudioSource sound in sounds){
				sound.ignoreListenerVolume = true;
			}
		}
	}
	public void defocus(){
		GetComponentInParent<Kami> ().mic.GetComponent<MicController>().defocus ();
		focused = false;
		AudioListener.volume = 1.0f;
		foreach (AudioSource sound in sounds){
			sound.ignoreListenerVolume = false;
		}
	}

	public bool isCaptured() {
		return captured;
	}

	public bool shouldMove() {
		return !captured&!focused;
	}

	public void enterWhirlwind() {
		if (!captured) {
			captured = true;
			///scale position to radius of whirlwind
			float radius = GetComponentInParent<Kami> ().whirlwindRadius;
			float height = GetComponentInParent<Kami> ().whirlwindHeight;
			if (height == 0){height = 1;}
			Vector3 newPosition = transform.position.normalized * radius;
			newPosition.y = Random.Range(2, 2+height);
			transform.position = newPosition;
		}
	}

	public void exitWhirlwind() {
		if (captured) {
			captured = false;
		}
	}

	//only called if there exists an audiosource
	void playSound() {
		float nextBeat = GetComponentInParent<Kami> ().getNextBeat ();
		if (nextBeat > prevBeat) {
			sounds[soundIndex].PlayScheduled (nextBeat);
			soundIndex = (soundIndex + 1)%sounds.Length;
			prevBeat = nextBeat;
		}
	}

}
