using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text; //For Encoding

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
			Debug.Log ("HI from async client!");
		}
		/**
		 * Asynchronously connects to the localhost server on the cached port number.
		 * Sets up a TCP socket, and a corresponding UDP server.
		 * bound to the same port.  
		 * 
		 * UNDONE: Think about establishing TCP communication to request what port to set up,
		 * in case ports can't be directly forwarded, etc. and Unity doesn't know.
		 * 
		 * Returns True if the connection was successful, false otherwise.
		 */
		public bool connectAsync(){
			try { 
				// Create TCP socket for authenticated communication 
				IPEndPoint endpt = new IPEndPoint(IPAddress.Parse(host), outPort); 
				clientAuth = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				
				clientAuth.BeginConnect(endpt, new AsyncCallback(connectCallback), clientAuth);

				/* Set up nondurable UDP */
				server = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				server.Blocking = false;
				
				// Bind to UDP host port
				IPEndPoint inpt = new IPEndPoint(IPAddress.Parse(host), inPort);
				server.Bind (inpt);
				return true;
		
			} catch (Exception e){
				Debug.Log (e);
				return false;
			}
		}
		public void closeSockets(){
			clientAuth.Close ();
			server.Close ();
		}


		public ManualResetEvent getConnectedSignal(){
			return connectedSignal;
		}
		/**
		 * Polls the UDP socket server to see if there are any new messsages. 
		 * Returns false if nothing is present.
		 */
		public bool readUnblocked (byte[] buffer){

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
					return true;
				} else {
					/* Nothing there */
					return false;
				}
						
			} catch (Exception e){
				Debug.Log(e);
				return false;
			}
		} 
		/* Delegate method to be called upon connecting to an AI server asynchronously.
		 * Params: IAsyncResult: */
		public static void connectCallback(IAsyncResult ar){
			
			try {
				Socket clientState = (Socket)ar.AsyncState;
				// Complete connection
				clientState.EndConnect (ar);
				Debug.Log ("Completed connection to localhost on (port unknown - update msg state");
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

