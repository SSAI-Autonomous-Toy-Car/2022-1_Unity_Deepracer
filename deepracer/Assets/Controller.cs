using UnityEngine;
using System.Collections;
using System.Collections.Generic;


     
public class Controller : MonoBehaviour {
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    public float maxSteeringAngle;

    
    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) {
            return;
        }
     
        Transform visualWheel = collider.transform.GetChild(0);
     
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
     
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }


    public float CurrentAcceleration
    {
        get => m_currentAcceleration;
        set => m_currentAcceleration = Mathf.Clamp(value, 0f, 1f);
    }

    public float CurrentSteeringAngle
    {
        get => m_currentSteeringAngle;
        set => m_currentSteeringAngle = Mathf.Clamp(value,-1f,1f);
    }
    
    private float m_currentSteeringAngle = 0f;
    private float m_currentAcceleration = 0f;
    
    public void FixedUpdate()
    {
        float motor = maxMotorTorque * m_currentAcceleration;
        float steering = maxSteeringAngle * m_currentSteeringAngle;
     
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
