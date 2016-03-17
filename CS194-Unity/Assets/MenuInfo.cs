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

	public void p1setAI(bool flag){
		p1AI = flag;
	}
	public void p2setAI(bool flag){
		p2AI = flag;
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
