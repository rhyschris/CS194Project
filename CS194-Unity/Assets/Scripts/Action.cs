public class Action {
	public ActionType actionType;
	public AttackType attackType;
	public float oldXPosition;
	public float distanceMoved;
	public Action() {
		actionType = ActionType.doNothing;
		attackType = AttackType.noAttack;
		oldXPosition = 0.0f;
		distanceMoved = 0.0f;
	}
}

public enum ActionType {
	walkTowards,
	runTowards,
	moveAway,
	blockUp,
	blockDown,
	crouch,
	doNothing,
	attack
};

public enum AttackType {
	noAttack,
	attack1,
	attack2,
	attack3,
	attack4
};
