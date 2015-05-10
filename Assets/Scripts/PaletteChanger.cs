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

	// This is the camera that renders the background color
	public UnityStandardAssets.ImageEffects.EdgeDetection edgeDetectionCamera;


	public void Start() {

		// Initialize current palette colors.
		curPlatformColor = platformMat.color;
		curPondColor = pondMat.color;
		curIslandColor = islandMat.color;
		curTreeTrunkColor = treeTrunkMat.color;
		curTreeFoliageColor = treeFoliageMat.color;
		curCloudColor = cloudMat.color;
		curBGColor = edgeDetectionCamera.edgesOnlyBgColor;

	}

	public void Update() {

		// Only do things if colors are currently changing
		if (colorChanging) {

			// Update time since the change.

			// S

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

	}
	
}
