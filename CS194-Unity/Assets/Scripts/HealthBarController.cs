using UnityEngine;
using System.Collections;

public class HealthBarController : MonoBehaviour {
	private Camera cam;
	private GameObject p1healthbar;
	private GameObject p2healthbar;
	private GameObject base1;
	private GameObject base2;
	private GameObject bar1;
	private GameObject bar2;
	private GameObject top1;
	private GameObject top2;
	private GameObject bottom1;
	private GameObject bottom2;
	private float baseXScale;
	private float edgeYZScale;
	private float distance;
	private float xOffset;
	private float yOffset;
	private float barHeight;
	private float barWidth;
	void Start () {
		// GET OBJECTS
		GameObject cameraObj = GameObject.Find ("Camera");
		cam = cameraObj.GetComponent<Camera> ();
		p1healthbar = GameObject.Find ("Player1HB");
		p2healthbar = GameObject.Find ("Player2HB");
		base1 = GameObject.Find ("Base1");
		base2 = GameObject.Find ("Base2");
		bar1 = GameObject.Find ("Bar1");
		bar2 = GameObject.Find ("Bar2");
		top1 = GameObject.Find ("Top1");
		top2 = GameObject.Find ("Top2");
		bottom1 = GameObject.Find ("Bottom1");
		bottom2 = GameObject.Find ("Bottom2");
		// SET TRANSFORM ATTRIBUTES
		baseXScale = 0.05f;
		edgeYZScale = 0.05f;
		distance = 0.5f;
		xOffset = 0.05f;
		yOffset = 0.05f;
		barHeight = 0.10f;
		barWidth = 0.4f;
		// SET INITIAL TRANSFORM
		base1.transform.localScale = new Vector3 (baseXScale, 1.0f, 1.0f);
		bar1.transform.localScale = new Vector3 (1.0f - baseXScale, 1.0f - (2.0f * edgeYZScale), 1.0f - (2.0f * edgeYZScale));
		top1.transform.localScale = new Vector3 (1.0f - baseXScale, edgeYZScale, 1.0f);
		bottom1.transform.localScale = new Vector3 (1.0f - baseXScale, 1.0f, edgeYZScale);
		base2.transform.localScale = new Vector3 (baseXScale, 1.0f, 1.0f);
		bar2.transform.localScale = new Vector3 (1.0f - baseXScale, 1.0f - (2.0f * edgeYZScale), 1.0f - (2.0f * edgeYZScale));
		top2.transform.localScale = new Vector3 (1.0f - baseXScale, edgeYZScale, 1.0f);
		bottom2.transform.localScale = new Vector3 (1.0f - baseXScale, 1.0f, edgeYZScale);
		base1.transform.localPosition = new Vector3 (baseXScale * 0.5f, 0.0f, 0.0f);
		bar1.transform.localPosition = new Vector3 (((1.0f - baseXScale) * 0.5f) + baseXScale, 0.0f, 0.0f);
		top1.transform.localPosition = new Vector3 (((1.0f - baseXScale) * 0.5f) + baseXScale, ((1.0f - edgeYZScale) * 0.5f), 0.0f);
		bottom1.transform.localPosition = new Vector3 (((1.0f - baseXScale) * 0.5f) + baseXScale, 0.0f, ((1.0f - edgeYZScale) * 0.5f));
		base2.transform.localPosition = new Vector3 (baseXScale * -0.5f, 0.0f, 0.0f);
		bar2.transform.localPosition = new Vector3 (((1.0f - baseXScale) * -0.5f) - baseXScale, 0.0f, 0.0f);
		top2.transform.localPosition = new Vector3 (((1.0f - baseXScale) * -0.5f) - baseXScale, ((1.0f - edgeYZScale) * 0.5f), 0.0f);
		bottom2.transform.localPosition = new Vector3 (((1.0f - baseXScale) * -0.5f) - baseXScale, 0.0f, ((1.0f - edgeYZScale) * 0.5f));
		Vector3 wpLeft = cam.ViewportToWorldPoint(new Vector3(xOffset, 1.0f - yOffset - (barHeight * 0.5f), distance));
		Vector3 wpRight = cam.ViewportToWorldPoint(new Vector3(xOffset + barWidth, 1.0f - yOffset - (barHeight * 0.5f), distance));
		float wpBarWidth = Vector3.Distance (wpLeft, wpRight);
		Vector3 wpTop = cam.ViewportToWorldPoint(new Vector3(xOffset, 1.0f - yOffset, distance));
		Vector3 wpBottom = cam.ViewportToWorldPoint(new Vector3(xOffset, 1.0f - yOffset - barHeight, distance));
		float wpBarHeight = Vector3.Distance (wpTop, wpBottom);
		p1healthbar.transform.localScale = new Vector3 (wpBarWidth, wpBarHeight, wpBarHeight);
		p2healthbar.transform.localScale = new Vector3 (wpBarWidth, wpBarHeight, wpBarHeight);
	}
	public void setPercent(bool player2, float percent) {
		if (percent < 0.01f) {
			percent = 0.01f;
			if (player2)
				bar2.SetActive (false);
			else
				bar1.SetActive (false);
		} else if (percent > 1.0f) {
			percent = 1.0f;
		}
		if (player2) {
			Vector3 newScale = bar2.transform.localScale;
			newScale.x = (1.0f - baseXScale) * percent;
			bar2.transform.localScale = newScale;
			Vector3 newPosition = bar2.transform.localPosition;
			newPosition.x = (newScale.x * -0.5f) - baseXScale;
			bar2.transform.localPosition = newPosition;
		} else {
			Vector3 newScale = bar1.transform.localScale;
			newScale.x = (1.0f - baseXScale) * percent;
			bar1.transform.localScale = newScale;
			Vector3 newPosition = bar1.transform.localPosition;
			newPosition.x = (newScale.x * 0.5f) + baseXScale;
			bar1.transform.localPosition = newPosition;
		}
	}
	public void remoteUpdate() {
		p1healthbar.transform.position = cam.ViewportToWorldPoint (new Vector3(xOffset, 1.0f - yOffset - (barHeight * 0.5f), distance));
		p2healthbar.transform.position = cam.ViewportToWorldPoint (new Vector3(1.0f - xOffset, 1.0f - yOffset - (barHeight * 0.5f), distance));
		p1healthbar.transform.eulerAngles = cam.transform.eulerAngles;
		p2healthbar.transform.eulerAngles = cam.transform.eulerAngles;
		//p1healthbar.transform.Rotate (45.0f, 0.0f, 0.0f);
		//p2healthbar.transform.Rotate (45.0f, 0.0f, 0.0f);
	}
}