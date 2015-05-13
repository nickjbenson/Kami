using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpawnType {
	TREETOPS,
	HILLS,
	TREETOPS_OR_HILLS,
	DISTANCE
}

public class CritterSpawnData {

	public Critter[] critters;
	public Vector3[] positions;

}

public class Spawn {
	
	public EncounterSpawner spawner;

	public Critter[] critters;
	public int[] spawnArray;
	public SpawnType spawnType;

	private List<int> possibleCritterAmounts;
	private Vector3[] possibleSpawnLocations;

	/// <summary>
	/// Defines a single critter-spawning event by specifying possible
	/// critters to be spawned, how many to be spawned, and where to spawn them.
	/// </summary>
	/// <param name="critters">Possible critters to choose from.</param>
	/// <param name="spawnArray">This array is important and a bit weird,
	/// because C# sucks at convenient data structures. The even indices of this
	/// array determine possibilities for HOW MANY critters to spawn
	/// when this spawn is invoked. The odd indices correspond to
	/// the RANDOM WEIGHTING to apply to the even-index values.
	/// For example, [1, 1, 2, 2, 3, 1] specifies a random distribution
	/// that will choose to spawn 1 critter 25% of the time, 2 critters
	/// 50% of the time, and 3 critters 25% of the time.</param>
	/// <param name="spawnType">Spawn type specifying which of the
	/// possible spawn groups will be used to randomly choose a spawn point.</param>
	public Spawn(EncounterSpawner spawner, Critter[] critters, int[] spawnArray, SpawnType spawnType) {
		this.critters = critters;
		this.spawnArray = spawnArray;
		this.spawnType = spawnType;

		// Populate possible critter amounts
		possibleCritterAmounts = new List<int> ();

		int amount = 0;
		int weight = 0;
		for (int i = 0; i < spawnArray.Length; i++) {
			// Even indices are amounts
			amount = spawnArray[i];
			// Odd indices are weights
			i++;
			weight = spawnArray[i]; 
			for (int j = 0; j < weight; j++) {
				possibleCritterAmounts.Add(amount);
			}
		}
		//EncounterSpawner.print ("Possible critter amounts: " + possibleCritterAmounts);

		// Populate possible spawn locations
		possibleSpawnLocations = spawner.GetSpawnLocations (spawnType);
		//EncounterSpawner.print ("Possible spawn locations: " + possibleSpawnLocations);

	}

	/// <summary>
	/// Evaluates the spawn based on its configuration and returns
	/// the data necessary to spawn critters at a certain locations.
	/// </summary>
	public CritterSpawnData SpawnCritters() {

		// Choose number of critters to spawn.
		int numCritters = possibleCritterAmounts [Random.Range (0, possibleCritterAmounts.Count)];

		// Choose which critters to spawn and where to spawn them.
		Critter[] critters = new Critter[numCritters];
		Vector3 position = possibleSpawnLocations [Random.Range (0, possibleSpawnLocations.Length)];
		Vector3[] positions = new Vector3[numCritters];
		for (int i = 0; i < numCritters; i++) {
			critters[i] = this.critters[Random.Range (0, this.critters.Length)];
			positions[i] = position;
		}

		// Construct the spawn data to return
		CritterSpawnData spawnData = new CritterSpawnData ();
		spawnData.critters = critters;
		spawnData.positions = positions;

		return spawnData;

	}

}

public class EncounterSpawner : MonoBehaviour {

	// OBJECT HOOKS
	public Kami kami; // for timing and critter types
	public PaletteChanger paletteChanger; // for knowing what the current palette is
	public List<Transform> treetopSpawnLocations;
	public List<Transform> hillSpawnLocations;
	public List<Transform> distantSpawnLocations;

	// SPAWN / ENCOUNTER DATA
	public Dictionary<string, List<Spawn>> paletteDict = new Dictionary<string, List<Spawn>>();
	public List<int> possibleMeasureWaitTimes;
	private int measuresSinceSpawn = -1;
	private int measuresToWait = 0;

	// BEAT TRACKING / AUDIO LOOPING VARIABLES
	public double initTime = 0;
	public double myTime = 0;
	public double nextMeasure;
	public double nextBeat;
	public double nextSixteenth;
	public int sixteenthCount = -1;
	public int beatCount = -1;
	public int measureCount = -1;

	public double KamiTime {
		get {
			return kami.DSPTime;
		}
	}
	public double MeasureLength {
		get {
			return kami.globalTempo * 4;
		}
	}
	public double BeatLength {
		get {
			return kami.globalTempo;
		}
	}
	public double SixteenthLength {
		get {
			return kami.globalTempo / 8.0;
		}
	}

