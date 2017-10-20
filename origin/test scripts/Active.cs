using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Active : MonoBehaviour {

	public GameObject need2disappear;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
			need2disappear.gameObject.SetActive (false);
		}
	}
}
