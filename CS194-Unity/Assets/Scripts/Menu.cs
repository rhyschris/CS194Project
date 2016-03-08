using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	bool hva = false;
	bool ava = false;

	private PlayerController player1;
	private PlayerController player2;

	private MenuInfo MI;
	// Use this for initialization
	void Start () {
		GameObject MIObj = GameObject.Find ("Info");
		MI = MIObj.GetComponent<MenuInfo> ();
		DontDestroyOnLoad (MI);
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	// need to figure out how to set hva and ava too
	public void loadhvhLevel() {
		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadhvaLevel() {
		MI.p2isAI ();
		//player2.setPlayerAI ();
		launchSingleAIScript ("");

		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadavaLevel() {
		// flag for two AI
		MI.p1isAI();
		MI.p2isAI ();
		launchTwoAIScripts ();
		SceneManager.LoadScene ("Scenes/Arena");
	}

	private void launchSingleAIScript(string args){
		Process process = new Process();
		// Configure the process using the StartInfo properties.
		process.StartInfo.FileName = "C:/Python27/python";
		process.StartInfo.Arguments = "C:/Documents/CS194-Unity/ai/agents/basicQlearn.py "+args;
		process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
		process.Start();
	}
	private void launchTwoAIScripts(){
		launchSingleAIScript ("");
		launchSingleAIScript ("5998");
	}
}
