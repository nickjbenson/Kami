using UnityEngine;
using System.Collections;

public class Critter : MonoBehaviour {

	// OBJECT HOOKS
	public Kami kami;

	// CAPTURE VARIABLES
	private bool beingCaptured = false;
	private bool captured = false;

	// CAPTURE FUNCTIONS / PROPERTIES
	public virtual void OnStartCapture () { }
	public void BeginCapturing() {
		if (!captured) {
			beingCaptured = true;
			OnStartCapture ();
		}
	}
	public virtual void OnStopCapture() { }
	public void StopCapturing() {
		if (!captured && beingCaptured) {
			beingCaptured = false;
			OnStopCapture ();
		}
	}
	public virtual void OnRelease() { }
	public void Release() {
		if (!captured && beingCaptured)
			StopCapturing ();
		else {
			print ("You released a critter you had captured!");
			kami.DeregisterCapture(this);
			OnRelease ();
		}
		beingCaptured = false;
		captured = false;
	}
	protected void FinalizeCapture() {
		kami.RegisterCapture (this);
		beingCaptured = false;
		captured = true;
	}
	public bool BeingCaptured {
		get {
			return beingCaptured;
		}
	}
	public bool Captured {
		get {
			return captured;
		}
	}



}
