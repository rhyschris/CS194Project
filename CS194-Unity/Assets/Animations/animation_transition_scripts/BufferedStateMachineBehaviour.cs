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

	public BufferedStateMachineBehaviour () {

		startBufferTime = 0.0f;
		endBufferTime = 0.95f;
		//Stub implementation
	}

	override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		if (stateInfo.normalizedTime >= startBufferTime && stateInfo.normalizedTime <= endBufferTime) {
			if (! this.active) { 
				this.active = true;
			}
		} else {
			if (this.active){
				this.active = false;
			}
		}
	}
	/**
	 * Returns whether an active behavior is being animated.
	 */
	public bool isActive (){
		return active;
	}
}

