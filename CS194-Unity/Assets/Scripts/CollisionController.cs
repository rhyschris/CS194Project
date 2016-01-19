using UnityEngine;
using System.Collections;

public class CollisionStuff : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// After detecting collisions
	void OnCollisionEnter(Collision col) {
		if (col.gameObject == GameObject.Find ("Player2"))
			Debug.Log ("Player 2 hit Player 1");
	}
}
