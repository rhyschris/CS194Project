using AssemblyCSharp;
using System;
using System.Threading;
using System.Net.Sockets;

using UnityEngine;

public class AI {


	/* Cache the last action */
	private Action lastChoice;
	private byte[] buffer;
	private AsyncAIClient network;


	public AI (int inPort, int outPort) {
		buffer = new byte[Constants.UDP_MAX];
		lastChoice = new Action ();
		network = new AsyncAIClient ("127.0.0.1", inPort, outPort); 
		/* Set up server */
		network.connectAsync ();
	}

	/**
	 * Checks the result of the asynchronous TCP / UDP setup, 
	 * returns True if it was set up correctly, false otherwise.
	 */
	public bool verifyNetwork(){
		ManualResetEvent connectionSignal = network.getConnectedSignal ();

		try {
			bool avail = connectionSignal.WaitOne (AsyncAIClient.serverTimeout);
			if (!avail){ 
				//network.closeSockets();
			}
			return true;
		} catch (Exception e){
			Debug.Log(e);
			return false;	
		}
	}
	/**
	 * Entry point for a GameDelegate to ask for an AI's
	 * next move by polling its client's non-blocking socket.
	 * If no action update is known, then the last 
	 * policy is returned by default.  
	 * 
	 */
	public Action queryAction(GameState state) {
		if (network.readUnblocked (buffer)) {
			lastChoice = decodeAction ();
		}
		return lastChoice;
	}
	/**
	 * Interprets the byte array to retrieve an action
	 * corresponding to a GameState (but not necessarily 
	 * the most recent request or the last frame) from
	 * a valid buffer.
	 * 
	 * A discrete action can then be described by 
	 * (AttackType || ActionType.attack ? 1 : 0 || ActionType)
	 * and the action types set by masking with the constant masks below.
	 * 
	 * See Action.cs for mask and enum values.  
	 */
	private Action decodeAction(){
		int actionFlags = (short)buffer [0];

		Action action = new Action ();
		action.actionType = (ActionType) (actionFlags & Action.ACTION_MASK);

		/* Current behavior doesn't allow attacking during movement.  
		 * FIXME: Delete next line when this is no longer true.
		 */
		/*if ((actionFlags & Action.IS_ATTACK_MASK) != 0) {
			action.actionType = ActionType.attack;
		}	
	   */
		action.oldXPosition = (float)(actionFlags & Action.XPOS_MASK);
		//action.attackType = (AttackType)(actionFlags & Action.ATTACK_MASK);
		return action;
	}
}
