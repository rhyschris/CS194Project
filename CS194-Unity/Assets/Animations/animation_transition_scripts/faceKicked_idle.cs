using UnityEngine;
using System.Collections;

public class faceKicked_idle : BufferedStateMachineBehaviour {

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.SetBool ("faceKicked", false);
		Debug.Log ("Face kicked enter");
	}
		
}
