﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	// AI
	AI playerAI;
	// HITBOX VARIABLES
	private GameObject playerBodyBox;
	private GameObject playerHitBox;
	private GameObject playerBlockBox;
	// STAT VARIABLES
	private float health;
	public bool player1;
	public bool isAI;
	public float forwardVelocity;
	public float backwardVelocityFactor;
	public float runningVelocityFactor;
	public float upwardVelocity;
	public float downwardVelocity;
	public float JUMP_TIME_CONST;	
	// KEYBOARD INPUT
	private KeyCode Up; 
	private KeyCode Down;
	private KeyCode Left;
	private KeyCode Right;
	private KeyCode Run;
	private KeyCode Attack1;
	private KeyCode Attack2;
	private KeyCode Block;
	// ANIMATION VARIABLES
	private float timeEnds;
	private float timeAttackBegins;
	private float timeAttackEnds;
	private float timeJumpUpEnds;
	private float reach;
	private float attackDamage;
	private bool lowAttack;
	// INFO VARIABLES
	private bool inputHold;
	private bool attackWasThrown;
	private bool attackWasFinished;
	private bool attackHit;
	private bool blocking;
	private bool lowBlocking;
	private bool isJumping;
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
	// Determines the action for the player to peform this frame, either by querying
	// keyboard input or the AI.
	// --------------------------------------------------------------------------------
	public Action queryInput(float otherPlayerXPos) {
		Action action = new Action();
		if (isAI) {
			GameState state = new GameState ();
			// TODO: Build the state here.
			action = playerAI.queryAction (state);
			if (inputHold) {
				action = new Action ();
			}
		} else {
			if (!inputHold) {
				// QUERY KEYBOARD INPUT
				bool lowMod = Input.GetKey (Down);

				/*Do a block */
				if (Input.GetKey (Block)){
					action.actionType = lowMod? ActionType.blockDown:ActionType.blockUp;
				}
				/*Do a weak attack*/
				else if (Input.GetKeyDown (Attack1)){
					action.actionType = lowMod? ActionType.attack3:ActionType.attack1;
				}
				/*Do a strong attack */
				else if (Input.GetKeyDown (Attack2)){
					action.actionType = lowMod? ActionType.attack4:ActionType.attack2;
				}
				else if (Input.GetKeyDown (Up)){
					action.actionType = ActionType.jump;
				}		
				/*Crouch */
				else if (lowMod) {
					action.actionType = ActionType.crouch;
				} 
				else {
					bool running = Input.GetKey (Run);
					bool movingLeft = Input.GetKey (Left);
					bool movingRight = Input.GetKey (Right);
					bool movingAway = (movingLeft && (playerBodyBox.transform.position.x < otherPlayerXPos)) || (movingRight && (playerBodyBox.transform.position.x >= otherPlayerXPos));
					bool moving = movingLeft || movingRight;
					if ( (movingLeft && movingRight) || lowMod) {
						movingLeft = false;
						movingRight = false;
						running = false;
					}

					if (moving) {
						if (movingAway) {
							action.actionType = ActionType.moveAway;
							action.distanceMoved = forwardVelocity * backwardVelocityFactor * Time.deltaTime;
						} else if (running) {
							action.actionType = ActionType.runTowards;
							action.distanceMoved = forwardVelocity * runningVelocityFactor * Time.deltaTime;
						} else {
							action.actionType = ActionType.walkTowards;
							action.distanceMoved = forwardVelocity * Time.deltaTime;
						}
					}
					if (movingLeft) {
						action.distanceMoved = action.distanceMoved * -1.0f;
					}
				}
			}
		}
		action.oldXPosition = playerBodyBox.transform.position.x;
		return action;
	}
	// --------------------------------------------------------------------------------
	// PLAYER.HANDLEINPUT();
	// Handles the action queried earlier. Uses collision detection for movements to
	// ensure player's do not overlap.
	// --------------------------------------------------------------------------------
	public void handleInput(Action myAction, Action theirAction) {
		if (myAction.actionType == ActionType.walkTowards || myAction.actionType == ActionType.runTowards || myAction.actionType == ActionType.moveAway) {
			float projectedXPos = myAction.oldXPosition + myAction.distanceMoved;
			float projectedOtherXPos = theirAction.oldXPosition + theirAction.distanceMoved;
			float projectedDistance = Mathf.Abs (projectedXPos - projectedOtherXPos);

			//TODO: Can't jump over another player as of now. Fix that? 

			// If there is going to be a collision and moving towards...
			if (myAction.actionType != ActionType.moveAway && (projectedDistance < playerBodyBox.transform.localScale.x)) {
				// If the other is moving away....
				if (theirAction.actionType == ActionType.moveAway) {
					// Move right up to the other's projected position.
					if (myAction.distanceMoved < 0.0f) {
						playerBodyBox.transform.position = new Vector3 (projectedOtherXPos + playerBodyBox.transform.localScale.x, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
					} else {
						playerBodyBox.transform.position = new Vector3 (projectedOtherXPos - playerBodyBox.transform.localScale.x, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
					}
				} else {
					// They are both moving towards one another.
					// Move a ratio of the distance between them based on how much each was supposed to move.
					float distanceRatio = Mathf.Abs (myAction.distanceMoved) / (Mathf.Abs (myAction.distanceMoved) + Mathf.Abs (theirAction.distanceMoved));
					float distanceAway = Mathf.Abs (myAction.oldXPosition - theirAction.oldXPosition) - playerBodyBox.transform.localScale.x;
					float actualDistance = distanceAway * distanceRatio;
					if (myAction.distanceMoved < 0.0f) {
						actualDistance = actualDistance * -1.0f;
					}
					playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x + actualDistance, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
				}
			} else {
				playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x + myAction.distanceMoved, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
			}
		} else{
			switch (myAction.actionType){
				case ActionType.attack1:
					initiateAction (0.5f, 0.125f, 0.25f, 1.0f, 50, false);
					break;
				case ActionType.attack2:
					initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 100, false);
					break;
				case ActionType.attack3:
					initiateAction (0.5f, 0.125f, 0.25f, 1.0f, 50, true);
					break;
				case ActionType.attack4:
					initiateAction (1.0f, 0.25f, 0.5f, 1.0f, 100, true);
					break;
				case ActionType.jump:
					if (!isJumping) {/* Just started jump, need to note end of jump time*/
						isJumping = true;
						timeJumpUpEnds = Time.time + JUMP_TIME_CONST;
					}
					break;
			}
		}

		bool facingLeft = (myAction.oldXPosition > theirAction.oldXPosition);
		float blockBoxOffset = (playerBodyBox.transform.localScale.x + playerBlockBox.transform.localScale.x) * 0.5f;
		if (facingLeft) {
			blockBoxOffset = blockBoxOffset * -1.0f;
		}
		blockBoxOffset = playerBodyBox.transform.position.x + blockBoxOffset;
		if (myAction.actionType == ActionType.blockUp) {
			playerBlockBox.SetActive (true);
			playerBlockBox.transform.position = new Vector3 (blockBoxOffset, 1.5f, 0.0f);
			blocking = true;
			lowBlocking = false;
		} else if (myAction.actionType == ActionType.blockDown) {
			playerBlockBox.SetActive (true);
			playerBlockBox.transform.position = new Vector3 (blockBoxOffset, 0.5f, 0.0f);
			blocking = true;
			lowBlocking = true;
		} else {
			playerBlockBox.transform.position = new Vector3 (0.0f, -1.0f, 0.0f);
			blocking = false;
			lowBlocking = false;
		}
		/*Handle jumping animation*/
		if (isJumping){
			float newY;
			if (Time.time <= timeJumpUpEnds) {
				newY = playerBodyBox.transform.position.y + upwardVelocity;
			} else {
				newY = playerBodyBox.transform.position.y - downwardVelocity;

				if (newY <=this.getHalfHeight()) {/*We hit the ground, need to stop moving. We can now jump again*/
					isJumping = false;
					newY = this.getHalfHeight();
				}
			}
			playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x, newY, playerBodyBox.transform.position.z);
		}
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
	public void receiveAttack(float damage, bool blocked) {
		health = health - damage;
		if (blocked) {
			// TODO: Handle behavior if hit while blocking.
		} else {
			inputHold = false;
			finishAttack ();
			// TODO: Handle behavior if hit while not blocking.
		}
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
	public bool attackHandle() {
		return (attackWasThrown && !attackWasFinished && !attackHit);
	}
	public float getHitXPos() {
		return playerHitBox.transform.position.x;
	}
	public float getHalfHeight() {
		return playerBodyBox.transform.localScale.y * 0.5f;
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
	public float getYPos() {
		return playerBodyBox.transform.position.y;
	}
	public float getHealth() {
		return health;
	}
	public bool isHighAttack() {
		return !lowAttack;
	}
	public bool isHighBlocking() {
		return (blocking && !lowBlocking);
	}
	public bool isLowBlocking() {
		return (blocking && lowBlocking);
	}
	void Start () {
		health = 1000.0f;
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
		blocking = false;
		lowBlocking = false;
		if (player1) {
			playerBodyBox = GameObject.Find ("Player1BodyBox");
			playerHitBox = GameObject.Find ("Player1HitBox");
			playerBlockBox = GameObject.Find ("Player1BlockBox");
			Up = KeyCode.W;
			Down = KeyCode.S;
			Left = KeyCode.A;
			Right = KeyCode.D;
			Run = KeyCode.LeftShift;
			Attack1 = KeyCode.Q;
			Attack2 = KeyCode.E;
			Block = KeyCode.F;
		} else {
			playerBodyBox = GameObject.Find ("Player2BodyBox");
			playerHitBox = GameObject.Find ("Player2HitBox");
			playerBlockBox = GameObject.Find ("Player2BlockBox");
			Up = KeyCode.I;
			Down = KeyCode.K;
			Left = KeyCode.J;
			Right = KeyCode.L;
			Run = KeyCode.RightShift;
			Attack1 = KeyCode.U;
			Attack2 = KeyCode.O;
			Block = KeyCode.H;
		}

		if (isAI) {
			playerAI = new AI (4998, 4999);
			playerAI.verifyNetwork ();
		}
	}
}
