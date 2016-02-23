using UnityEngine;
using System.Collections;


/**
 * An extension of StateMachineBehavior that supports frame-based buffering relative 
 * to the animations.  
 */

public class BufferedStateMachineBehavior : StateMachineBehaviour{

		
	protected float startBufferTime; // Time % before throwing active behavior on animation, i.e. hitboxes
	protected float endBufferTime; // Time % between ending animation and ending active behavior, i.e. hitboxes

	protected bool active; // Tells you whether the behavior is active or not.

	public BufferedStateMachineBehavior () {
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
}

