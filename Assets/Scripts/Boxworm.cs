using UnityEngine;
using System.Collections;

public class Boxworm : Critter {

	// OBJECT HOOKS
	public Transform body;
	public Transform box1;
	public Transform box2;
	public Transform box3;
	public Transform box4;

	// ANIMATION VARIABLES
	public int hitIndexOffset = 5;
	public float animLerpTimeFactor = 1.0f;
	public float fadeLerpTimeFactor = 1.0f;
	private int hit_idx = 0;
	private int currentHit = 0;
	public Vector3 defaultOffset;
	public float defaultScale;
	public Color defaultColor;
	public Vector3 hit1Offset;
	public float hit1Scale;
	public Color hit1Color;
	public Vector3 hit2Offset;
	public float hit2Scale;
	public Color hit2Color;
	public Vector3 hit3Offset;
	public float hit3Scale;
	public Color hit3Color;
	public Vector3 hit4Offset;
	public float hit4Scale;
	public Color hit4Color;
	public Vector3[] hitOffsets;
	public float[] hitScales;
	public Color[] hitColors;
	private Vector3[] targetHitOffsets;
	private float[] targetHitScales;
	private Color[] targetHitColors;
	private Vector3[] curHitOffsets;
	private float[] curHitScales;
	private Color[] curHitColors;
	private double timeSinceSixteenth = 0;
	/*
	 * Hit reference:
	 * 1 (0) High Tom
	 * 2 (1) Low Tom
	 * 3 (2) Snare
	 * 4 (3) Bass
	 */

	// ANIMATION CONFIGURATION VARIABLES
	public BoxwormConfig config;
	public int[] hits;

	// MOVEMENT VARIABLES
	public float movementSpeed = 0.1f; // Movement speed forward
	private Vector3 targetDriftVector;
	private Vector3 curDriftVector;
	public float driftSpeed = 0.1f; // Perpendicular drift speed
	public float driftLerpSpeed = 0.1f; // Time factor for lerping to new drift vectors

	// LIFE/DEATH VARIABLES
	private bool dying = false;

	public int HitIndex {
		get {
			return (hit_idx + hitIndexOffset) % hits.Length;
		}
	}

	public override void CritterStart() {
		hitOffsets = new Vector3[4] {hit1Offset, hit2Offset, hit3Offset, hit4Offset};
		hitScales = new float[4] {hit1Scale, hit2Scale, hit3Scale, hit4Scale};
		hitColors = new Color[4] {hit1Color, hit2Color, hit3Color, hit4Color};
		targetHitOffsets = new Vector3[4] {defaultOffset, defaultOffset, defaultOffset, defaultOffset};
		targetHitScales = new float[4] {defaultScale, defaultScale, defaultScale, defaultScale};
		targetHitColors = new Color[4] {defaultColor, defaultColor, defaultColor, defaultColor};
		curHitOffsets = new Vector3[4] {defaultOffset, defaultOffset, defaultOffset, defaultOffset};
		curHitScales = new float[4] {defaultScale, defaultScale, defaultScale, defaultScale};
		curHitColors = new Color[4] {defaultColor, defaultColor, defaultColor, defaultColor};
	}

	public override AudioClip GetCritterAudio() {

		int idx = (int)Mathf.Ceil (Random.Range (3, 29)); //TODO Fix value range
		AudioClip clip = kami.GetBoxwormAudio(idx);
		
		// Animation configuration text parsing
		BoxwormConfig config = kami.GetBoxwormConfig (idx);
		hits = config.hits;
		
		return clip;

	}

	public override int GetCritterBeatsToLoop() {
		return 8;
	}

	public override void OnCritterSixteenth() {

		if (StartedPlaying) {
			hit_idx += 1;
			currentHit = hits [HitIndex];
		} else {
			currentHit = -1;
		}

		// Copy states down to tail from earlier boxes
		targetHitOffsets [3] = targetHitOffsets [2];
		targetHitScales [3] = targetHitScales [2];
		targetHitColors [3] = targetHitColors [2];
		targetHitOffsets [2] = targetHitOffsets [1];
		targetHitScales [2] = targetHitScales [1];
		targetHitColors [2] = targetHitColors [1];
		targetHitOffsets [1] = targetHitOffsets [0];
		targetHitScales [1] = targetHitScales [0];
		targetHitColors [1] = targetHitColors [0];

		// Set state of head box
		if (currentHit != -1) {
			targetHitOffsets[0] = hitOffsets [currentHit-1];
			targetHitScales[0] = hitScales [currentHit-1];
			targetHitColors[0] = hitColors [currentHit-1];
		} else {
			targetHitOffsets[0] = defaultOffset;
			targetHitScales[0] = defaultScale;
			targetHitColors[0] = defaultColor;
		}

		timeSinceSixteenth = 0;

	}

