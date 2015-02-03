using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

	void Update () {
        transform.position = startPos + (Input.mousePosition - new Vector3(Screen.width/2, Screen.height/2, 0)) * 0.01f;
	}
}
