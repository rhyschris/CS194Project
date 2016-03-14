using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	bool hva = false;
	bool ava = false;
	private KeyCode Quit;

	// Use this for initialization
	void Start () {
		Quit = KeyCode.Escape;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (Quit)) {
			Application.Quit ();
		}
	}


	// need to figure out how to set hva and ava too
	public void loadhvhLevel() {
		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadhvaLevel() {
		// flag for one AI
		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadavaLevel() {
		// flag for two AI
		SceneManager.LoadScene ("Scenes/Arena");
	}
		
}
