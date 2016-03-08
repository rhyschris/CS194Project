﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class PlayerController : MonoBehaviour {
	// AI
	AI playerAI;
	// HITBOX VARIABLES
	private GameObject playerBodyBox;
	private GameObject playerHitBox;
	private GameObject playerBlockBox;
	// STAT VARIABLES
	private float health;
	private float maxHealth;

	public float blockDamageModifier;
	private float blockPercentage;
	public bool player1;
	public bool isAI;
	public float forwardVelocity;
	public float backwardVelocityFactor;
	public float runningVelocityFactor;
	private ActionType jumpHorizAction;
	private ActionType lastAttack;
	private ActionType lastAction;

	//public float upwardVelocity;
	public float gravity;
	public float initialJumpVelocity;	
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
	private float jumpVelocity;
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


	// Animation Controller
	Animator fighterAnimator;
	public GameObject fighter;

	// Dictionary of state machine behaviors 
	// English 'behaviour' spelling to match the Unity module naming :/
	private BufferedStateMachineBehaviour[] animatedBehaviours; 

	/**
		 * PLAYER.UPDATE();
		 * In this function, player status that is not dependent upon input, such as in-
		 * progress animations, are handled.
		 *
		 */

	public void handleAutomaticUpdates(float otherPlayerXPos) {
		bool shouldHold = false;

		foreach (BufferedStateMachineBehaviour bhvr in animatedBehaviours) { 
			bool active = bhvr.isActive ();
			if (active) {
				shouldHold = true;
				if (bhvr.isTriggered ()) {
					throwAttack (otherPlayerXPos);
				}

			} else if (bhvr.isTriggered ()) {
				// Not active + recently changed
				finishAttack();
			}
		}
		inputHold = shouldHold;
	}

	/** 
		 * PLAYER.QUERYINPUT();
		 * Determines the action for the player to peform this frame, either by querying
		 * keyboard input or the AI.
		 */
	public Action queryInput(GameState curState) {
		Action action = new Action();
		/* Horizontal movement: determine if moving towards or away 
			 * Player does this by interpreting key intent
			 * AI does this by direct command
			 * 
			 * TODO: clean up player key intent (hierarchial enum)?
			 */
		bool running;
		bool movingLeft = false;
		bool movingRight;
		bool movingAway;
		bool moving;

		if (isAI) {

			action = playerAI.queryAction (curState);
			if (inputHold) {
				action = new Action ();
			} 
			/* Assuming AI is player 2 - he will face left */
			if (this.player1)
				movingRight = (action.actionType & Action.HMOVE_MASK) != ActionType.moveAway; 
			else
				movingLeft = (action.actionType & Action.HMOVE_MASK) != ActionType.moveAway; 

		} else {
			if (!inputHold) {
				// QUERY KEYBOARD INPUT
				bool lowMod = Input.GetKey (Down);

				/*Do a block */
				if (Input.GetKey (Block)){
					action.actionType |= lowMod? ActionType.blockDown:ActionType.blockUp;
				}
				/*Do a weak attack*/
				else if (Input.GetKeyDown (Attack1)){
					action.actionType |= lowMod? ActionType.attack3:ActionType.attack1;
				}
				/*Do a strong attack */
				else if (Input.GetKeyDown (Attack2)){
					action.actionType |= lowMod? ActionType.attack4:ActionType.attack2;
				}
				else if (Input.GetKeyDown (Up)){
					action.actionType |= ActionType.jump;
				}				
				/* Crouch */
				else if (lowMod) {
					action.actionType |= ActionType.crouch;
				} 
				else {

					float otherPlayerXPos = (player1) ? curState.getP2XPos() :  curState.getP1XPos();
					running = Input.GetKey (Run);
					movingLeft = Input.GetKey (Left);
					movingRight = Input.GetKey (Right);
					movingAway = (movingLeft && (playerBodyBox.transform.position.x < otherPlayerXPos)) || (movingRight && (playerBodyBox.transform.position.x >= otherPlayerXPos));
					moving = movingLeft || movingRight;
					if ( (movingLeft && movingRight) || lowMod) {
						movingLeft = false;
						movingRight = false;
						running = false;
					}

					if (moving) {
						if (movingAway) {
							action.actionType |= ActionType.moveAway;
						} else if (running) {
							action.actionType |= ActionType.runTowards;
						} else {
							action.actionType |= ActionType.walkTowards;	
						}
					}
				}
			}
		}

		action.oldXPosition = playerBodyBox.transform.position.x;
		updatePosition (action, movingLeft);
		return action;
	}

	/** 
		 * Updates the action argument to indicate the distance moved during this frame.
		 */
	public void updatePosition (Action action, bool movingLeft){
		ActionType hmove = action.actionType & Action.HMOVE_MASK;
		if (hmove == ActionType.moveAway) {
			action.distanceMoved = forwardVelocity * backwardVelocityFactor * Time.deltaTime;
		} else if (hmove == ActionType.runTowards) {
			action.distanceMoved = forwardVelocity * runningVelocityFactor * Time.deltaTime;
		} else if (hmove == ActionType.walkTowards) {
			action.distanceMoved = forwardVelocity * Time.deltaTime;
		}
		if (movingLeft)
			action.distanceMoved *= -1.0f;
	}
	/** --------------------------------------------------------------------------------
		 * PLAYER.HANDLEINPUT();
		 * Handles the action queried earlier. Uses collision detection for movements to
		 * ensure player's do not overlap.
		 * --------------------------------------------------------------------------------
		 */
	public void handleInput(Action myAction, Action theirAction) {
		ActionType my_horiz = myAction.actionType & Action.HMOVE_MASK;
		ActionType their_horiz = theirAction.actionType & Action.HMOVE_MASK;

		if (myAction.actionType == ActionType.walkTowards) {
			fighterAnimator.SetBool ("runForward", true);
		} else {
			fighterAnimator.SetBool ("runForward", false);
		}
		if (myAction.actionType == ActionType.moveAway) {
			fighterAnimator.SetBool ("runBackward", true);
		} else {
			fighterAnimator.SetBool ("runBackward", false);
		}


		if (! (my_horiz>0)){
			switch (myAction.actionType) {
			case ActionType.attack1:
				lastAttack = ActionType.attack1;
				fighterAnimator.SetBool("highPunch", true);
				initiateAction ( 1.0f, 50, false);
				break;
			case ActionType.attack2:
				lastAttack = ActionType.attack2;
				fighterAnimator.SetBool ("highKick", true);
				initiateAction (1.0f, 100, false);
				break;
			case ActionType.attack3:
				lastAttack = ActionType.attack3;
				fighterAnimator.SetBool ("lowKick", true);
				initiateAction ( 1.0f, 50, true);
				break;
			case ActionType.attack4:
				lastAttack = ActionType.attack4;
				fighterAnimator.SetBool ("lowTrip", true);
				initiateAction ( 1.0f, 100, true);
				break;
			case ActionType.jump:
				if (!isJumping) {/* Just started jump, need to note end of jump time*/
					isJumping = true;
					jumpVelocity = initialJumpVelocity;
					jumpHorizAction = lastAction;
				}
				break;
			}
		}
		if (isJumping){
			bool movingLeft = false;
			myAction.actionType = jumpHorizAction;
			if (!this.player1)
				movingLeft = (myAction.actionType & Action.HMOVE_MASK) != ActionType.moveAway;
			else
				movingLeft = myAction.actionType==ActionType.moveAway;

			updatePosition(myAction,movingLeft);
			myAction.distanceMoved = (float) (myAction.distanceMoved*1.5);
			my_horiz = myAction.actionType & Action.HMOVE_MASK;
		}

		if (my_horiz > 0){

			float projectedXPos = myAction.oldXPosition + myAction.distanceMoved;
			float projectedOtherXPos = theirAction.oldXPosition + theirAction.distanceMoved;
			float projectedDistance = Mathf.Abs (projectedXPos - projectedOtherXPos);

			//TODO: Can't jump over another player as of now. Fix that? 

			// If there is going to be a collision and moving towards...
			if (my_horiz != ActionType.moveAway && (projectedDistance < playerBodyBox.transform.localScale.x)) {
				// If the other is moving away....
				if (their_horiz == ActionType.moveAway) {
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

		}


		bool facingLeft = (myAction.oldXPosition > theirAction.oldXPosition);
		float blockBoxOffset = (playerBodyBox.transform.localScale.x + playerBlockBox.transform.localScale.x) * 0.5f;
		if (facingLeft) {
			blockBoxOffset = blockBoxOffset * -1.0f;
		}
		blockBoxOffset = playerBodyBox.transform.position.x + blockBoxOffset;

		ActionType blockAction = myAction.actionType & Action.ATTACK_MASK;
		if (blockAction == ActionType.blockUp) {
			playerBlockBox.SetActive (true);
			playerBlockBox.transform.position = new Vector3 (blockBoxOffset, 1.5f, 0.0f);
			blocking = true;
			lowBlocking = false;
			blockPercentage -= .002f;
			blockPercentage = (blockPercentage <= 0.0f) ? 0.0f : blockPercentage; 
		} else if (blockAction == ActionType.blockDown) {
			playerBlockBox.SetActive (true);
			playerBlockBox.transform.position = new Vector3 (blockBoxOffset, 0.5f, 0.0f);
			blocking = true;
			lowBlocking = true;
			blockPercentage -= .002f;
			blockPercentage = (blockPercentage <= 0.0f) ? 0.0f : blockPercentage; 		
		} else {
			playerBlockBox.transform.position = new Vector3 (0.0f, -1.0f, 0.0f);
			blocking = false;
			lowBlocking = false;
			blockPercentage += .002f;
			blockPercentage = (blockPercentage >= 1.0f) ? 1.0f : blockPercentage; 		}

		/*Handle jumping animation*/
		if (isJumping){
			float newY = playerBodyBox.transform.position.y + jumpVelocity;
			if (newY <=this.getHalfHeight()) {/*We hit the ground, need to stop moving. We can now jump again*/
				isJumping = false;
				newY = this.getHalfHeight();
			}
			playerBodyBox.transform.position = new Vector3 (playerBodyBox.transform.position.x, newY, playerBodyBox.transform.position.z);
			jumpVelocity = jumpVelocity - gravity; 

		}

		lastAction = myAction.actionType;
	}
	/**
		 * PLAYER.THROWATTACK();
		 * Throws up the attack hitbox, but only if it needs to be thrown up.
		 * Tells the player that its attack hitbox was thrown up.
		 */
	private void throwAttack(float otherPlayerXPos) {
		if (!attackWasThrown) {
			bool facingRight = (this.getXPos() < otherPlayerXPos);
			float height = this.getYPos() -this.getHalfWidth();
			float playerWidth = this.getHalfWidth();
			float reachWidth = reach * 0.5f;
			if (!lowAttack) {
				height = this.getYPos() + this.getHalfWidth();
			}
			if (facingRight) {
				playerHitBox.transform.position = new Vector3 (getXPos () + playerWidth + reachWidth, height, 0.0f);
			} else {
				playerHitBox.transform.position = new Vector3 (getXPos () - playerWidth - reachWidth, height, 0.0f);
			}
			attackWasThrown = true;
		}
	}
	/**
		 * PLAYER.FINISHATTACK();
		 * Throws down the attack hitbox, but only if it needs to be thrown down.
		 * Tells the player that its attack hitbox was thrown down.
		 */
	private void finishAttack() {
		if (!attackWasFinished) {
			playerHitBox.SetActive (false);
			playerHitBox.transform.position = new Vector3 (0.0f, -10.0f, 0.0f);
			playerHitBox.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			attackWasFinished = true;
		}
	}
	/**
		 * PLAYER.RECEIVEATTACK();
		 * The player receives an attack for the given amount of damage. This function
		 * should define the behavior a player goes through when they are hit by an attack.
		 */
	public void receiveAttack(float damage, bool blocked) {
		if (blocked) {
			damage = damage * (1.0f - blockPercentage) + damage * blockPercentage * blockDamageModifier;
			health = health - damage;
			// TODO: Handle behavior if hit while blocking.
		} else {
			health = health - damage;
			inputHold = false;
			finishAttack ();
			// TODO: Handle behavior if hit while not blocking.
		}
	}
	/**
	 * PLAYER.INITIATEACTION();
	 * Initates a new attack action with the provided attributes.
	 */
	private void initiateAction(float newReach, float newDamage, bool newLow) {
		// inputHold = true;
		attackWasThrown = false;
		attackWasFinished = false;
		attackHit = false;

		reach = newReach;
		attackDamage = newDamage;
		lowAttack = newLow;
		playerHitBox.SetActive (true);
		playerHitBox.transform.position = new Vector3 (0.0f, -10.0f, 0.0f);
		playerHitBox.transform.localScale = new Vector3 (reach, 1.0f, 1.0f);
		// set animation bool
	}
	public bool attackHandle() {
		return (attackWasThrown && !attackWasFinished && !attackHit);
	}
	public float getHitXPos() {
		return playerHitBox.transform.position.x;
	}
	public float getHitYPos() {
		return playerHitBox.transform.position.y;
	}

	public void moveToX(float x){
		playerBodyBox.transform.position = new Vector3 (x, playerBodyBox.transform.position.y, playerBodyBox.transform.position.z);
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
	public float getHitHalfHeight() {
		return playerHitBox.transform.localScale.y * 0.5f;
	}

	public void tellHit() {
		attackHit = true;
	}
	public float getAttackDamage() {
		return attackDamage;
	}
	public float getHealthPercent() {
		return health / maxHealth;
	}
	public float getXPos() {
		return playerBodyBox.transform.position.x;
	}
	public ActionType lastAttackThrown(){
		return lastAttack;
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

	public void setPlayerAI(){
		isAI = true;
		Process process = new Process();
		// Configure the process using the StartInfo properties.
		process.StartInfo.FileName = "CMD.EXE";
		process.StartInfo.Arguments = "/K cd ..\\ai\\agents && C:\\Python27\\python basic_qlearn.py 5998";
		process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

		process.Start();
		process.WaitForExit ();
	}
	public void resetPlayer(){
		int xpos = 4;
		if (player1)
			xpos = -4;
		playerBodyBox.transform.position = new Vector3 (xpos, getHalfHeight (), 0);
		health = maxHealth; // Debug for animation
	}
	void Start () {
		// fighter is the model, fighterAnimator is the animation controller, we need access to it here in order
		// to set the correct bools that trigger different animation states
		fighterAnimator = fighter.GetComponent<Animator> ();
		// Capture the animation behaviors that underlie each state.  
		animatedBehaviours = fighterAnimator.GetBehaviours<BufferedStateMachineBehaviour>();

		health = 2000.0f; // Debug for animation
		maxHealth = health;
		blockPercentage = 1.0f;
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

			if (isAI) {
				int inPort = 4998;

				playerAI = new AI (inPort, inPort + 1);
				playerAI.verifyNetwork ();

			} else {
				Up = KeyCode.W;
				Down = KeyCode.S;
				Left = KeyCode.A;
				Right = KeyCode.D;
				Run = KeyCode.LeftShift;
				Attack1 = KeyCode.Q;
				Attack2 = KeyCode.E;
				Block = KeyCode.F;
			}

		} else { // player 2
			playerBodyBox = GameObject.Find ("Player2BodyBox");
			playerHitBox = GameObject.Find ("Player2HitBox");
			playerBlockBox = GameObject.Find ("Player2BlockBox");

			if (isAI){
				int inPort = 5998;

				playerAI = new AI (inPort, inPort + 1);
				playerAI.verifyNetwork ();
			} else {
				Up = KeyCode.I;
				Down = KeyCode.K;
				Left = KeyCode.J;
				Right = KeyCode.L;
				Run = KeyCode.RightShift;
				Attack1 = KeyCode.U;
				Attack2 = KeyCode.O;
				Block = KeyCode.H;
			}

		}
	}
}