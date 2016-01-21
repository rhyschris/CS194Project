using UnityEngine;
using System.Collections;

public class CollisionController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {

	}

	void OnCollisionEnter(Collision col){
		if (col.gameObject == GameObject.Find("Player2")) {
			Debug.Log ("Player 2 hit player 1");
		} else if (col.gameObject == GameObject.Find("Player1")) {
			Debug.Log ("Player 1 hit player 2");
		}
	}
}
