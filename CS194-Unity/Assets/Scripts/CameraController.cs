﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	private Camera mainCamera;
	private HealthBarController healthbarcontroller;
	private PlayerController player1;
	private PlayerController player2;
	public float angle;
	public float xPadding;
	public float widthMinimum;
	// KEYBOARD INPUTS
	private KeyCode anglePlus;
	private KeyCode angleMinus;
	private KeyCode xPaddingPlus;
	private KeyCode xPaddingMinus;
	private KeyCode widthMinimumPlus;
	private KeyCode widthMinimumMinus;

	private float xmoment;
	private float ymoment;
	private float zmoment;

	void Start () {
		mainCamera = Camera.main;
		GameObject healthBars = GameObject.Find ("HealthBars");
		healthbarcontroller = healthBars.GetComponent<HealthBarController> ();
		transform.eulerAngles = new Vector3 (angle, 0.0f, 0.0f);
		GameObject player1Obj = GameObject.Find ("Player1");
		GameObject player2Obj = GameObject.Find ("Player2");
		player1 = player1Obj.GetComponent<PlayerController> ();
		player2 = player2Obj.GetComponent<PlayerController> ();
		anglePlus = KeyCode.I;
		angleMinus = KeyCode.K;
		xPaddingPlus = KeyCode.L;
		xPaddingMinus = KeyCode.J;
		widthMinimumPlus = KeyCode.Period;
		widthMinimumMinus = KeyCode.Comma;

		xmoment = 0.0f;
		ymoment = 0.0f;
		zmoment = 0.0f;
	}
	void LateUpdate() {
		float xTransformRad = transform.eulerAngles.x * Mathf.Deg2Rad;
		float yheight = (player1.getYPos() + player2.getYPos()) * .75f;
		float xwidth = Mathf.Abs(player2.getXPos() - player1.getXPos()) + xPadding;
		xwidth = Mathf.Max (xwidth, widthMinimum);
		float hFOVRad = 2.0f * Mathf.Atan (Mathf.Tan (mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * mainCamera.aspect);
		float distance = (.75f * xwidth) / Mathf.Tan (hFOVRad * 0.5f);
		float yAboveHeight = distance * Mathf.Sin (xTransformRad);
		float zAbsolute = distance * Mathf.Cos (xTransformRad);
		float x = (player1.getXPos() + player2.getXPos()) *.5f;
		float y = yAboveHeight + yheight;
		float z = zAbsolute * -1.0f;

		xmoment = 0.8f * xmoment + 0.2f * x;
		ymoment = 0.8f * ymoment + 0.2f* y;
		zmoment = 0.8f * zmoment + 0.2f * z;

		transform.position = new Vector3 (xmoment, ymoment, zmoment);
		healthbarcontroller.remoteUpdate ();
	}
	public void modAngle(float modBy) {
		angle = angle + modBy;
		transform.eulerAngles = new Vector3 (angle, 0.0f, 0.0f);
	}
	public void modXPadding(float modBy) {
		xPadding = xPadding + modBy;
	}
	public void modWidthMinimum(float modBy) {
		widthMinimum = widthMinimum + modBy;
	}
	public KeyCode getAnglePlus() {
		return anglePlus;
	}
	public KeyCode getAngleMinus() {
		return angleMinus;
	}
	public KeyCode getXPaddingPlus() {
		return xPaddingPlus;
	}
	public KeyCode getXPaddingMinus() {
		return xPaddingMinus;
	}
	public KeyCode getWidthMinimumPlus() {
		return widthMinimumPlus;
	}
	public KeyCode getWidthMinimumMinus() {
		return widthMinimumMinus;
	}
}
