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
	public GameObject player2BodyBox;
	public GameObject player2HitBox;
	public GameObject player2BlockBox;
	public float forwardVelocity;
	public float backwardVelocityFactor;
	public float runningVelocityFactor;
	public Camera perspectiveCamera;
	public float perspXPadding;
	public float perspAngle;
	public float perspWidthMinimum;
	private class Player
	{
		private GameObject playerBodyBox;
		private GameObject playerHitBox;
		private GameObject playerBlockBox;
		private bool inputHold;
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
		private bool lowAttack;
		public Player() {
			running = false;
			movingLeft = false;
			movingRight = false;
			inputHold = false;
		}
		public void start(GameObject playerBodyBoxP, GameObject playerHitBoxP, GameObject playerBlockBoxP) {
			playerBodyBox = playerBodyBoxP;
			playerHitBox = playerHitBoxP;
			playerBlockBox = playerBlockBoxP;
		}
		// --------------------------------------------------------------------------------
		// PLAYER.UPDATE();
		// In this function, player status that is not dependant upon input, such as in-
		// progress animations, are handled.
		// --------------------------------------------------------------------------------
		public void update(float otherPlayerXPos) {
			if (inputHold) {
				if (Time.time >= timeEnds) {
					// Animation is over. Release hold and return.
					playerHitBox.SetActive (false);
					inputHold = false;
					return;
				} else if (Time.time >= timeAttackEnds) {
					// Attack period is over. Disable attack hitbox.
					playerHitBox.SetActive (false);
					return;
				} else if (Time.time >= timeAttackBegins) {
					// Attack period has began. Enable attack hitbox.
					bool facingRight = (playerBodyBox.transform.position.x < otherPlayerXPos);
					float height = 0.5f;
					float playerWidth = playerBodyBox.transform.localScale.x * 0.5f;
					float reachWidth = reach * 0.5f;
					if (!lowAttack) {
						height = 1.5f;
					}
					if (facingRight) {
						playerHitBox.transform.position = new Vector3 (getXPos() + playerWidth + reachWidth, height, 0.0f);
					} else {
						playerHitBox.transform.position = new Vector3 (getXPos() - playerWidth - reachWidth, height, 0.0f);
					}
					return;
				}
			}
		}
		// --------------------------------------------------------------------------------
		// PLAYER.QUERYINPUT();
		// queryInput() takes some limited game state data and determines based on input
		// for the frame information on how the player is to behave. Player behavior is
		// actually handled in handleInput(), which contains a collision resolution
		// algorithm.
		// --------------------------------------------------------------------------------
		public void queryInput(bool player1, float otherPlayerXPos, float fv, float bvf, float rvf) {
			bool lowMod = false;
			running = false;
			movingLeft = false;
			movingRight = false;
			movingAway = false;
			distanceMoved = 0.0f;
			oldXPosition = playerBodyBox.transform.position.x;
			if (!inputHold) {
				if ((player1 && Input.GetKey (KeyCode.S)) || (!player1 && Input.GetKey (KeyCode.K))) {
					lowMod = true;
				}
				if ((player1 && Input.GetKeyDown (KeyCode.Q)) || (!player1 && Input.GetKeyDown (KeyCode.U))) {
					if (lowMod) {
						// Do a weak lower attack
						initiateAction(1.0f,0.25f,0.5f,1.0f,true);
					} else {
						// Do a weak upper attack
						initiateAction(1.0f,0.25f,0.5f,1.0f,false);
					}
					return;
				}
				if ((player1 && Input.GetKeyDown (KeyCode.E)) || (!player1 && Input.GetKeyDown (KeyCode.O))) {
					if (lowMod) {
						// Do a strong lower attack
						initiateAction(2.0f,1.25f,0.5f,1.0f,true);
					} else {
						// Do a strong upper attack
						initiateAction(2.0f,1.25f,0.5f,1.0f,false);
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
		private void initiateAction(float duration, float attackBegin, float attackDuration, float newReach, bool newLow) {
			timeEnds = Time.time + duration;
			timeAttackBegins = Time.time + attackBegin;
			timeAttackEnds = Time.time + attackBegin + attackDuration;
			reach = newReach;
			lowAttack = newLow;
			inputHold = true;
			playerHitBox.SetActive (true);
			playerHitBox.transform.position = new Vector3 (0.0f, -10.0f, 0.0f);
			playerHitBox.transform.localScale = new Vector3 (reach, 1.0f, 1.0f);
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
	}
	private Player player1;
	private Player player2;
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
			player1.queryInput (true, player2.getXPos (), forwardVelocity, backwardVelocityFactor, runningVelocityFactor);
			player2.queryInput (false, player1.getXPos (), forwardVelocity, backwardVelocityFactor, runningVelocityFactor);
			// HANDLE PLAYER INPUT
			player1.handleInput (player2.getOldXPos (), player2.getDistanceMoved (), player2.getIsMovingAway());
			player2.handleInput (player1.getOldXPos (), player1.getDistanceMoved (), player1.getIsMovingAway());
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
}
