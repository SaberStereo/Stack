using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
	//camera的跟随和backgroundcolor的改变
	Camera Ccamera;
	Color targetColor;

	Vector3 offset;
	Vector3 DesiredPos;

	void Start () {
		offset = this.gameObject.transform.position;
		DesiredPos = offset;

		Ccamera = GetComponentInChildren<Camera> ();

		Ccamera.backgroundColor = Random.ColorHSV (0.2f, 1f, 0.2f, 0.7f, 0.55f, 1f);
		targetColor = Ccamera.backgroundColor;
	}

	void Update () {

		this.transform.position = Vector3.Lerp (this.transform.position, DesiredPos, 3f * Time.deltaTime);

		Ccamera.backgroundColor = Color.Lerp (Ccamera.backgroundColor, targetColor, Time.deltaTime);
	}

	public void ChangeCameraBColor(){
		targetColor = Per15StepAddCameraColor (Ccamera.backgroundColor);
	}

	Color Per15StepAddCameraColor(Color color){//改变相机的solid color的颜色
		float H, S, V;
		Color.RGBToHSV (color, out H, out S, out V);
		if (Random.Range (-1, 1) >= 0) {
			H += Random.Range (0.2f, 0.3f);
			S += Random.Range (0.1f, 0.2f);
		} else {
			H -= Random.Range (0.2f, 0.3f);
			S -= Random.Range (0.1f, 0.2f);
		}
		//Debug.Log (H);
		//Debug.Log (S);//测试H和S的变化大小
		return Color.HSVToRGB (H, S, V);
	}

	public void SetMovement(Vector3 targetPos){
		DesiredPos = targetPos + offset;
	}
}
