using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugTextController : MonoBehaviour {
	private Text debugText;
	private bool inDebugMode;
	private bool displayDebugText;
	void Start () {
		debugText = GameObject.Find ("DebugText").GetComponent<Text> ();
		inDebugMode = false;
		displayDebugText = false;
		debugText.text = "";
	}
	void LateUpdate () {
		if(inDebugMode) {
			// Set debug text to display in paused debug mode.
			debugText.text = "";
		} else if (displayDebugText) {
			// Set debug text to display in unpaused debug mode.
			debugText.text = "";
		}
	}
	public bool toggleDebugMode() {
		inDebugMode = !inDebugMode;
		debugText.text = "";
		return inDebugMode;
	}
	public void toggleDebugText() {
		displayDebugText = !displayDebugText;
		debugText.text = "";
	}
}