	void Start () {


		// *******************
		// ENCOUNTERS / SPAWNS
		// *******************
		// Initialize all possible palette spawns.
		
		Spawn tempS;

		// *************
		// Purple Spring
		// *************
		List<Spawn> purpleSpringSpawns = new List<Spawn>();
		// a few (1-3) hummingloops
		tempS = new Spawn(this,
		                  new Critter[] {kami.hummingloop.GetComponent<Critter>()},
						  new int[] {1, 1, 2, 2, 3, 1},
		 				  SpawnType.TREETOPS_OR_HILLS);
		purpleSpringSpawns.Add (tempS);
		// a few (1-3) maracaws
		tempS = new Spawn(this,
		                  new Critter[] {kami.maracaw.GetComponent<Critter>()},
						  new int[] {1, 2, 2, 4, 3, 1, 4, 1},
		SpawnType.TREETOPS_OR_HILLS);
		purpleSpringSpawns.Add (tempS);
		// some boxworms from the distance
		tempS = new Spawn(this,
		                  new Critter[] {kami.boxworm.GetComponent<Critter>()},
						  new int[] {1, 1, 2, 1},
						  SpawnType.DISTANCE);
		purpleSpringSpawns.Add (tempS);

		// Finalize Purple Spring spawns
		paletteDict.Add ("purple spring", purpleSpringSpawns);

		// ************
		// Dazzle Night
		// ************
		List<Spawn> dazzleNightSpawns = new List<Spawn> ();
		// some boxworms from the distance
		tempS = new Spawn(this,
		                  	new Critter[] {kami.boxworm.GetComponent<Critter>()},
							new int[] {1, 1, 2, 3, 3, 2, 4, 1},
							SpawnType.DISTANCE);
		dazzleNightSpawns.Add (tempS);
		// an oscilloop from the distance
		tempS = new Spawn(this,
		                  	new Critter[] {kami.oscilloop.GetComponent<Critter>()},
							new int[] {1, 1},
							SpawnType.DISTANCE);
		dazzleNightSpawns.Add (tempS);
		// some maracaws from the hills
		tempS = new Spawn(this,
		                  new Critter[] {kami.maracaw.GetComponent<Critter>()},
						  new int[] {1, 1, 2, 2, 3, 1},
						  SpawnType.TREETOPS_OR_HILLS);
		dazzleNightSpawns.Add (tempS);
		// some MORE maracaws from the hills (makes oscilloops rarer)
		tempS = new Spawn(this,
		                  new Critter[] {kami.maracaw.GetComponent<Critter>()},
							new int[] {1, 1, 2, 2, 3, 1},
							SpawnType.TREETOPS_OR_HILLS);
		dazzleNightSpawns.Add (tempS);
		// some MORE boxworms from the distance (makes oscilloops rarer)
		tempS = new Spawn(this,
		                  new Critter[] {kami.boxworm.GetComponent<Critter>()},
		new int[] {1, 1, 2, 3, 3, 2, 4, 1},
		SpawnType.DISTANCE);
		dazzleNightSpawns.Add (tempS);

		// Finalize Dazzle Night spawns
		paletteDict.Add ("dazzle night", dazzleNightSpawns);

		// ***********
		// Sunset Daze
		// ***********
		List<Spawn> sunsetDazeSpawns = new List<Spawn> ();
		// 1-2 hummingloops in the treetops/hills
		tempS = new Spawn(this,
		                  new Critter[] {kami.hummingloop.GetComponent<Critter>()},
							new int[] {1, 1, 2, 1},
							SpawnType.TREETOPS_OR_HILLS);
		purpleSpringSpawns.Add (tempS);
		// 1-2 MORE hummingloops in the treetops/hills (oscilloops rarer)
		tempS = new Spawn(this,
		                  new Critter[] {kami.hummingloop.GetComponent<Critter>()},
		new int[] {1, 1, 2, 1},
		SpawnType.TREETOPS_OR_HILLS);
		purpleSpringSpawns.Add (tempS);
		// 1-2 MORE hummingloops in the treetops/hills (oscilloops rarer)
		tempS = new Spawn(this,
		                  new Critter[] {kami.hummingloop.GetComponent<Critter>()},
		new int[] {1, 1, 2, 1},
		SpawnType.TREETOPS_OR_HILLS);
		purpleSpringSpawns.Add (tempS);
		// an oscilloop from the distance
		tempS = new Spawn(this,
		                  new Critter[] {kami.oscilloop.GetComponent<Critter>()},
		new int[] {1, 1},
		SpawnType.DISTANCE);
		dazzleNightSpawns.Add (tempS);
		// some mines in the hills
		tempS = new Spawn (this,
		                   new Critter[] {kami.mine.GetComponent<Critter> ()},
		new int[] {1, 5, 2, 3, 3, 1},
		SpawnType.HILLS);
		sunsetDazeSpawns.Add(tempS);
		// some MORE mines in the hills (rarer oscilloops)
		tempS = new Spawn (this,
		                   new Critter[] {kami.mine.GetComponent<Critter> ()},
		new int[] {1, 5, 2, 3, 3, 1},
		SpawnType.HILLS);
		sunsetDazeSpawns.Add(tempS);
		// some MORE mines in the hills (rarer oscilloops)
		tempS = new Spawn (this,
		                   new Critter[] {kami.mine.GetComponent<Critter> ()},
		new int[] {1, 5, 2, 3, 3, 1},
		SpawnType.HILLS);
		sunsetDazeSpawns.Add(tempS);

		// Finalize Sunset Daze
		paletteDict.Add ("sunset daze", sunsetDazeSpawns);

		// Spawn measure waiting time default initialization.
		if (possibleMeasureWaitTimes.Count == 0) {
			possibleMeasureWaitTimes = new List<int>() {1, 2, 4};
		}
		measuresToWait = possibleMeasureWaitTimes [Random.Range (0, possibleMeasureWaitTimes.Count)];

		// Animation time initialization.
		initTime = (float)AudioSettings.dspTime;
		
		// ***************************
		// AUDIO TIMING INITIALIZATION
		// ***************************
		// Shamelessly copied from Critter
		
		initTime = KamiTime;
		
		nextBeat = kami.NextBeat - initTime;
		nextSixteenth = kami.NextSixteenth - initTime;
		nextMeasure = kami.NextMeasure - initTime;
		
		int numSixteenthsLeft = (int)(nextBeat / SixteenthLength);
		//		print ("This implies there are " + numSixteenthsLeft + " sixteenths left until the beat.");
		sixteenthCount = 7 - numSixteenthsLeft;
		int numBeatsLeft = (int)(nextMeasure / BeatLength);
		//		print ("It also implies that there are " + numBeatsLeft + " beats left until the measure.");
		beatCount = 3 - numBeatsLeft;
	
	}
	
