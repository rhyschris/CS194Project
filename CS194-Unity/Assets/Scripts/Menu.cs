using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	bool hva = false;
	bool ava = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	// need to figure out how to set hva and ava too
	public void loadLevel() {
		if (hva) {
			// flag for one AI
		} else if (ava) {
			// flag for two AIs
		}
		SceneManager.LoadScene ("/Scenes/Arena");
	}
}