	public override void OnCritterBeat() {
		// Get a new target drift vector.
		targetDriftVector = GetNewDriftVector ();
	}

	// Called every beat
	public Vector3 GetNewDriftVector() {
		var upFactor = Random.Range (0.1f, 1f);
		var rightFactor = Random.Range (0.1f, 1f);
		var flipUp = Random.value < 0.5f ? -1 : 1;
		var flipRight = Random.value < 0.5f ? -1 : 1;
		return ((transform.up * upFactor * flipUp) + (transform.right * rightFactor * flipRight)).normalized;
	}

	public override void OnCritterLoop() {
		hit_idx = 0;
	}

	public override void PostCritterUpdate() {

		// ******************
		// ANIMATION BEHAVIOR
		// ******************

		for (int i = 0; i < 4; i++) {

			// Fade target variables to default variables based on time since sixteenth note

			targetHitOffsets[i] = Vector3.Lerp(targetHitOffsets[i], defaultOffset, (float)timeSinceSixteenth * fadeLerpTimeFactor);
			targetHitScales[i] = Mathf.Lerp(targetHitScales[i], defaultScale, (float)timeSinceSixteenth * fadeLerpTimeFactor);
			targetHitColors[i] = Color.Lerp(targetHitColors[i], defaultColor, (float)timeSinceSixteenth * fadeLerpTimeFactor);

			// Lerp current config variables to target variables

			curHitOffsets[i] = Vector3.Lerp (curHitOffsets[i], targetHitOffsets[i], animLerpTimeFactor);
			curHitScales[i] = Mathf.Lerp (curHitScales[i], targetHitScales[i], animLerpTimeFactor);
			curHitColors[i] = Color.Lerp (curHitColors[i], targetHitColors[i], animLerpTimeFactor);
		}



		// Set current offset, scale, and color of the tail box
		box4.transform.localPosition = curHitOffsets[3];
		box4.transform.localScale = new Vector3(curHitScales[3], curHitScales[3], curHitScales[3]);
		box4.gameObject.GetComponent<Renderer> ().material.color = curHitColors[3];

		// Set current offset, scale, color of the 3rd box
		box3.transform.localPosition = curHitOffsets[2];
		box3.transform.localScale = new Vector3(curHitScales[2], curHitScales[2], curHitScales[2]);
		box3.gameObject.GetComponent<Renderer> ().material.color = curHitColors[2];

		// Set current offset, scale, color of the 2nd box
		box2.transform.localPosition = curHitOffsets[1];
		box2.transform.localScale = new Vector3(curHitScales[1], curHitScales[1], curHitScales[1]);
		box2.gameObject.GetComponent<Renderer> ().material.color = curHitColors[1];
		
		// Set the current offset, scale, and color of the head box
		box1.transform.localPosition = curHitOffsets[0];
		box1.transform.localScale = new Vector3(curHitScales[0], curHitScales[0], curHitScales[0]);
		box1.gameObject.GetComponent<Renderer> ().material.color = curHitColors[0];

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************
		
		if (Captured || BeingPulled) {
			// Don't think anything is necessary here??
			
		} else {
			
			// If too close to player, drift away
			if (DistanceFromKami <= kami.turnaroundRad) {
				targetDriftVector = (this.transform.position - kami.transform.position).normalized;
			}

			// Lerp current drift vector to target drift vector
			curDriftVector = Vector3.Lerp (curDriftVector, targetDriftVector, driftLerpSpeed);

			// Move forward at speed
			transform.position += transform.forward * movementSpeed;

			// Move by drift vector
			transform.position += curDriftVector * driftSpeed;
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}
			
			// **************
			// DEATH BEHAVIOR
			// **************
			
			// For now, just die immediately when dying
			if (dying) {
				Destroy(this.gameObject);
			}
			
		}
	}

	public override void OnCritterTooCloseToObject (Collider collider) {
		// Set drift vector away from collider
		targetDriftVector = ((transform.position - collider.transform.position) + transform.position).normalized;
	}
	public override void OnCritterStillTooCloseToObject(Collider collider) {
		// Set drift vector away from collider
		targetDriftVector = ((transform.position - collider.transform.position) + transform.position).normalized * 2;
	}

	public override Vector3 getRandomSpawnLocation() {
		float boxwormSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * boxwormSpawnRad;
		while (rPos.sqrMagnitude < kami.turnaroundRad * kami.turnaroundRad) {
			rPos = Random.insideUnitSphere * boxwormSpawnRad; // try again
		}
		return rPos;
	}

	public class BoxwormConfig {
		
		public int[] hits;
		
	}
	
}
