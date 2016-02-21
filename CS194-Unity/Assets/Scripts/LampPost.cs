using UnityEngine;
using System.Collections;

public class LampPost : MonoBehaviour {

	public Light street_light;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update(){
		StartCoroutine (Flicker());
	}

	IEnumerator Flicker() {
		street_light.enabled = true;
		yield return new WaitForSeconds (Random.Range(1.0f, 60.0f));
		street_light.enabled = false;
		yield return new WaitForSeconds (Random.Range(1.0f,60.0f));
	}
}
