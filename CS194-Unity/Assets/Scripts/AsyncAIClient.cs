using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text; //For Encoding
using System.Collections.Generic;

// To get Debug log to write somewhere we can see
using UnityEngine;

#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0168 // variable defined but never used
/**
 * Adapts an asynchronous socket to connect to a listening server.
 */

namespace AssemblyCSharp {

	public class AsyncAIClient {

		/** --------------
		 * ManualResetEvents are boolean signaling mechanism.
		 * Unlike semaphores, no thread "holds" a ManualResetEvent:
		 * Any threads waiting on ManualResetEvents are signaled
		 * once the internal state is set.  
		 * 
		 * Main methods: 
		 * .Set() sets internals state to "true"
		 * .WaitOne() "blocks" on the event (in this case, blocking 
		 * yields the queue to the next function, which should be a scheduled callback.
		 * 
		 * connectedSignal must be in a global namespace, so it is static.
		 */

		private static ManualResetEvent connectedSignal = new ManualResetEvent(false);

		/* UDP socket for receiving messages */
		private Socket server;
		/* UDP socket for sending gameState */
		private Socket clientGameState;
		/*Endpoint to send UDP data to */
		IPEndPoint endpt;
		/* TCP socket for verifying whether the server exists (debugging), AI commands */
		private Socket clientAuth; 

		public const int serverTimeout = 100; // Wait 100 ms if necessary
		private string host;

		private int outPort; // Outgoing port for symmetric TCP / UDP 
		private int inPort; // Incoming port for UDP connection 

		public AsyncAIClient (string host, int inPort, int outPort)	{
			this.host = host;
			this.inPort = inPort;
			this.outPort = outPort;
		}
		/**
		 * Asynchronously connects to the localhost server on the cached port number.
		 * Sets up a TCP socket attempting to contact localhost:outPort, and a 
		 * corresponding UDP server, bound to the inPort.
		 * 
		 * UNDONE: Think about establishing TCP communication to request what port to set up,
		 * in case ports can't be directly forwarded, etc. and Unity doesn't know.
		 * 
		 * Returns True if the connection was successful, false otherwise.
		 */
		public bool connectAsync(){
			try { 
				/* Create TCP socket for authenticated communication */
				endpt = new IPEndPoint(IPAddress.Parse(host), outPort); 
				clientAuth = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				clientAuth.BeginConnect(endpt, new AsyncCallback(connectCallback), clientAuth);

				/* Set up nondurable UDP server for receiving actions*/
				server = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				server.Blocking = false;

				/*Set up nondurable UDP client to send gamestate */
				IPEndPoint RemoteEndPoint= new IPEndPoint(IPAddress.Parse(host), outPort);
				clientGameState = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);

				/* Bind to UDP host port */
				IPEndPoint inpt = new IPEndPoint(IPAddress.Parse(host), inPort);
				server.Bind (inpt);
				return true;

			} catch (Exception e){
				Debug.Log (e);
				return false;
			}
		}
		public void closeTCPSocket (){
			clientAuth.Close ();
		}


		public ManualResetEvent getConnectedSignal(){
			return connectedSignal;
		}
		/**
		 * Polls the UDP socket server to see if there are any new messsages. 
		 * Returns:
		 *    size: If bytes have been read into the buffer.
		 *    0:    If no bytes were available, but the poll was successful 
		 *   -1:    If there was some error when trying to read bytes
		 * 
		 * TODO: replace pure numbers with constants
		 */    
		public int readUnblocked (byte[] buffer){

			try {
				/**
				 * Poll() returns true if
				 * 1) Listen() has a connection pending
				 * 2) Data is available to read 
				 * 3) Connection is closed reset, terminated
				 * 
				 * 1) and 3) don't apply because we are the server and UDP
				 * is connectionless, respectively.
				 */

				if (server.Poll(0, SelectMode.SelectRead)){
					int size = server.Available;
					server.Receive(buffer, 0, size, 0);   
					Debug.Log("Received UDP packet with contents: " + Encoding.UTF8.GetString(buffer));
					return size;
				} else {
					/* Nothing there */
					return 0;
				}
			} catch (Exception e){
				Debug.Log(e);
				return -1;
			}
		} 

		public int sendGameState(GameState state){
			List<float> myFloats = state.getFloatList ();

			int numElems = myFloats.Count;
			int width = sizeof(float);
			byte[] data = new byte[myFloats.Count * width+1];

			int i = 0;
			foreach (float f in myFloats)
			{
				
				byte[] converted = BitConverter.GetBytes(f);

				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(converted);
				}

				for (int j = 0; j < width; ++j)
				{
					data[i * width + j] = converted[j];
				}
				i++;
			}
			data[numElems*width] = state.getFlags(); 

			return clientGameState.SendTo(data, myFloats.Count * width+1, SocketFlags.None, endpt);	
		}
		/* Delegate method to be called upon connecting to an AI server asynchronously.
		 * Params: IAsyncResult: */
		public static void connectCallback(IAsyncResult ar){

			try {
				Socket clientState = (Socket)ar.AsyncState;
				// Complete connection
				clientState.EndConnect (ar);
				Debug.Log ("Completed TCP connection to localhost on outPort (default 4999)");
				connectedSignal.Set ();

			} catch (Exception unexpected) {
				if (unexpected is ObjectDisposedException) {
					Debug.Log ("Socket has been closed");
				} else if (unexpected is InvalidOperationException) {
					Debug.Log ("EndConnect was previously called for the asynchronous connection.");
				} else if (unexpected is SocketException) {
					SocketError errcode = (SocketError)((SocketException)unexpected).ErrorCode;
					switch (errcode) {
					case(SocketError.AccessDenied):
						Debug.Log ("Access Denied to socket");
						break;
					case(SocketError.ConnectionRefused):
						Debug.Log ("Connection Refused: Remote host refusing connection");
						break;
					case(SocketError.ConnectionReset):
						Debug.Log ("Connection Reset");
						break;
					case(SocketError.HostUnreachable):
						Debug.Log ("Host Unreachable: No route to remote host");
						break;
					case(SocketError.NetworkUnreachable):
						Debug.Log ("Network Unreachable: No route to remote host");
						break;
					default:
						Debug.Log ("Socket Error. Error Code: " + errcode.ToString ());
						break;
					}
				} else {
					Debug.Log ("Unexpected Error");
					Debug.Log (unexpected);
				}
			}
		}

	}

}

