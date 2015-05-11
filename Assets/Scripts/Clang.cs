using UnityEngine;
using System.Collections;

public class Clang : Critter {
	
	// ANIMATION VARIABLES
	public int state = 0; //0 = inactive, 1 = clanging, -1 = de-clanging
	public float animSpeed;
	private float dist = 0;
	public Transform top;
	public Transform bottom;
	private Vector3 base_top_pos;
	
	// MOVEMENT VARIABLES
	public float speed = 0.01f; // Movement speed towards target
	private Vector3 target; // target destination
	private bool refreshTarget = true; // whether we should get a new target
	private bool leaving = false; // whether or not the mine is leaving
	
	// LIFE/DEATH VARIABLES
	public int survivalTime = 24;
	private bool dying = false;
	
	public override void CritterStart () {
		base_top_pos = top.position;
	}
	
	public override AudioClip GetCritterAudio() {
		int idx = (int) Mathf.Ceil(Random.Range (1, 8));
		AudioClip clip = kami.GetMineAudio(idx);
		//TODO: replace with actual clip
		//TODO: map color to pitch, using config
		return clip;
	}
	
	public override int GetCritterBeatsToLoop() {
		return 4;
	}
	
	public override void OnCritterBeat() {
		survivalTime -= 1;
		// TODO: check config to set active
	}
	
	public override void PostCritterUpdate() {
		
		// TODO: get rid of this once you've implemented beat matching
		if (Input.GetKey ("z")) {
			state = 1;
		}
		
		// *****************
		// MOVEMENT BEHAVIOR
		// *****************
		
		if (BeingPulled || Captured) {
			// Movement logic while captured or being pulled.
			leaving = false;
		} else {
			// Movement logic while not captured.
			
			// While not leaving, get new target whenever
			// we must refresh it
			if (Vector3.Distance(transform.position, target) < 0.1){
				refreshTarget = true;
			}
			
			if (!leaving) {
				if (refreshTarget) {
					target = getRandomSpawnLocation();
					refreshTarget = false;
				}
			}
			
			// If too close to player, turn around
			if (DistanceFromKami <= kami.turnaroundRad && !BeingPulled && survivalTime > 0) {
				target = (transform.position - kami.transform.position) + transform.position;
			}
			
			// Leave if survivalTime is below zero
			if (survivalTime <= 0 && !leaving) {
				leaving = true;
				// Get a target far away
				target = Random.onUnitSphere * 200;
				speed = 0.05f;
			}
			
			// Start dying past the death radius
			if (DistanceFromKami > kami.deathRadius) {
				dying = true;
			}
			
			// Move forward at speed
			transform.position += transform.forward * speed;
			base_top_pos += transform.forward * speed;
		}
		
		// *****************
		// SPINNING BEHAVIOR
		// *****************
		
		if (state == 1) {
//			top.position = Vector3.Lerp(top.position, clang_pos, animSpeed);
//			bottom.position = Vector3.Lerp(bottom.position, clang_pos, animSpeed);
			top.position += Vector3.down/100f * animSpeed;
			bottom.position += Vector3.up/100f * animSpeed;
			if (Vector3.Distance(top.position, base_top_pos) >= 0.16){
				state = -1;
			}
		} else if (state == -1){
//			top.position = Vector3.Lerp(top.position, base_top_pos, animSpeed);
//			bottom.position = Vector3.Lerp(bottom.position, base_bottom_pos, animSpeed);
			top.position += Vector3.up/100f * animSpeed;
			bottom.position += Vector3.down/100f * animSpeed;
			if (Vector3.Distance(top.position, base_top_pos) <= 0.01){
				state = 0;
			}
		}
		
		// **************
		// DEATH BEHAVIOR
		// **************
		
		// For now, just die immediately when dying
		if (dying) {
			Destroy(this.gameObject);
		}
		
	}
	
	public override Vector3 getRandomSpawnLocation() {
		float maxSpawnRad = kami.deathRadius - 5f;
		Vector3 rPos = Random.insideUnitSphere * maxSpawnRad;
		while (rPos.sqrMagnitude < (kami.turnaroundRad+1) * (kami.turnaroundRad+1)) {
			rPos = Random.insideUnitSphere * maxSpawnRad; // try again
		}
		return rPos;
	}
}
