using UnityEngine;
using System.Collections;

public class PaletteChanger : MonoBehaviour {
	
	// OBJECT HOOKS
	public Material platformMat;
	public Material pondMat;
	public Material islandMat;
	public Material treeTrunkMat;
	public Material treeFoliageMat;
	public Material cloudMat;
	public Material bgMat;

	// WORLD PALETTE MODE
	public string currentPalette = "purple spring";

	// COLOR PALETTES
	// ps - Purple Spring
	public Color psPlatformColor;
	public Color psPondColor;
	public Color psIslandColor;
	public Color psTreeTrunkColor;
	public Color psTreeFoliageColor;
	public Color psCloudColor;
	public Color psBGColor;
	// dn - Dazzle Night
	public Color dnPlatformColor;
	public Color dnPondColor;
	public Color dnIslandColor;
	public Color dnTreeTrunkColor;
	public Color dnTreeFoliageColor;
	public Color dnCloudColor;
	public Color dnBGColor;
	// sd - Sunset Daze
	public Color sdPlatformColor;
	public Color sdPondColor;
	public Color sdIslandColor;
	public Color sdTreeTrunkColor;
	public Color sdTreeFoliageColor;
	public Color sdCloudColor;
	public Color sdBGColor;
	
	// COLOR ANIMATION
	private Color oldPlatformColor;
	private Color oldPondColor;
	private Color oldIslandColor;
	private Color oldTreeTrunkColor;
	private Color oldTreeFoliageColor;
	private Color oldCloudColor;
	private Color oldBGColor;
	private Color targetPlatformColor;
	private Color targetPondColor;
	private Color targetIslandColor;
	private Color targetTreeTrunkColor;
	private Color targetTreeFoliageColor;
	private Color targetCloudColor;
	private Color targetBGColor;
	private Color curPlatformColor;
	private Color curPondColor;
	private Color curIslandColor;
	private Color curTreeTrunkColor;
	private Color curTreeFoliageColor;
	private Color curCloudColor;
	private Color curBGColor;
	private bool colorChanging;
	private float timeSinceChange = 0f;
	public float paletteChangeSpeed = 1f;
	private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

	public void Start() {

		// Initialize palette colors.
		oldPlatformColor = curPlatformColor = psPlatformColor;
		oldPondColor = curPondColor = psPondColor;
		oldIslandColor = curIslandColor = psIslandColor;
		oldTreeTrunkColor = curTreeTrunkColor = psTreeTrunkColor;
		oldTreeFoliageColor = curTreeFoliageColor = psTreeFoliageColor;
		oldCloudColor = curCloudColor = psCloudColor;
		oldBGColor = curBGColor = psBGColor;

		// Initialize actual colors based on current color variables
		platformMat.color = curPlatformColor;
		pondMat.color = curPondColor;
		islandMat.color = curIslandColor;
		treeTrunkMat.color = curTreeTrunkColor;
		treeFoliageMat.color = curTreeFoliageColor;
		cloudMat.color = curCloudColor;
		bgMat.color = curBGColor;

	}

	public void Update() {

		// DEBUG COLOR CHANGE COMMANDS

		if (Input.GetKeyDown ("d")) {
			ChangePalette("dazzle night");
		}
		if (Input.GetKeyDown ("s")) {
			ChangePalette("sunset daze");
		}
		if (Input.GetKeyDown ("p")) {
			ChangePalette("purple spring");
		}

		// Only do things if colors are currently changing
		if (colorChanging) {

			// Update time since the change.
			timeSinceChange += Time.deltaTime;

			// Get animation easing time.
			var t = ease.Evaluate(timeSinceChange * paletteChangeSpeed);

			// Lerp current colors to target colors.
			curPlatformColor = Color.Lerp (oldPlatformColor, targetPlatformColor, t);
			curPondColor = Color.Lerp (oldPondColor, targetPondColor, t);
			curIslandColor = Color.Lerp (oldIslandColor, targetIslandColor, t);
			curTreeTrunkColor = Color.Lerp (oldTreeTrunkColor, targetTreeTrunkColor, t);
			curTreeFoliageColor = Color.Lerp (oldTreeFoliageColor, targetTreeFoliageColor, t);
			curCloudColor = Color.Lerp (oldCloudColor, targetCloudColor, t);
			curBGColor = Color.Lerp (oldBGColor, targetBGColor, t);

			// Actually update the colors based on current color variables
			platformMat.color = curPlatformColor;
			pondMat.color = curPondColor;
			islandMat.color = curIslandColor;
			treeTrunkMat.color = curTreeTrunkColor;
			treeFoliageMat.color = curTreeFoliageColor;
			cloudMat.color = curCloudColor;
			bgMat.color = curBGColor;

			// If enough time has passed, disable colorChanging
			if (t == 1) {
				colorChanging = false;
			}
		}

	}

	public void ChangePalette(string palette) {

		if (palette != currentPalette) {
			currentPalette = palette;
			colorChanging = true;
			timeSinceChange = 0f;
			SetTargetPalette(palette);
		}

	}

	private void SetTargetPalette(string palette) {

		// Set old colors for referencing for a smooth color transition.
		oldPlatformColor = curPlatformColor;
		oldPondColor = curPondColor;
		oldIslandColor = curIslandColor;
		oldTreeTrunkColor = curTreeTrunkColor;
		oldTreeFoliageColor = curTreeFoliageColor;
		oldCloudColor = curCloudColor;
		oldBGColor = curBGColor;

		if (palette == "purple spring") {
			targetPlatformColor = psPlatformColor;
			targetPondColor = psPondColor;
			targetIslandColor = psIslandColor;
			targetTreeTrunkColor = psTreeTrunkColor;
			targetTreeFoliageColor = psTreeFoliageColor;
			targetCloudColor = psCloudColor;
			targetBGColor = psBGColor;
		} else if (palette == "dazzle night") {
			targetPlatformColor = dnPlatformColor;
			targetPondColor = dnPondColor;
			targetIslandColor = dnIslandColor;
			targetTreeTrunkColor = dnTreeTrunkColor;
			targetTreeFoliageColor = dnTreeFoliageColor;
			targetCloudColor = dnCloudColor;
			targetBGColor = dnBGColor;
		} else if (palette == "sunset daze") {
			targetPlatformColor = sdPlatformColor;
			targetPondColor = sdPondColor;
			targetIslandColor = sdIslandColor;
			targetTreeTrunkColor = sdTreeTrunkColor;
			targetTreeFoliageColor = sdTreeFoliageColor;
			targetCloudColor = sdCloudColor;
			targetBGColor = sdBGColor;
		}
	}
	
}
