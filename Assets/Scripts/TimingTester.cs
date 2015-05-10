using UnityEngine;
using System.Collections;

public class TimingTester : MonoBehaviour {

	public Kami kami;

	public double initTime = 0;
	public double myTime = 0;
	public double nextBeat;
	public double nextSixteenth;
	public int sixteenthCount = -1;

	public double KamiTime {
		get {
			return kami.DSPTime;
		}
	}
	public double BeatLength {
		get {
			return kami.globalTempo;
		}
	}
	public double SixteenthLength {
		get {
			return kami.globalTempo/8.0;
		}
	}

	// Use this for initialization
	void Start () {

		print ("I have been born at " + KamiTime);
		initTime = KamiTime;
		
		nextBeat = kami.NextBeat - initTime;
		nextSixteenth = kami.NextSixteenth - initTime;
		print ("Kami says next beat is at " + nextBeat);
		print ("Kami says the next sixteenth is at " + nextSixteenth);
	
	}
	
	// Update is called once per frame
	void Update () {

		myTime = KamiTime - initTime;

		if (myTime >= nextSixteenth) {
			nextSixteenth += SixteenthLength;
			sixteenthCount += 1;

			print ("Sixteenth " + sixteenthCount + ". Hit at " + myTime + ". Kami says the next sixteenth is at " + (kami.NextSixteenth - initTime)
			       + ", I think it's at " + nextSixteenth);

			if ((sixteenthCount + 4) % 8 == 0) {
				print ("This is where you should schedule the audio."
				       + " schedule it at the next beat, which is " + nextBeat);
				int bufferLength = 0, numBuffers = 0;
				AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
				print ("According to my calculations, the earliest time from now at which you can schedule is "
				       + ((bufferLength * numBuffers) / (float)AudioSettings.outputSampleRate) + " seconds from now");
			}
			
			if (sixteenthCount % 8 == 0) {
				nextBeat += BeatLength;
				sixteenthCount = 0;
				print ("Beat. Hit at " + myTime + ". Kami says the next beat is at " + (kami.NextBeat - initTime)
				       + ", I think it's at " + nextBeat);
			}
		}
	
	}
}
