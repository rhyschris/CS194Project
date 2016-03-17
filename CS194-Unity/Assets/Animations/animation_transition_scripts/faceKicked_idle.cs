﻿using UnityEngine;
using System.Collections;

public class faceKicked_idle : BufferedStateMachineBehaviour {

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetBool ("faceKicked", false);
		Debug.Log ("Face kicked enter");
		animator.SetBool ("inAnimation", true);

	}
		

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetBool ("inAnimation", false);
		this.active = false;
	}
		
}
