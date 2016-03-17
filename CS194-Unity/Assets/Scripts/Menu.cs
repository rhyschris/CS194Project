using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

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
		MI.p1setAI (false);
		MI.p1setAI (false);
		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadhvaLevel() {
		MI.p1setAI (false);
		MI.p2setAI (true);

		//player2.setPlayerAI ();
		//launchSingleAIScript ("5998");

		SceneManager.LoadScene ("Scenes/Arena");
	}

	public void loadavaLevel() {
		// flag for two AI
		MI.p1setAI(true);
		MI.p2setAI (true);
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
		//launchSingleAIScript ("");
		//launchSingleAIScript ("5998");
	}
}