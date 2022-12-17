using System;
using System.Collections;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour {
	[SerializeField] private KeyCode screenshotKey;

	void Update() {
		if (Input.GetKeyDown(this.screenshotKey))
			StartCoroutine(this.TakeScreenShot());
	}

	IEnumerator TakeScreenShot() {
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot("Screenshot_" + DateTime.Now.ToString("yy-dd-MM_HH.mm.ss") + ".png");
	}
}
