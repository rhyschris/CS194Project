using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameDelegate : MonoBehaviour {
	private bool debugging;
	private bool displayDebugText;
	public GameObject player1Obj;
	public GameObject player2Obj;
	private PlayerController player1;
	private PlayerController player2;
	public Text debugText;
	public Camera perspCamera;
	public float perspXPadding;
	public float perspAngle;
	public float perspWidthMinimum;
	void Start ()
	{
		debugging = false;
		displayDebugText = false;
		debugText.text = "";
		perspCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
		player1 = player1Obj.GetComponent<PlayerController> ();
		player2 = player2Obj.GetComponent<PlayerController> ();
	}
	void Update()
	{
		// QUIT THE GAME
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		// ENTER DEBUGGING MODE
		if (Input.GetKeyDown (KeyCode.BackQuote)) {
			debugging = !debugging;
			if (!debugging) {
				debugText.text = "";
			}
		}
		if (debugging) {
			// ALTER PERSPECTIVE CAMERA ANGLE ATTRIBUTES
			if (Input.GetKeyDown (KeyCode.I)) {
				perspAngle = perspAngle - 0.5f;
				perspCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
			}
			if (Input.GetKeyDown (KeyCode.J)) {
				perspXPadding = perspXPadding - 0.5f;
			}
			if (Input.GetKeyDown (KeyCode.K)) {
				perspAngle = perspAngle + 0.5f;
				perspCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
			}
			if (Input.GetKeyDown (KeyCode.L)) {
				perspXPadding = perspXPadding + 0.5f;
			}
			if (Input.GetKeyDown (KeyCode.Comma)) {
				perspWidthMinimum = perspWidthMinimum - 0.5f;
			}
			if (Input.GetKeyDown (KeyCode.Period)) {
				perspWidthMinimum = perspWidthMinimum + 0.5f;
			}
			// RESET PLAYER POSITIONS
			if (Input.GetKeyDown (KeyCode.Semicolon)) {
				// Later on, make it so this resets everything to default locations and statuses.
			}
			debugText.text =
				"perspAngle: " + perspAngle.ToString () +
				"\nperspXPadding: " + perspXPadding.ToString () +
				"\nperspWidthMinimum: " + perspWidthMinimum.ToString ();
		} else {
			if (Input.GetKeyDown (KeyCode.Quote)) {
				displayDebugText = !displayDebugText;
				if (!displayDebugText) {
					debugText.text = "";
				}
			}
			// UPDATE PLAYER STATUS
			player1.handleAutomaticUpdates (player2.getXPos ());
			player2.handleAutomaticUpdates (player1.getXPos ());
			// QUERY PLAYER INPUT
			player1.queryInput (player2.getXPos ());
			player2.queryInput (player1.getXPos ());
			// HANDLE PLAYER INPUT
			player1.handleInput (player2.getOldXPos (), player2.getDistanceMoved (), player2.getIsMovingAway());
			player2.handleInput (player1.getOldXPos (), player1.getDistanceMoved (), player1.getIsMovingAway());
			// DO HIT DETECTION
			handlePlayerHit (player1, player2);
			handlePlayerHit (player2, player1);
			if (displayDebugText) {
				debugText.text =
					"Player1X: " + player1.getXPos ().ToString () + " Player2X: " + player2.getXPos ().ToString () +
					"\nPlayer1Moving: " + player1.getIsMoving ().ToString () + " Player2Moving: " + player2.getIsMoving ().ToString () +
					"\nPlayer1Away: " + player1.getIsMovingAway ().ToString () + " Player2Away: " + player2.getIsMovingAway ().ToString ();
			}
		}
	}
	void LateUpdate()
	{
		float xTransformRad = perspCamera.transform.eulerAngles.x * Mathf.Deg2Rad;
		float yheight = (player1.getYPos() + player2.getYPos()) * 0.5f;
		float xwidth = Mathf.Abs(player2.getXPos() - player1.getXPos()) + perspXPadding;
		xwidth = Mathf.Max (xwidth, perspWidthMinimum);
		float hFOVRad = 2.0f * Mathf.Atan (Mathf.Tan (perspCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * perspCamera.aspect);
		float distance = (0.5f * xwidth) / Mathf.Tan (hFOVRad * 0.5f);
		float yAboveHeight = distance * Mathf.Sin (xTransformRad);
		float zAbsolute = distance * Mathf.Cos (xTransformRad);
		float x = (player1.getXPos() + player2.getXPos()) * 0.5f;
		float y = yAboveHeight + yheight;
		float z = zAbsolute * -1.0f;
		perspCamera.transform.position = new Vector3 (x, y, z);
	}
	void handlePlayerHit(PlayerController attacker, PlayerController defender) {
		// If the attacker is engaged in an attack that needs to be handled:
		if (attacker.attackHandle ()) {
			// See if the attack box is in the body box.
			float distance = Mathf.Abs(defender.getXPos() - attacker.getHitXPos());
			distance = distance - attacker.getHitHalfWidth() - defender.getHalfWidth();
			if (distance <= 0.0f) {
				// This was a hit. Handle it.
				Debug.Log("Hit!");
				attacker.tellHit();
				defender.receiveAttack (attacker.getAttackDamage ());
			}
		}
	}
}
