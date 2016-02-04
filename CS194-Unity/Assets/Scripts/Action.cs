public class Action {

	public const ActionType VMOVE_MASK = (ActionType)0x3; // 2 l.s. bits
	public const ActionType HMOVE_MASK = (ActionType)0x1e; // 3-5th l.s. bits
	public const ActionType ATTACK_MASK = (ActionType)0xf0; // 6-8th l.s. bits
	public const long XPOS_MASK = 0xffffffff00; // 9 .. 40 l.s. bits

	public ActionType actionType;
	public float oldXPosition;
	public float distanceMoved;


	public Action() {
		actionType = ActionType.doNothing;
		oldXPosition = 0.0f;
		distanceMoved = 0.0f;
	}
}

/** Shift the action types for convenient network IO
 *  A discrete action can then be described by a bitwise OR of the appropriate
 *  actions
 * 
 *  The following sets of actions are mutually exclusive:
 *  (doNothing)
 *  (jump, crouch)
 *  (walkTowards, runTowards, moveAway)
 *  (attack1-4, blockUp, blockDown)
 * 
 *  A network packet for an action is expected to look like (big endian):
 *  -------------------------------------------------------------------------
 *  LSB Bits-  (40 ... 9)     (8, 7, 6)             (5, 4, 3)              (2, 1)         
 *  Message -   oldXPos   || [Attack type] || [Horizontal movement] || [Vertical Movement] 
 * ---------------------------------------------------------------------------
 * 
 *  Blocks cannot be done at the same time as attacking.  Some attacks
 *  may not be compatible with crouching, but are not programmatically disallowed.
 */

public enum ActionType : byte {
	doNothing = 0,
	crouch = 0x1,
	jump = 0x3,
	walkTowards = (0x1 << 2),
	runTowards = (0x2 << 2),
	moveAway = (0x3 << 2),
	blockUp = (0x1 << 4),
	blockDown = (0x2 << 4),
	attack1 = (0x3 << 4),
	attack2 = (0x4 << 4),
	attack3 = (0x5 << 4),
	attack4 = (0x6 << 4)
};
	