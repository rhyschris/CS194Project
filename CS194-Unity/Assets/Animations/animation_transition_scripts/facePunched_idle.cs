using UnityEngine;
using System.Collections;

public class facePunched_idle : BufferedStateMachineBehaviour {


	public facePunched_idle () {
		this.startBufferTime = 0.0f;
		this.endBufferTime = 0.4f;
	}

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetBool ("facePunched", false);
	}	

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Debug.Log ("EXIT: startBufferTime: " + startBufferTime);
		// In case the animation ends early, we must avoid input holds
		this.active = false;
	}

}
