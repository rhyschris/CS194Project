using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * A GameDelegate supervises the behavior of the players,
 * creates the attack hitboxes, and generally controls the 
 * simplified game state.  
 *
 * A GameDelegate responds to messages from each player, and propagates changes
 * in health from attacks, and confirms a hit.  
 */
namespace AssemblyCSharp {
public class GameDelegate : MonoBehaviour {


	private List<GameObject> players;
	
	/* Beginning of Lifecycle:
	 * Instantiate the player list so that players can 
	 * add themselves to the delegate before updating.
	 */
	void Awake(){ 
		players = new List<GameObject> ();	
		Debug.Log ("Delegate awakened");
	}

	void Start () {
		// pass
	}

	void addPlayer(GameObject player){
		players.Add (player); 
	}

	// Update is called once per frame
	void Update () {
		
	}
	
	/**
	 * Handles the post-frame collision actions; notifies the players
	 *
	 **/
	void LateUpdate(){
		/* TODO: replace with collision logic, make symmetric for p1 -> p2 and p2->p1 */

		if (players.Count <= 1)
			return;
		
		for (int i = 0; i < players.Count; i++) {
			for (int j = 0; j < players.Count; j++) {
				// (attacker, victim) pair
				Pair<GameObject, GameObject> playerPair = Pair.New(players[i], players[j]);
				// Perform hit detection...
				bool gotHit = false;
				if (gotHit) {
					//logic...
				}
			}
		}
	
	}
}
}