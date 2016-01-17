using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameScript : MonoBehaviour {
	public Text DataText;
	public GameObject player1;
	public GameObject player2;
	public float moveIncrement;
	public Camera perspectiveCamera;
	public float perspXPadding;
	//public float perspYPadding;
	public float perspAngle;
	public float perspWidthMinimum;
	public Camera orthographicCamera;
	//public float orthoPadding;
	//public float orthoAngle;
	//public float orthoDistance;
	//public float orthoWidthMinimum;
	void Start ()
	{
		perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
		//orthographicCamera.transform.eulerAngles = new Vector3 (orthoAngle, 0.0f, 0.0f);
		perspectiveCamera.enabled = true;
		orthographicCamera.enabled = false;
	}
	void Update()
	{
		// QUIT THE GAME
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		// CONTROL THE PLAYERS
		if (Input.GetKey (KeyCode.W)) {
			player1.transform.position = new Vector3 (player1.transform.position.x, player1.transform.position.y + moveIncrement, player1.transform.position.z);
		}
		if (Input.GetKey (KeyCode.A)) {
			player1.transform.position = new Vector3 (player1.transform.position.x - moveIncrement, player1.transform.position.y, player1.transform.position.z);
		}
		if (Input.GetKey (KeyCode.S)) {
			player1.transform.position = new Vector3 (player1.transform.position.x, player1.transform.position.y - moveIncrement, player1.transform.position.z);
		}
		if (Input.GetKey (KeyCode.D)) {
			player1.transform.position = new Vector3 (player1.transform.position.x + moveIncrement, player1.transform.position.y, player1.transform.position.z);
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			player2.transform.position = new Vector3 (player2.transform.position.x, player2.transform.position.y + moveIncrement, player2.transform.position.z);
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			player2.transform.position = new Vector3 (player2.transform.position.x - moveIncrement, player2.transform.position.y, player2.transform.position.z);
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			player2.transform.position = new Vector3 (player2.transform.position.x, player2.transform.position.y - moveIncrement, player2.transform.position.z);
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			player2.transform.position = new Vector3 (player2.transform.position.x + moveIncrement, player2.transform.position.y, player2.transform.position.z);
		}
		// RESET PLAYER POSITIONS
		if (Input.GetKeyDown (KeyCode.Semicolon)) {
			player1.transform.position = new Vector3 (-4.0f, 1.0f, 0.0f);
			player2.transform.position = new Vector3 (4.0f, 1.0f, 0.0f);
		}
		// ALTER PERSPECTIVE CAMERA ANGLE ATTRIBUTES
		if (Input.GetKeyDown (KeyCode.I)) {
			perspAngle = perspAngle - 0.5f;
			perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
		}
		if (Input.GetKeyDown (KeyCode.J)) {
			perspXPadding = perspXPadding - 0.5f;
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			perspAngle = perspAngle + 0.5f;
			perspectiveCamera.transform.eulerAngles = new Vector3 (perspAngle, 0.0f, 0.0f);
		}
		if (Input.GetKeyDown (KeyCode.L)) {
			perspXPadding = perspXPadding + 0.5f;
		}
		if (Input.GetKeyDown (KeyCode.Comma)) {
			perspWidthMinimum = perspWidthMinimum - 0.5f;
		}
		if (Input.GetKeyDown (KeyCode.Period)) {
			perspWidthMinimum = perspWidthMinimum + 0.5f;
		}
		// SWITCH CAMERA
		if (Input.GetKeyDown (KeyCode.Space)) {
			//perspectiveCamera.enabled = !perspectiveCamera.enabled;
			//orthographicCamera.enabled = !orthographicCamera.enabled;
		}
		// UPDATE DATATEXT
		DataText.text =
		"perspAngle:        " + perspAngle.ToString () +
		"\nperspXPadding:     " + perspXPadding.ToString () +
		"\nperspWidthMinimum: " + perspWidthMinimum.ToString ();
	}
	void LateUpdate()
	{
		/*
		float vFOVrad = perspectiveCamera.fieldOfView * Mathf.Deg2Rad;
		*/float xTransformRad = perspectiveCamera.transform.eulerAngles.x * Mathf.Deg2Rad;/*
		float yspan = Mathf.Abs (player1.transform.position.y - player2.transform.position.y) + perspYPadding;
		float a = (Mathf.Pow (Mathf.Cos (xTransformRad), 2.0f) * Mathf.Pow (Mathf.Tan (xTransformRad - (0.5f * vFOVrad)), 2.0f)) +
		          Mathf.Pow (Mathf.Cos (xTransformRad), 2.0f) -
		          (Mathf.Pow (Mathf.Cos (xTransformRad), 2.0f) / Mathf.Pow (Mathf.Tan (xTransformRad + (0.5f * vFOVrad)), 2.0f));
		float b = 2.0f * yspan * Mathf.Cos (xTransformRad) * Mathf.Tan (xTransformRad - (0.5f * vFOVrad));
		float c = Mathf.Pow (yspan, 2.0f);
		float vDistance = ((-1.0f * b) + Mathf.Sqrt (Mathf.Pow (b, 2.0f) - (4.0f * a * c))) / (2.0f * a);
		float zd = vDistance * Mathf.Cos (xTransformRad);
		float yblind = zd * Mathf.Tan (xTransformRad - (0.5f * vFOVrad));
		float ybottom = Mathf.Min (player1.transform.position.y, player2.transform.position.y) - (0.5f * perspYPadding);
		float yheight = yspan - Mathf.Sqrt (Mathf.Pow (vDistance, 2.0f) + Mathf.Pow (zd, 2.0f)) + yblind + ybottom;*/
		float yheight = (player1.transform.position.y + player2.transform.position.y) * 0.5f;
		float xwidth = Mathf.Abs(player2.transform.position.x - player1.transform.position.x) + perspXPadding;
		xwidth = Mathf.Max (xwidth, perspWidthMinimum);
		float hFOVRad = 2.0f * Mathf.Atan (Mathf.Tan (perspectiveCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * perspectiveCamera.aspect);
		float hDistance = (0.5f * xwidth) / Mathf.Tan (hFOVRad * 0.5f);
		float distance = /*Mathf.Max (vDistance, hDistance);*/ hDistance;
		float yAboveHeight = distance * Mathf.Sin (xTransformRad);
		float zAbsolute = distance * Mathf.Cos (xTransformRad);
		float x = (player1.transform.position.x + player2.transform.position.x) * 0.5f;
		float y = yAboveHeight + yheight;
		float z = zAbsolute * -1.0f;
		perspectiveCamera.transform.position = new Vector3 (x, y, z);
		// UPDATE THE ORTHOGRAPHIC CAMERA
		/*width = Mathf.Abs(player2.transform.position.x - player1.transform.position.x + orthoPadding);
		if (width < orthoWidthMinimum) {
			width = orthoWidthMinimum;
		}
		orthographicCamera.orthographicSize = (width * 0.5f) / orthographicCamera.aspect;
		distance = orthoDistance;
		x = (player1.transform.position.x + player2.transform.position.x) * 0.5f;
		y = 0.0f;
		z = 0.0f;
		orthographicCamera.transform.position = new Vector3 (x, y, z);*/
	}
}
