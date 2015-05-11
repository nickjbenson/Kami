using UnityEngine;
using System.Collections;

public class Hummingloop : Critter {

	// OBJECT HOOKS
	public Transform body;
	public Transform leftWing;
	public Transform rightWing;

	// ANIMATION VARIABLES
	public int hummingloop_idx = 0;
	public int pitchIndexOffset = 0;
	public float timeConstant = 1f;
	public float wingAngleToBodyFloatRatio = 0.001f;
	public float defaultWingAngle = 0f;
	public float defaultWingFrequencyMult = 1f;
	public float defaultWingAmplitude = 50f;
	public float hummingWingFrequencyMult = 16f;
	public float hummingWingAmplitude = 6f;
	private string anim_state = "idle";
	private float curPitchAngle = 0f;
	private double timeThisState = 0f;
	private double timeStateChanged = 0;
	private float curWingAngle = 0f;
	private float targetWingAngle = 0f;

	// ANIMATION CONFIGURATION VARIABLES
	private HummingloopConfig config;
	private int[] pitches;
	private int currentPitch = -1;
	private int middlePitch = 0;
	private int pitchRadius = 0;

	// MOVEMENT VARIABLES
	public float speed = 0.05f; // Movement speed towards target
	public float rotSpeed = 0.1f; // Rotation speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the hummingloop is leaving
	private bool targetWasReset = false;

	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;

	public int PitchIndex {
		get {
			return (hummingloop_idx + pitchIndexOffset) % pitches.Length;
		}
	}

	public override void CritterStart() {
		// Nothing needed.
	}

	public override AudioClip GetCritterAudio() {
		int idx = (int)Mathf.Ceil (Random.Range (3, 29));
		AudioClip clip = kami.GetHummingloopAudio(idx);

		// Animation configuration text parsing
		HummingloopConfig config = kami.GetHummingloopConfig (idx);
		pitches = config.pitches;
		middlePitch = config.middlePitch;
		pitchRadius = config.pitchRadius;

		return clip;
	}

	public override int GetCritterBeatsToLoop() {
		return 4;
	}
	
	// Called once per beat
	public override void OnCritterBeat() {

		// Track time and refresh the movement target.

		survivalTime -= 1;
		refreshTarget = true;
		
		// Look around if captured.
		
		if (Captured) {
			// Set the target to be something on the same plane as the hummingloop
			target = transform.position + new Vector3(Random.Range (-1f, 1f), 0, Random.Range (-1f, 1f));
			targetWasReset = true;
		}
	}

	public override void OnCritterSixteenth() {

		if (StartedPlaying) {
			hummingloop_idx += 1;
		} else {
			anim_state = "idle";
		}

		// Animation state logic
		if (hummingloop_idx < pitches.Length) {
			if (pitches [PitchIndex] != 0) {
				currentPitch = pitches[PitchIndex];
				if (currentPitch == -1) {
					anim_state = "idle";
					timeThisState = 0f;
					timeStateChanged = LivingTime;
				}
				else {
					timeThisState = 0f;
					timeStateChanged = LivingTime;
					anim_state = "humming";
					curPitchAngle = defaultWingAngle - defaultWingAmplitude * ((currentPitch - middlePitch) / (float)pitchRadius);
				}
			}
		}

		// Movement

		if (targetWasReset && leaving) {
			// Get a target far away
			target = Random.onUnitSphere * 200;
			targetWasReset = false;
		}
	}

	public override void OnCritterLoop() {
		hummingloop_idx = 0;
	}

	public override void OnCritterCapture() {

	}
	
	// Update is called once per frame
	public override void PostCritterUpdate () {

		// ******************
		// ANIMATION BEHAVIOR
		// ******************

		// Update time spent in the current state.
		timeThisState += LivingTime - timeStateChanged;

		// Update current wing angle.
		if (anim_state == "idle") {
			targetWingAngle = defaultWingAngle + defaultWingAmplitude
				* Mathf.Sin (Mathf.PI * (float)TimeSinceLoop * timeConstant * kami.globalTempo * defaultWingFrequencyMult);
		} else if (anim_state == "humming") {
			targetWingAngle = curPitchAngle + hummingWingAmplitude
				* Mathf.Sin (Mathf.PI * (float)TimeSinceLoop * timeConstant * kami.globalTempo * hummingWingFrequencyMult);
		}

		// Lerp curWingAngle to targetWingAngle.
		curWingAngle = Mathf.Lerp (curWingAngle, targetWingAngle, 0.5f);

		// Update wings based on current angle.
		leftWing.localRotation = Quaternion.Euler (curWingAngle, 0f, 0f);
		rightWing.localRotation = Quaternion.Euler (curWingAngle, 180f, 0f);

		// Update body based on current wing angle.
		body.localPosition = new Vector3 (0, 0 + curWingAngle * wingAngleToBodyFloatRatio, 0);

		// *****************
		// MOVEMENT BEHAVIOR
		// *****************

		if (BeingPulled || Captured) {
			// Movement logic while captured or being pulled.
			// TODO: Need anything here?

		} else {
			// Movement logic while not captured.

			// While not leaving, get new target whenever
			// we must refresh it
			if (!leaving) {
				if (refreshTarget) {
					target = getRandomSpawnLocation();
					refreshTarget = false;
				}
			}
			
			// If too close to player, turn around
			if (DistanceFromKami <= kami.turnaroundRad && !BeingPulled && survivalTime > 0) {
				target = (transform.position - kami.transform.position) + transform.position;
				targetWasReset = true;
			}
			
			// Leave if survivalTime is below zero
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
				// Get a target far away
				target = Random.onUnitSphere * 200;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}

			// Move forward at speed
			transform.position += transform.forward * speed;
		}
		
		// Smoothly rotate to target
		// Slerp to facing
		transform.rotation = Quaternion.Slerp(transform.rotation,
		                                      Quaternion.LookRotation (target - transform.position),
		                                      rotSpeed);

		// **************
		// DEATH BEHAVIOR
		// **************

		// For now, just die immediately when dying
		if (dying) {
			Destroy(this.gameObject);
		}
	}

	public override void OnCritterTooCloseToObject (Collider collider) {
		// Get new target away from the collider
		target = (transform.position - collider.transform.position) + transform.position;
		targetWasReset = true;
	}

	public override Vector3 getRandomSpawnLocation() {
		float maxSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * maxSpawnRad;
		while (rPos.sqrMagnitude < (kami.turnaroundRad+1) * (kami.turnaroundRad+1)) {
			rPos = Random.insideUnitSphere * maxSpawnRad; // try again
		}
		return rPos;
	}

	public class HummingloopConfig {
		
		public int[] pitches;
		public int middlePitch = 0;
		public int pitchRadius = 0;

	}
}
