using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    public float zoom;
    public Transform followObject;
    public float followObjectMouseScale = 0;
    void LateUpdate()
    {
        GetComponent<Camera>().orthographicSize = Mathf.Pow(2,zoom);
        if (followObject) {
            Vector3 pos = followObject.position;
            pos.z = transform.position.z; // prevent z from changing
            
            // mouse offset
            Vector2 mouseOffset = (Input.mousePosition / new Vector2(Screen.width, Screen.height)) * 2f - Vector2.one;
            if (mouseOffset.magnitude > 1.0f) mouseOffset.Normalize();
            pos += (Vector3)(mouseOffset * followObjectMouseScale);
            
            transform.position = pos;
        }
    }
}