	// Update is called once per frame
	void Update () {

		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		// Blatantly copied from Critter

		myTime = KamiTime - initTime;
		
		if (myTime >= nextSixteenth) {
			nextSixteenth += SixteenthLength;
			sixteenthCount += 1;
			
			// Sixteenth note.
			
			if (sixteenthCount % 8 == 0) { // Beat.
				nextBeat += BeatLength;
				sixteenthCount = 0;
				beatCount += 1;
				
				// Beat (quarter note).
				
				if (beatCount % 4 == 0) { // Measure.
					nextMeasure += MeasureLength;
					beatCount = 0;
					measureCount += 1;

					// Measure.
					OnMeasure();
				}
			}
		}
		
	}

	public void OnMeasure() {

		// ***************************
		// ENCOUNTER SPAWNING BEHAVIOR
		// ***************************
		// Logic performed every measure.

		measuresSinceSpawn += 1;

		if (measuresSinceSpawn == measuresToWait) {

			// Do a spawn based on the current palette.
			DoRandomSpawn(paletteChanger.currentPalette);

			if (measuresToWait == 4) {
				// If we waited 4 measures, spawn one more time.
				DoRandomSpawn(paletteChanger.currentPalette);
			}

			// Reset the measure spawn counter.
			measuresSinceSpawn = 0;
			// Randomly set measuresToWait.
			measuresToWait = possibleMeasureWaitTimes [Random.Range (0, possibleMeasureWaitTimes.Count)];
		}

	}

	public void DoRandomSpawn(string currentPalette) {

		List<Spawn> possibleSpawns = paletteDict [currentPalette];
		
		Spawn toSpawn = possibleSpawns [Random.Range (0, possibleSpawns.Count)];
		CritterSpawnData spawnData = toSpawn.SpawnCritters ();
		
		for (int i = 0; i < spawnData.critters.Length; i++) {
			kami.spawnCritter (spawnData.critters[i], spawnData.positions[i]);
		}

	}

	public Vector3[] GetSpawnLocations(SpawnType type) {

		Vector3[] toReturn;
		List<Transform> spawnTransforms = new List<Transform>();

		switch (type) {
		case SpawnType.TREETOPS:
			spawnTransforms = treetopSpawnLocations;
			break;
		case SpawnType.HILLS:
			spawnTransforms = hillSpawnLocations;
			break;
		case SpawnType.TREETOPS_OR_HILLS:
			foreach (Transform t in treetopSpawnLocations) {
				spawnTransforms.Add (t);
			}
			foreach (Transform t in hillSpawnLocations) {
				spawnTransforms.Add (t);
			}
			break;
		case SpawnType.DISTANCE:
			spawnTransforms = distantSpawnLocations;
			break;
		default:
			spawnTransforms = treetopSpawnLocations;
			break;
		}

		toReturn = new Vector3[spawnTransforms.Count];
		for (int i = 0; i < toReturn.Length; i++) {
			toReturn[i] = spawnTransforms[i].position;
		}

		return toReturn;

	}

}
