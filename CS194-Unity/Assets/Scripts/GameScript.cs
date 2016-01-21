using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameScript : MonoBehaviour {
	private bool debugging;
	private bool displayDebugText;
	public Text debugText;
	public GameObject player1BodyBox;
	public GameObject player1HitBox;
	public GameObject player1BlockBox;
	public bool player1AI;
	public GameObject player2BodyBox;
	public GameObject player2HitBox;
	public GameObject player2BlockBox;
	public bool player2AI;
	public float forwardVelocity;
	public float backwardVelocityFactor;
	public float runningVelocityFactor;
	public Camera perspectiveCamera;
	public float perspXPadding;
	public float perspAngle;
	public float perspWidthMinimum;
	private Player player1;
	private Player player2;

	private class GameState
	{
	}
	private class Action
	{
		public bool moving;
		public bool movingAway;
		public bool running;
		public bool blocking;
		public bool lowBlocking;
		public int initiateAttack;
		public Action() {
			moving = false;
			movingAway = false;
			running = false;
			blocking = false;
			lowBlocking = false;
			initiateAttack = 0;
		}
	}
	private class AI
	{
		public Action queryAction(GameState state) {
			Action resultAction = new Action();
			return resultAction;
		}
	}
	private class Player
	{
		// STAT VARIABLES
		private float health;
		// HITBOX VARIABLES
		private GameObject playerBodyBox;
		private GameObject playerHitBox;
		private GameObject playerBlockBox;
		// MOVEMENT VARIABLES
		private bool running;
		private bool movingLeft;
		private bool movingRight;
		private bool movingAway;
		private float distanceMoved;
		private float oldXPosition;
		// ANIMATION VARIABLES
		private float timeEnds;
		private float timeAttackBegins;
		private float timeAttackEnds;
		private float reach;
		private float attackDamage;
		private bool lowAttack;
		// INFO VARIABLES
		private bool inputHold;
		private bool attackWasThrown;
		private bool attackWasFinished;
		private bool attackHit;
		public Player() {
			health = 1000.0f;
			running = false;
			movingLeft = false;
			movingRight = false;
			movingAway = false;
			distanceMoved = 0.0f;
			oldXPosition = 0.0f;
			timeEnds = 0.0f;
			timeAttackBegins = 0.0f;
			timeAttackEnds = 0.0f;
			reach = 0.0f;
			attackDamage = 0.0f;
			lowAttack = false;
			inputHold = false;
			attackWasThrown = false;
			attackWasFinished = false;
			attackHit = false;
		}
		public void start(GameObject playerBodyBoxP, GameObject playerHitBoxP, GameObject playerBlockBoxP) {
			playerBodyBox = playerBodyBoxP;
			playerHitBox = playerHitBoxP;
			playerBlockBox = playerBlockBoxP;
		}
		// --------------------------------------------------------------------------------
		// PLAYER.THROWATTACK();
		// Throws up the attack hitbox, but only if it needs to be thrown up.
		// Tells the player that its attack hitbox was thrown up.
		// --------------------------------------------------------------------------------
		private void throwAttack(float otherPlayerXPos) {
			if (!attackWasThrown) {
				bool facingRight = (playerBodyBox.transform.position.x < otherPlayerXPos);
				float height = 0.5f;
				float playerWidth = playerBodyBox.transform.localScale.x * 0.5f;
				float reachWidth = reach * 0.5f;
				if (!lowAttack) {
					height = 1.5f;
				}
				if (facingRight) {
					playerHitBox.transform.position = new Vector3 (getXPos () + playerWidth + reachWidth, height, 0.0f);
				} else {
					playerHitBox.transform.position = new Vector3 (getXPos () - playerWidth - reachWidth, height, 0.0f);
				}
				attackWasThrown = true;
			}
		}
		// --------------------------------------------------------------------------------
		// PLAYER.FINISHATTACK();
		// Throws down the attack hitbox, but only if it needs to be thrown down.
		// Tells the player that its attack hitbox was thrown down.
		// --------------------------------------------------------------------------------
		private void finishAttack() {
			if (!attackWasFinished) {
				playerHitBox.SetActive (false);
				playerHitBox.transform.position = new Vector3 (0.0f, -10.0f, 0.0f);
				playerHitBox.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				attackWasFinished = true;
			}
		}
		// --------------------------------------------------------------------------------
		// PLAYER.RECEIVEATTACK();
		// The player receives an attack for the given amount of damage. This function
		// should define the behavior a player goes through when they are hit by an attack.
		// --------------------------------------------------------------------------------
		public void receiveAttack(float damage) {
			health = health - damage;
		}
		// --------------------------------------------------------------------------------
		// PLAYER.INITIATEACTION();
		// Initates a new attack action with the provided attributes.
		// --------------------------------------------------------------------------------
		private void initiateAction(float duration, float attackBegin, float attackDuration, float newReach, float newDamage, bool newLow) {
			inputHold = true;
			attackWasThrown = false;
			attackWasFinished = false;
			attackHit = false;
			timeEnds = Time.time + duration;
			timeAttackBegins = Time.time + attackBegin;
			timeAttackEnds = Time.time + attackBegin + attackDuration;
			reach = newReach;
			attackDamage = newDamage;
			lowAttack = newLow;
			playerHitBox.SetActive (true);
			playerHitBox.transform.position = new Vector3 (0.0f, -10.0f, 0.0f);
			playerHitBox.transform.localScale = new Vector3 (reach, 1.0f, 1.0f);
		}
		// --------------------------------------------------------------------------------
		// PLAYER.UPDATE();
		// In this function, player status that is not dependant upon input, such as in-
		// progress animations, are handled.
		// --------------------------------------------------------------------------------
		public void update(float otherPlayerXPos) {
			if (inputHold) {
				if (Time.time >= timeEnds) {
					if (attackWasThrown) {
						// Animation is over. Release hold and return.
						finishAttack();
						inputHold = false;
					} else {
						// Attack animation cannot end without attack being thrown for at least a frame.
						// Throw the attack. Next frame, it will be seen that the attack was thrown.
						throwAttack(otherPlayerXPos);
					}
				} else if (Time.time >= timeAttackEnds) {
					// Attack period over. Finish attack hitbox.
					finishAttack();
				} else if (Time.time >= timeAttackBegins) {
					// Attack period began. Throw attack hitbox.
					throwAttack(otherPlayerXPos);
				}
			}
		}
		// --------------------------------------------------------------------------------
		// PLAYER.QUERYINPUT();
		// queryInput() takes some limited game state data and determines based on input
		// for the frame how the player is to behave. Player behavior is
		// actually handled in handleInput(), which contains a collision resolution
		// algorithm.
		// --------------------------------------------------------------------------------
		/*public void queryInput(bool player1, bool isAI, float otherPlayerXPos, float fv, float bvf, float rvf) {
			if (!inputHold) {
				Action action = new Action ();
				if (isAI) {
					GameState state = new GameState (); 
				} else {
					return;
				}
			}
		}*/
		public void queryInput(bool player1, bool isAI, float otherPlayerXPos, float fv, float bvf, float rvf) {
			bool lowMod = false;
			running = false;
			movingLeft = false;
			movingRight = false;
			movingAway = false;
			distanceMoved = 0.0f;
			oldXPosition = playerBodyBox.transform.position.x;
			if (!inputHold) {
				if (!isAI) {
					if ((player1 && Input.GetKey (KeyCode.S)) || (!player1 && Input.GetKey (KeyCode.K))) {
						lowMod = true;
					}
					if ((player1 && Input.GetKeyDown (KeyCode.Q)) || (!player1 && Input.GetKeyDown (KeyCode.U))) {
						if (lowMod) {
							// Do a weak lower attack
							initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 50.0f, true);
						} else {
							// Do a weak upper attack
							initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 50.0f, false);
						}
						return;
					}
					if ((player1 && Input.GetKeyDown (KeyCode.E)) || (!player1 && Input.GetKeyDown (KeyCode.O))) {
						if (lowMod) {
							// Do a strong lower attack
							initiateAction (2.0f, 1.25f, 0.5f, 1.0f, 50.0f, true);
						} else {
							// Do a strong upper attack
							initiateAction (2.0f, 1.25f, 0.5f, 1.0f, 50.0f, false);
						}
						return;
					}
					if ((player1 && Input.GetKey (KeyCode.LeftShift)) || (!player1 && Input.GetKey (KeyCode.RightShift))) {
						running = true;
					}
					if ((player1 && Input.GetKey (KeyCode.A)) || (!player1 && Input.GetKey (KeyCode.J))) {
						movingLeft = true;
					}
					if ((player1 && Input.GetKey (KeyCode.D)) || (!player1 && Input.GetKey (KeyCode.L))) {
						movingRight = true;
					}
					if ((movingLeft == movingRight) && (movingLeft == true)) {
						movingLeft = false;
						movingRight = false;
						running = false;
					}
				} else {
				}
			}
			movingAway = (movingLeft && (playerBodyBox.transform.position.x < otherPlayerXPos)) || (movingRight && (playerBodyBox.transform.position.x >= otherPlayerXPos));
			if (movingLeft) {
				distanceMoved = -1.0f * fv;
			}
			if (movingRight) {
				distanceMoved = fv;
			}
			if (movingAway) {
				running = false;
				distanceMoved = distanceMoved * bvf;
			}
			if (running) {
				distanceMoved = distanceMoved * rvf;
			}
			distanceMoved = distanceMoved * Time.deltaTime;
		}
		// --------------------------------------------------------------------------------
		// PLAYER.HANDLEINPUT();
		// Based on limited game state data, handleInput() handles the movement that was
		// planned to be performed in the queryInput() phase. This function contains
		// collision detection algorithms, ensuring that when players move their body boxes
		// never intersect with one another.
		// --------------------------------------------------------------------------------
		public void handleInput(float otherOldXPos, float otherDistanceMoved, bool otherMovingAway) {
			if (getIsMoving ()) {
				float projectedXPos = oldXPosition + distanceMoved;
				float projectedOtherXPos = otherOldXPos + otherDistanceMoved;
				float projectedDistance = Mathf.Abs (projectedXPos - projectedOtherXPos);
				// If there is going to be a collision and moving towards...
				if (!movingAway && (projectedDistance < playerBodyBox.transform.localScale.x)) {
					// If the other is moving away....
					if (otherMovingAway) {
						// Move right up to the other's projected position.
						if (movingLeft) {
							playerBodyBox.transform.position = new Vector3 (projectedOtherXPos + playerBodyBox.transform.localScale.x, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
						} else {
							playerBodyBox.transform.position = new Vector3 (projectedOtherXPos - playerBodyBox.transform.localScale.x, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
						}
					} else {
						// They are both moving towards one another.
						// Move a ratio of the distance between them based on how much each was supposed to move.
						float distanceRatio = Mathf.Abs(distanceMoved) / (Mathf.Abs(distanceMoved) + Mathf.Abs(otherDistanceMoved));
						float distanceAway = Mathf.Abs (oldXPosition - otherOldXPos) - playerBodyBox.transform.localScale.x;
						float actualDistance = distanceAway * distanceRatio;
						if (movingLeft) {
							actualDistance = actualDistance * -1.0f;
						}
						playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x + actualDistance, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
					}
				} else {
					playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x + distanceMoved, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
				}
			}
		}
		public bool attackHandle() {
			return (attackWasThrown && !attackWasFinished && !attackHit);
		}
		public float getHitXPos() {
			return playerHitBox.transform.position.x;
		}
		public float getHalfWidth() {
			return playerBodyBox.transform.localScale.x * 0.5f;
		}
		public float getHitHalfWidth() {
			return playerHitBox.transform.localScale.x * 0.5f;
		}
		public void tellHit() {
			attackHit = true;
		}
		public float getAttackDamage() {
			return attackDamage;
		}
		public float getXPos() {
			return playerBodyBox.transform.position.x;
		}
		public float getOldXPos() {
			return oldXPosition;
		}
		public float getYPos() {
			return playerBodyBox.transform.position.y;
		}
		public float getDistanceMoved() {
			return distanceMoved;
		}
		public bool getIsMoving() {
			return (movingLeft || movingRight);
		}
		public bool getIsMovingAway() {
			return movingAway;
		}
		public float getHeath() {
			return health;
		}
	}
	void Start ()
	{
		debugging = false;
		displayDebugText = false;
		debugText.text = "";
		perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
		player1 = new Player ();
		player2 = new Player ();
		player1.start (player1BodyBox, player1HitBox, player1BlockBox);
		player2.start (player2BodyBox, player2HitBox, player2BlockBox);
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
				perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
			}
			if (Input.GetKeyDown (KeyCode.J)) {
				perspXPadding = perspXPadding - 0.5f;
			}
			if (Input.GetKeyDown (KeyCode.K)) {
				perspAngle = perspAngle + 0.5f;
				perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
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
			player1.update (player2.getXPos ());
			player2.update (player1.getXPos ());
			// QUERY PLAYER INPUT
			player1.queryInput (true, player1AI, player2.getXPos (), forwardVelocity, backwardVelocityFactor, runningVelocityFactor);
			player2.queryInput (false, player2AI, player1.getXPos (), forwardVelocity, backwardVelocityFactor, runningVelocityFactor);
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
		float xTransformRad = perspectiveCamera.transform.eulerAngles.x * Mathf.Deg2Rad;
		float yheight = (player1.getYPos() + player2.getYPos()) * 0.5f;
		float xwidth = Mathf.Abs(player2.getXPos() - player1.getXPos()) + perspXPadding;
		xwidth = Mathf.Max (xwidth, perspWidthMinimum);
		float hFOVRad = 2.0f * Mathf.Atan (Mathf.Tan (perspectiveCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * perspectiveCamera.aspect);
		float distance = (0.5f * xwidth) / Mathf.Tan (hFOVRad * 0.5f);
		float yAboveHeight = distance * Mathf.Sin (xTransformRad);
		float zAbsolute = distance * Mathf.Cos (xTransformRad);
		float x = (player1.getXPos() + player2.getXPos()) * 0.5f;
		float y = yAboveHeight + yheight;
		float z = zAbsolute * -1.0f;
		perspectiveCamera.transform.position = new Vector3 (x, y, z);
	}
	void handlePlayerHit(Player attacker, Player defender) {
		// If the attacker is engaged in an attack that needs to be handled:
		if (attacker.attackHandle ()) {
			// See if the attack box is in the body box.
			float distance = Mathf.Abs(defender.getXPos() - attacker.getHitXPos());
			distance = distance - attacker.getHitHalfWidth() - defender.getHalfWidth();
			if (distance <= 0.0f) {
				// This was a hit. Handle it.
				attacker.tellHit();
				defender.receiveAttack (attacker.getAttackDamage ());
			}
		}
	}
}
