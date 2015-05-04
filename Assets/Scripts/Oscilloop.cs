using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class Oscilloop : Critter {

	/// <summary>
	/// This aligns the movement of the Oscilloop
	/// with the movement of its audio. Don't touch
	/// unless you know what you're doing!
	/// </summary>
	public float timeConstant = 2f;
	private float initTime = 0f;
	public float curTime = 0f;
	
	public Transform inner1;
	public Transform inner2;
	public Transform inner3;
	public Transform inner4;
	public Transform inner5;
	public Transform inner6;
	public Transform inner7;
	public Transform inner8;
	public Transform[] innerRings;
	public Transform masterRing;
	public Transform egg;

	// OSCILLATION VARIABLES
	// normalization constant (divisor)
	private float norm_const = 0;
	private float[] freqs = new float[16];
	private float[] amps = new float[16];
	
	// BEAT TRACKING / LOOPING VARIABLES
	public int beatsToLoop = 64;
	private double nextBeatTime;
	AudioSource[] sources;
	private int soundIndex = 0;
	public int beatsSinceLastPlay = 0;

	// Use this for initialization
	void Start () {
		nextBeatTime = kami.getNextBeat ();
		initTime = (float)AudioSettings.dspTime;
		
		// AUDIO INITIALIZATION
		// Find audio file to play
		int idx = (int) Mathf.Ceil(Random.Range (1, 10));
		print ("Oscilloop, chose " + idx);
		// Load audio clip
		AudioClip clip = (AudioClip)Resources.Load ("Audio/osci_output" + idx);
		// Get AudioSource components (already in Prefab)
		sources = GetComponents<AudioSource> ();
		foreach (AudioSource source in sources) {
			source.clip = clip;
		}
		// Start looping on the next available beat
		beatsSinceLastPlay = beatsToLoop - 1;

		// OSCILLATION CONFIGURATION INITIALIZATION
		innerRings = new Transform[] {inner1, inner2, inner3, inner4, inner5, inner6, inner7, inner8};

		// Oscillation configuration text parsing
		TextAsset config = (TextAsset)Resources.Load ("Audio/osci_output" + idx + "_config");
		print (config.text);
		var result = Regex.Split(config.text, "\r\n|\r|\n");
		int i = 0;
		foreach (string line in result) {
			if (!string.IsNullOrEmpty(line)) {
				if (!line.Contains (" ")) {
					// Normalization constant (first line of file)
					norm_const = float.Parse (line);
				}
				else {
					// Format: "{freq} {amp}\n"
					string[] elems = line.Split (' ');
					freqs[i] = float.Parse (elems[0]);
					amps[i] = float.Parse (elems[1]);
					i++;
				}
			}
		}
		// Apply normalization constant (divisor) to amplitudes
		for (i = 0; i < 16; i++) {
			amps[i] /= norm_const;
		}
	}
	
	// Update is called once per frame
	void Update () {

		// *********************
		// INFORMATION GATHERING
		// *********************


		curTime = (float)AudioSettings.dspTime - initTime;

		// **********************
		// BEAT-COUNTING BEHAVIOR
		// **********************
		
		// Get a new target every beat
		if (nextBeatTime <= AudioSettings.dspTime) {
			nextBeatTime = kami.getNextBeat();
			beatsSinceLastPlay += 1;
			print ("beat.");
		}

		// ****************
		// RING OSCILLATION
		// ****************

		for (int i = 0; i < 8; i++) {
			Transform ring = innerRings[i];

			// There are 16 frequency oscillations and 8 rings,
			// so here we sum 2 frequencies per ring.
			Vector3 newPosition = new Vector3(0, Mathf.Sin(Mathf.PI * curTime * timeConstant * freqs[i*2]), 0) * amps[i*2];
			newPosition += new Vector3(0, Mathf.Sin (Mathf.PI * curTime * timeConstant * freqs[i*2 + 1]), 0) * amps[i*2 + 1];

			ring.localPosition = newPosition;
		}

		masterRing.localPosition = new Vector3 (0, Mathf.Sin (0.025f * curTime * timeConstant), 0);

		// egg.localPosition = new Vector3 (0, 0.1f * Mathf.Sin ((float)AudioSettings.dspTime * timeConstant), 0);

		// Finally, play beautiful sounds!
		playSound ();
	}
	
	void playSound() {
		if (beatsSinceLastPlay >= beatsToLoop) {
			sources[soundIndex].PlayScheduled(nextBeatTime);
			soundIndex = (soundIndex + 1)%sources.Length;
			beatsSinceLastPlay = 0;
		}
	}
}
