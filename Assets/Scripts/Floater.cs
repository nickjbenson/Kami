using UnityEngine;
using System.Collections;

public class Floater : MonoBehaviour {
	
	public float amplitude = 1.0f;
	public float frequency = 1.0f;

	public bool useRandomFrequency = true;
	public bool useRandomAmplitude = true;

	public float minFrequency = 0.1f;
	public float maxFrequency = 2.0f;
	public float minAmplitude = 0.5f;
	public float maxAmplitude = 2.0f;

	public Vector3 oscillationDirection = Vector3.up;

	private Vector3 oldOffset = Vector3.zero;

	// Use this for initialization
	void Start () {
		if (useRandomFrequency) {
			frequency = Random.Range (minFrequency, maxFrequency);
		}
		if (useRandomAmplitude) {
			amplitude = Random.Range (minAmplitude, maxAmplitude);
		}
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 offset = amplitude * oscillationDirection * Mathf.Sin ((float)Time.time * frequency);

		transform.localPosition = transform.localPosition + offset - oldOffset;

		oldOffset = offset;

	}
}
