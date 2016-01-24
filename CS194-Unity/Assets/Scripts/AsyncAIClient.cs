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

		/**
		 * ManualResetEvents are boolean signaling mechanism.
		 * Unlike semaphores, no thread "holds" a ManualResetEvent:
		 * Any threads waiting on ManualResetEvents are signaled
		 * once the internal state is set.  
		 * 
		 * Main methods: .Set() sets internals state to "true"
		 * .WaitOne() "blocks" on the event (in this case, blocking 
		 * yields the queue to the next function, which should be a scheduled callback.
		 * 
		 * connectedSignal must be in a global namespace, so it is static.
		 */

		private static ManualResetEvent connectedSignal = new ManualResetEvent(false);
		private static ManualResetEvent sentSignal = new ManualResetEvent(false);

		/* UDP socket for receiving messages */
		private Socket client;
		/* TCP socket for verifying whether the server exists (debugging), general commands */
		private Socket clientAuth; 

		private int serverTimeout = 100; // Wait 100 ms if necessary
		private int port;

		public AsyncAIClient (int port)	{
			this.port = port;
			Debug.Log ("HI from async client!");
		}
		/**
		 * Asynchronously connects to the localhost server on the cached port number.
		 * Sets up a TCP socket, and if binding is successful, sets up a corresponding UDP socket
		 * 
		 * Returns True if the connection was successful, false otherwise.
		 */
		public bool connectAsync(){
			try { 
				// Create TCP socket for authenticated communication 
				IPEndPoint endpt = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port); 

				clientAuth = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				clientAuth.BeginConnect(endpt, new AsyncCallback(connectCallback), clientAuth);

				bool wasSignaled = connectedSignal.WaitOne(serverTimeout);

				if (!wasSignaled){
					Debug.Log("ClientAuth did not reach the destination");
					clientAuth.Close();
					return false;
				}
				/* Set up nondurable UDP */
				client = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				client.Blocking = false;
				// set default host: same endpoint (doesn't do anything for UDP)
				client.Connect (endpt);

				// send UDP packet directly to listening AI server.
				byte[] byteData = Encoding.ASCII.GetBytes("hello world\n");
				client.Send (byteData, SocketFlags.None);

				return true;

			} catch (Exception e){
				Debug.Log (e);
				return false;
			}
		}

		/* Delegate method to be called upon connecting to an AI server asynchronously.
		 * Params: IAsyncResult: */
		private static void connectCallback(IAsyncResult ar){
			
			try {
				Socket clientState = (Socket)ar.AsyncState;
				// Complete connection
				clientState.EndConnect (ar);
				Debug.Log ("Completed connection to localhost on port 5001");
				connectedSignal.Set ();

			/* TODO: More verbose exception handling (i.e. for ConnectionRefused) */
			} catch (Exception unexpected){
				Debug.Log (unexpected);
			}
		}
	
	}

}

