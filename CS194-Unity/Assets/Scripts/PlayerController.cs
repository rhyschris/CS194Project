using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	// HITBOX VARIABLES
	public GameObject playerBodyBox;
	public GameObject playerHitBox;
	public GameObject playerBlockBox;
	// STAT VARIABLES
	private float health;
	public bool player1;
	public bool isAI;
	public float forwardVelocity;
	public float backwardVelocityFactor;
	public float runningVelocityFactor;
	// KEYBOARD INPUT
	KeyCode /*Up, */Down, Left, Right, Run, Attack1, Attack2/*, Attack3, Attack4*/;
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
		inputHold = false;
		finishAttack ();
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
	public void handleAutomaticUpdates(float otherPlayerXPos) {
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
	public void queryInput(float otherPlayerXPos) {
		bool lowMod = false;
		running = false;
		movingLeft = false;
		movingRight = false;
		movingAway = false;
		distanceMoved = 0.0f;
		oldXPosition = playerBodyBox.transform.position.x;
		if (!inputHold) {
			if (isAI) {
				// QUERY AI INPUT
				// TODO: Query the AI for input.
			} else {
				// QUERY KEYBOARD INPUT
				lowMod = Input.GetKey(Down);
				if (Input.GetKeyDown (Attack1)) {
					if (lowMod) {
						// Do a weak lower attack
						initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 50.0f, true);
					} else {
						// Do a weak upper attack
						initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 50.0f, false);
					}
					return;
				}
				if (Input.GetKeyDown (Attack2)) {
					if (lowMod) {
						// Do a strong lower attack
						initiateAction (2.0f, 1.25f, 0.5f, 1.0f, 50.0f, true);
					} else {
						// Do a strong upper attack
						initiateAction (2.0f, 1.25f, 0.5f, 1.0f, 50.0f, false);
					}
					return;
				}
				running = Input.GetKey (Run);
				movingLeft = Input.GetKey (Left);
				movingRight = Input.GetKey (Right);
				if ((movingLeft == movingRight) && (movingLeft == true)) {
					movingLeft = false;
					movingRight = false;
					running = false;
				}
			}
		}
		movingAway = (movingLeft && (playerBodyBox.transform.position.x < otherPlayerXPos)) || (movingRight && (playerBodyBox.transform.position.x >= otherPlayerXPos));
		if (movingLeft) {
			distanceMoved = -1.0f * forwardVelocity;
		}
		if (movingRight) {
			distanceMoved = forwardVelocity;
		}
		if (movingAway) {
			running = false;
			distanceMoved = distanceMoved * backwardVelocityFactor;
		}
		if (running) {
			distanceMoved = distanceMoved * runningVelocityFactor;
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
	void Awake () {
		
	}
	void Start () {
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
		if (player1) {
			//Up = KeyCode.W; 
			Down = KeyCode.S;
			Left = KeyCode.A;
			Right = KeyCode.D;
			Run = KeyCode.LeftShift;
			Attack1 = KeyCode.Q;
			Attack2 = KeyCode.E;
		} else {
			//Up = KeyCode.I;
			Down = KeyCode.K;
			Left = KeyCode.J;
			Right = KeyCode.L;
			Run = KeyCode.RightShift;
			Attack1 = KeyCode.U;
			Attack2 = KeyCode.O;
		}
	}
}
