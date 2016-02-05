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
	private const int PACKET_LENGTH = 1;


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
				network.closeTCPSocket();
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
		int size = network.readUnblocked (buffer);

		if (size > 0) {
			lastChoice = decodeAction (size);
		} else if (size == -1) {
			Debug.Log ("---ABORT--- ... Uhhh could not read from socket (SocketError)"); 
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
	private Action decodeAction(int bytesFilled){

		/* Take the most recently sent action */
		Action action = new Action ();

		// Change to packetlength
		int read_loc = bytesFilled - PACKET_LENGTH;

		byte[] actionPacket = new byte[PACKET_LENGTH];

		Buffer.BlockCopy(buffer, read_loc, actionPacket, 0, PACKET_LENGTH);
		Debug.Log ("actionPacket: " + actionPacket[0]);
		/* least significant byte contains all of the bitwise or'ed action flags */

		action.actionType = (ActionType) actionPacket[0];

		debugAction (action);
		return action;
	}
	/**
	 * Debugs an action to the Unity console.
	 */
	private void debugAction(Action action){
		switch (action.actionType & Action.HMOVE_MASK) {
		case (ActionType.walkTowards):
			Debug.Log("AI: walking towards");
			break;
		case (ActionType.runTowards):
			Debug.Log ("Walking towards");
			break;
		case (ActionType.moveAway):
			Debug.Log ("Moving away");
			break;
		default:
			Debug.Log ("Not moving");
			break;

		}
		Debug.Log ("action: " + (byte)action.actionType);
	}
}
