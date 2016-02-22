using UnityEngine;
using System.Collections;


/**
 * An extension of StateMachineBehavior that supports frame-based buffering relative 
 * to the animations.  
 */

public class BufferedStateMachineBehavior : StateMachineBehaviour{

		
	protected float startBufferTime; // Time % before throwing active behavior on animation, i.e. hitboxes
	protected float endBufferTime; // Time % between ending animation and ending active behavior, i.e. hitboxes


	public BufferedStateMachineBehavior () {
		//Stub implementation
	}

}

