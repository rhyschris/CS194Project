using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using AssemblyCSharp;

public class GameDelegate : MonoBehaviour {
	public float blockDamageModifier;
	private bool paused;
	// CONTROLLERS
	private CameraController mainCamera;
	private PlayerController player1;
	private PlayerController player2;
	private DebugTextController debugText;
	private Text winText;
	// KEYBOARD INPUTS
	private KeyCode Quit;
	private KeyCode Reset;
	private KeyCode TogglePause;
	private KeyCode ToggleDebugText;


	void Start ()
	{
		
		paused = false;
		GameObject mainCameraObj = GameObject.Find ("Camera");
		GameObject player1Obj = GameObject.Find ("Player1");
		GameObject player2Obj = GameObject.Find ("Player2");
		GameObject debugTextObj = GameObject.Find ("DebugText");
		GameObject winTextObj = GameObject.Find ("WinText");
		mainCamera = mainCameraObj.GetComponent<CameraController> ();
		player1 = player1Obj.GetComponent<PlayerController> ();
		player2 = player2Obj.GetComponent<PlayerController> ();
		debugText = debugTextObj.GetComponent<DebugTextController> ();
		winText = winTextObj.GetComponent<Text> ();
		Quit = KeyCode.Escape;
		Reset = KeyCode.Semicolon;
		TogglePause = KeyCode.BackQuote;
		ToggleDebugText = KeyCode.Quote;
		winText.text = "";

	}
	void Update()
	{
		// QUIT THE GAME
		if (Input.GetKeyDown (Quit)) {
			Application.Quit ();
		}
		// ENTER DEBUGGING MODE
		if (Input.GetKeyDown (TogglePause)) {
			paused = !paused;
			debugText.toggleDebugText();
		}
		if (paused) {
			// ALTER PERSPECTIVE CAMERA ANGLE ATTRIBUTES
			if (Input.GetKeyDown (mainCamera.getAngleMinus())) {
				mainCamera.modAngle (-0.5f);
			}
			if (Input.GetKeyDown (mainCamera.getXPaddingPlus())) {
				mainCamera.modXPadding (-0.5f);
			}
			if (Input.GetKeyDown (mainCamera.getAnglePlus())) {
				mainCamera.modAngle (0.5f);
			}
			if (Input.GetKeyDown (mainCamera.getXPaddingPlus())) {
				mainCamera.modXPadding (0.5f);
			}
			if (Input.GetKeyDown (mainCamera.getWidthMinimumMinus())) {
				mainCamera.modWidthMinimum (-0.5f);
			}
			if (Input.GetKeyDown (mainCamera.getWidthMinimumMinus())) {
				mainCamera.modWidthMinimum (0.5f);
			}
			// RESET PLAYER POSITIONS
			if (Input.GetKeyDown (Reset)) {
				// Later on, make it so this resets everything to default locations and statuses.
			}
		} else {
			if (Input.GetKeyDown (ToggleDebugText)) {
				debugText.toggleDebugText();
			}
			// UPDATE PLAYER STATUS
			player1.handleAutomaticUpdates (player2.getXPos ());
			player2.handleAutomaticUpdates (player1.getXPos ());
			// QUERY PLAYER INPUT
			Action player1Action = player1.queryInput (player2.getXPos ());
			Action player2Action = player2.queryInput (player1.getXPos ());
			// HANDLE PLAYER INPUT
			player1.handleInput (player1Action, player2Action);
			player2.handleInput (player2Action, player1Action);
			// DO HIT DETECTION
			handlePlayerHit (player1, player2);
			handlePlayerHit (player2, player1);
			debugText.setMessage (player1.getHealth(), player2.getHealth());
		}
	}
	private void handlePlayerHit(PlayerController attacker, PlayerController defender) {
		// If the attacker is engaged in an attack that needs to be handled:
		if (attacker.attackHandle ()) {
			// See if the attack box is in the body box.
			float distance = Mathf.Abs(defender.getXPos() - attacker.getHitXPos());
			distance = distance - attacker.getHitHalfWidth() - defender.getHalfWidth();
			if (distance <= 0.0f) {
				// This was a hit. Handle it.
				if((attacker.isHighAttack() && defender.isHighBlocking()) || (!attacker.isHighAttack() && defender.isLowBlocking())) {
					Debug.Log ("Blocked!");
					attacker.tellHit ();
					defender.receiveAttack (attacker.getAttackDamage () * blockDamageModifier, true);
				} else {
					Debug.Log ("Hit!");
					attacker.tellHit ();
					defender.receiveAttack (attacker.getAttackDamage (), false);
				}
				if(defender.getHealth() <= 0.0f) {
					winText.text = "Victory!";
				}
			}
		}
	}
}
