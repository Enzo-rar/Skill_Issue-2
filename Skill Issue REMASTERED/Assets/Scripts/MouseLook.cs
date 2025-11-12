using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    { // Movimiento rat�n
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes = RotationAxes.MouseXandY;
    public float sensitivityHor = 9.0f;    // velocidad
    public float sensitivityVert = 9.0f;
    public float minPitchAngle = -45.0f;   // rango de rotaci�n vertical
    public float maxPitchAngle = 45.0f;

    private float pitchAngle = 0;  // cabeceo (pitch) actual 

    void Update()
    {
        if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
        }
        else
        {
            pitchAngle -= Input.GetAxis("Mouse Y") * sensitivityVert;
            pitchAngle = Mathf.Clamp(pitchAngle, minPitchAngle, maxPitchAngle);
            float yawAngle = transform.localEulerAngles.y; // mantener el mismo �ngulo de gui�ada (yaw)
            if (axes == RotationAxes.MouseXandY)
            {
                yawAngle += Input.GetAxis("Mouse X") * sensitivityHor;
            }
            transform.localEulerAngles = new Vector3(pitchAngle, yawAngle, 0);
        }
    }
}

