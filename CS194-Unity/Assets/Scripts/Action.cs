﻿public class Action {


	public const long ACTION_MASK = 0x1f; // l.s. 5 bits 
	public const long IS_ATTACK_MASK = 0x10; // 5th bit only
	public const long ATTACK_MASK = 0x1e0; // 6-9th l.s. bits
	public const long XPOS_MASK = 0x1fffffffe00; // 10th - 42nd bits

	public ActionType actionType;
	public float oldXPosition;
	public float distanceMoved;


	public Action() {
		actionType = ActionType.doNothing;
		//attackType = AttackType.noAttack;
		oldXPosition = 0.0f;
		distanceMoved = 0.0f;
	}
}

/** Shift the action types for convenient network IO
 *  A discrete action can then be described by 
 * 
 *  (old_X_position || AttackType || ActionType.attack ? 1 : 0 || ActionType)
 * 
 *  and the action types set by masking with the constant masks below.
 */
public enum ActionType : long {
	doNothing = 0,
	walkTowards,
	runTowards,
	moveAway,
	blockUp,
	blockDown,
	crouch,
	jump, 
	attack1,
	attack2,
	attack3,
	attack4
};
	