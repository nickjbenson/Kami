using UnityEngine;
using System.Collections;

public class DebrisSpawner : MonoBehaviour {

	public Transform debris1;
	public Transform debris2;
	public Transform debris3;
	private Transform[] debris;

	public float debrisDensity = 1.0f;

	public float minScale = 0.5f;
	public float maxScale = 2.0f;

	public float minSpawnRadius = 50f;
	public float maxSpawnRadius = 80f;

	void Start () {
		debris = new Transform[] {debris1, debris2, debris3};

		Transform tempOriginal;
		Vector3 tempScale;
		float tempRad;
		Vector3 tempLocation;
		Transform newDebris;

		float numDebris = 50 * debrisDensity;
		for (int i = 0; i < numDebris; i++) {
			tempOriginal = debris[(int)Random.Range (0, debris.Length)];
			tempScale = new Vector3(Random.Range (minScale, maxScale),
			                        Random.Range (minScale, maxScale),
			                        Random.Range (minScale, maxScale));
			tempRad = Random.Range (minSpawnRadius, maxSpawnRadius);
			tempLocation = Random.onUnitSphere * tempRad;
			newDebris = (Instantiate(tempOriginal, tempLocation, Quaternion.LookRotation(transform.position - tempLocation)) as Transform);
			newDebris.transform.parent = this.transform;
			Vector3 localS = newDebris.transform.localScale;
			newDebris.transform.localScale = new Vector3(localS.x * tempScale.x,
			                                             localS.y * tempScale.y,
			                                             localS.z * tempScale.z);
		}
	}

}
