using UnityEngine;
using System.Collections;

public class MenuInfo : MonoBehaviour {

	private bool p1AI;
	private bool p2AI;
	// Use this for initialization
	void Start () {
		p1AI = false;
		p2AI = false;
	}

	public void p1isAI(){
		p1AI = true;
	}
	public void p2isAI(){
		p2AI = true;
	}
	public bool isp1AI(){
		return p1AI;
	}
	public bool isp2AI(){
		return p2AI;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
