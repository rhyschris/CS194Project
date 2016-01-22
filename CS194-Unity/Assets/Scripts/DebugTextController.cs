using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugTextController : MonoBehaviour {
	private Text debugText;
	private bool displayDebugText;
	private string message;
	void Start () {
		debugText = GameObject.Find ("DebugText").GetComponent<Text> ();
		displayDebugText = false;
		debugText.text = "";
	}
	void LateUpdate () {
		if (displayDebugText) {
			debugText.text = message;
		} else {
			debugText.text = "";
		}
	}
	public void toggleDebugText() {
		displayDebugText = !displayDebugText;
		debugText.text = "";
	}
	public void setMessage(float player1HP, float player2HP) {
		message =
			"Player1: " + player1HP.ToString () + "\n" +
			"Player2: " + player2HP.ToString () ;
	}
}
