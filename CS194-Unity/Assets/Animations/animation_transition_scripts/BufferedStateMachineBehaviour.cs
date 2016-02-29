using UnityEngine;
using System.Collections;


/**
 * An extension of StateMachineBehavior that supports frame-based buffering relative 
 * to the animations.  
 */

public class BufferedStateMachineBehaviour : StateMachineBehaviour{

		
	protected float startBufferTime; // Time % before throwing active behavior on animation, i.e. hitboxes
	protected float endBufferTime; // Time % between ending animation and ending active behavior, i.e. hitboxes

	protected bool active; // Tells you whether the behavior is active or not.
	protected bool actionTriggered; // Tells you whether 'active' was flipped in the last frame. 

	public BufferedStateMachineBehaviour () {

		startBufferTime = 0.25f;
		endBufferTime = 0.75f;
		//Stub implementation
	}


	override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		if (stateInfo.normalizedTime >= startBufferTime && stateInfo.normalizedTime <= endBufferTime) {
			if (!this.active) { 
				this.active = true;
				this.actionTriggered = true;
			} else {
				this.actionTriggered = false;
			}
		
		} else {
			if (this.active) {
				this.active = false;
				this.actionTriggered = true;
			} else {
				this.actionTriggered = false;
			}

		}
	}

	/**
	 * Returns whether an active behavior is being animated.
	 */
	public bool isActive (){
		return active;
	}
	public bool isTriggered (){
		return actionTriggered;
	}
}

