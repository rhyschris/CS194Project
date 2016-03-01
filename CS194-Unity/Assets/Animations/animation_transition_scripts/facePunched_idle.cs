﻿using UnityEngine;
using System.Collections;

public class facePunched_idle : BufferedStateMachineBehavior {


	public facePunched_idle () {
		this.startBufferTime = 0.1f;
		this.endBufferTime = 0.9f;
	}

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Debug.Log ("OnStateEnter");
		animator.SetBool ("facePunched", false);
		animator.SetBool ("active", false);

		Debug.Log ("animator: " + animator);
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

		Debug.Log ("animator: " + animator);
		Debug.Log (stateInfo.normalizedTime);
		if (stateInfo.normalizedTime >= startBufferTime && stateInfo.normalizedTime <= endBufferTime) {
			if (!animator.GetBool ("active")) { 
				animator.SetBool ("active", true);
				Debug.Log ("active!!!");
			}
		} else {
			if (animator.GetBool ("active")) {
				animator.SetBool ("active", false);
				Debug.Log ("inactive!!!");
			}
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Debug.Log ("EXIT: startBufferTime: " + startBufferTime);
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}