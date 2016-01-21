using UnityEngine;
using System.Collections;
using System;

namespace AssemblyCSharp {
public class PlayerController : MonoBehaviour {

	// variables: private by default
	// expose "getting hit"
	double health;
	float moveIncrement = 0.03f; 
	Modes.Stance stance;
	Modes.Attack atkMode;
	
	// Controlling keys (for same keyboard)	
	KeyCode Up, Down, Left, Right, Punch, Kick;
	// Use this for initialization
	void Start () {
		health = 100.0;
		stance = Modes.Stance.Stand;
		atkMode = Modes.Attack.None;
		
		// Set controls
		if (this.gameObject.name == "Player1") {
			Up = KeyCode.W; 
			Down = KeyCode.S;
			Left = KeyCode.A;
			Right = KeyCode.D;
			Punch = KeyCode.R;
			Kick = KeyCode.F;
		} else if (this.gameObject.name == "Player2") {
			Up = KeyCode.UpArrow;
			Down = KeyCode.DownArrow;
			Left = KeyCode.LeftArrow;
			Right = KeyCode.RightArrow;
			Punch = KeyCode.Slash;
			Kick = KeyCode.RightAlt;
		}
		/* GameDelegate initialized its list of players already; add self 
		 * Unity Scripts that inherit from MonoBehaviour are run on the UI loop
		 * and cannot be invoked off of the main thread, so this operation is synchronous
		 * with the Delegate's existence. 
		 */
		GameObject.Find("GameDelegate").SendMessage("addPlayer", this.gameObject);
		Debug.Log ("Player added"); 
	}

	/* Accessors */
	int flipStance (){
		stance = stance ^ Modes.Stance.Crouch;
		return (int)stance;
	}
	void setAttackMode(Modes.Attack mode){
		atkMode = mode;
	}

	Modes.Attack getAttackMode(Modes.Attack mode){
		return atkMode;
	}

	void subtractHealth(double damage){
		health -= damage;
	}
	double getHealth(){
		return health;
	}

	/** 
	 * Callback indicating that the enemy (passed in as an argument)
	 * was killed. 
	 * Use this for maintaining internal state; all game statistics are 
	 * already handled by the GameDelegate by the time this method is called.
	 */

	void didHitEnemy(GameObject enemy){
		// Stub: implement for custom behavior.
	}
	/** 
	 * Callback indicating that the player was hit by an enemy, with a given attack.
	 * 
	 * Use this for maintaining internal state; all game statistics are 
	 * already handled by the GameDelegate by the time this method is called.
	 */

	void didReceiveDamage(GameObject enemy, Modes.Attack attack){
		// Stub: implement for custom behavior
	}
			
	/* Set player controls. */
	void Update () {
		Transform trans = this.gameObject.transform;
		
		int xmove = System.Convert.ToInt32(Input.GetKey(Right)) - System.Convert.ToInt32(Input.GetKey(Left));
		int ymove = System.Convert.ToInt32(Input.GetKey(Up)) - System.Convert.ToInt32(Input.GetKey(Down));

		trans.position = new Vector3 (trans.position.x + xmove * moveIncrement, 
									  trans.position.y + ymove * moveIncrement, 
									  trans.position.z);
	}
}
}
