using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

using AssemblyCSharp;


//TODO: on start, send AI initial game state
public class GameDelegate : MonoBehaviour {
	private bool paused;
	private bool firstTime;
	private bool gameOver;
	private bool wait_for_start;
	private float stageEnd_left;
	private float stageEnd_right;
	// CONTROLLERS
	private CameraController mainCamera;
	private HealthBarController healthbarcontroller;
	private PlayerController player1;
	private PlayerController player2;
	private MenuInfo MI_gd;
	private DebugTextController debugText;
	private Text winText;
	// KEYBOARD INPUTS
	private KeyCode Quit;
	private KeyCode start_game;
	private KeyCode TogglePause;
	private KeyCode ToggleDebugText;
	void Start ()
	{
		stageEnd_left = -30.0f;
		stageEnd_right = 30.0f;
		wait_for_start = true;
		firstTime = true;
		paused = false;
		gameOver = false;
		GameObject mainCameraObj = GameObject.Find ("Camera");
		GameObject healthBars = GameObject.Find ("HealthBars");
		GameObject player1Obj = GameObject.Find ("Player1");
		GameObject player2Obj = GameObject.Find ("Player2");
		GameObject debugTextObj = GameObject.Find ("DebugText");
		GameObject winTextObj = GameObject.Find ("WinText");
		GameObject MIObj = GameObject.Find ("Info");

		MI_gd = MIObj.GetComponent<MenuInfo> ();
		mainCamera = mainCameraObj.GetComponent<CameraController> ();
		healthbarcontroller = healthBars.GetComponent<HealthBarController> ();
		player1 = player1Obj.GetComponent<PlayerController> ();
		player2 = player2Obj.GetComponent<PlayerController> ();
		debugText = debugTextObj.GetComponent<DebugTextController> ();
		winText = winTextObj.GetComponent<Text> ();
		Quit = KeyCode.Escape;
		start_game = KeyCode.G;
		TogglePause = KeyCode.BackQuote;
		ToggleDebugText = KeyCode.Quote;
		winText.text = "";
		winText.color = Color.white;


	}
	void Update()
	{
		//Debug.Log ("Sending state at time " + Time.time.ToString ());
		// QUIT THE GAME
		if (gameOver){//Input.GetKeyDown (Quit)) {
			SceneManager.LoadScene ("Scenes/Menu");
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
				
		}
			else{
				if (Input.GetKeyDown (ToggleDebugText)) {
					debugText.toggleDebugText();
				}
				// UPDATE PLAYER STATUS
				player1.handleAutomaticUpdates (player2.getXPos ());
				player2.handleAutomaticUpdates (player1.getXPos ());
				//BUILD GAME STATE
				GameState state = createGameState();

				// QUERY PLAYER INPUT
				Action player1Action = player1.queryInput (state);
				Action player2Action = player2.queryInput (state);
				// HANDLE PLAYER INPUT
				player1.handleInput (player1Action, player2Action);
				player2.handleInput (player2Action, player1Action);
				if (player1.getXPos () <= stageEnd_left)
					player1.moveToX (stageEnd_left);
				if (player2.getXPos () >= stageEnd_right)
					player2.moveToX (stageEnd_right);
				if ((player1.getXPos () + player1.getHalfWidth() * 2) > player2.getXPos ())
					player1.moveToX (player2.getXPos () - player1.getHalfWidth () * 2);
				// DO HIT DETECTION
				handlePlayerHit (player1, player2, true);
				handlePlayerHit (player2, player1, false);
			}
			debugText.setMessage (player1.getHealth(), player2.getHealth());
		}

		private void handlePlayerHit(PlayerController attacker, PlayerController defender, bool player1Attacker) {
			// If the attacker is engaged in an attack that needs to be handled:
			if (attacker.attackHandle ()) {
				// See if the attack box is in the body box.
				float xdistance = Mathf.Abs(defender.getXPos() - attacker.getHitXPos());
				xdistance -= (attacker.getHitHalfWidth() + defender.getHalfWidth());
				float ydistance = attacker.getHitYPos()-attacker.getHitHalfHeight();
				ydistance -= (defender.getYPos()+defender.getHalfHeight());
				if (xdistance <= 0.0f && ydistance<=0.0f) {
					// This was a hit. Handle it.
					if((attacker.isHighAttack() && defender.isHighBlocking()) || (!attacker.isHighAttack() && defender.isLowBlocking())) {
						Debug.Log ("Blocked!");
						attacker.tellHit ();
						defender.receiveAttack (attacker.getAttackDamage (), true);
					} else {
						Debug.Log ("Hit!!");
						Debug.Log("Health: "+defender.getHealth().ToString());
						attacker.tellHit ();
						defender.receiveAttack (attacker.getAttackDamage (), false);


						// set animation for punch
						GameObject defenderFighter = defender.fighter;
						Animator defenderAnimator;
						defenderAnimator = defenderFighter.GetComponent<Animator> ();

						Debug.Log (attacker.lastAttackThrown());
						if (attacker.lastAttackThrown() == ActionType.attack1) {
							defenderAnimator.SetBool ("facePunched", true);
						}
						if (attacker.lastAttackThrown () == ActionType.attack2) {
							defenderAnimator.SetBool ("faceKicked", true);
						}
						if (attacker.lastAttackThrown () == ActionType.attack3) {
							Debug.Log ("shin kicked");
							defenderAnimator.SetBool ("shinKicked", true);
						}
						if (attacker.lastAttackThrown () == ActionType.attack4){
							defenderAnimator.SetBool("isTripped", true);
						}



					}
					healthbarcontroller.setPercent (player1Attacker, defender.getHealthPercent ());
					if(defender.getHealth() <= 0.0f) {
						GameObject defenderFighter = defender.fighter;
						Animator defenderAnimator;
						defenderAnimator = defenderFighter.GetComponent<Animator> ();
						defenderAnimator.SetBool ("lost_game", true);

						GameObject attackerFighter = attacker.fighter;
						Animator attackerAnimator;
						attackerAnimator = attackerFighter.GetComponent<Animator> ();
						attackerAnimator.SetBool ("won_game", true);

					winText.text = "Victory for "+(defender.player1?"player2!":"player1!")+ "\n PRESS ESC TO RETURN TO MENU";
						wait_for_start = true;
					gameOver = true;
					}
				}
			}
		}

		private GameState createGameState(){
			GameState state = new GameState (player1.getXPos(),player1.getYPos(),player2.getXPos(),
				player2.getYPos(),player1.getHealth(),player2.getHealth());

			bool p1attacking = false, p1high = false, p1blocking = false, p1crouching = false;

			if (player1.attackHandle ()) {
				p1attacking = true;
				p1high = player1.isHighAttack ();
			} else if (player1.isLowBlocking ()) {
				p1blocking = true;
			} else if (player1.isHighBlocking ()) {
				p1blocking = true;
				p1high = true;
			} else if (false) { //TODO: fill in when crouching implemented
				p1crouching = true;
			}

			bool p2attacking = false, p2high = false, p2blocking = false, p2crouching = false;
			if (player2.attackHandle ()) {
				p2attacking = true;
				p2high = player2.isHighAttack ();
			} else if (player2.isLowBlocking ()) {
				p2blocking = true;
			} else if (player2.isHighBlocking ()) {
				p2blocking = true;
				p2high = true;
			} else if (false) { //TODO: fill in when crouching implemented
				p2crouching = true;
			}

			state.setFlags (p1attacking, p1blocking, p1crouching, p1high, 
				p2attacking, p2blocking, p2crouching, p2high);

			return state;
		}
	}
