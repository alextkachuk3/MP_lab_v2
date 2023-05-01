using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    private bool isBraking;
    private bool afterReset = false;
    private int resetCounter = 0;

    public double prevRotationY;
    // public double prevPositionX;

    public bool autopilot { get; set; }

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void Reset()
    {
        resetCounter = 10;
        afterReset = true;               
        verticalInput = 0f;
        horizontalInput = 0f;
        isBraking = false;
        currentbreakForce = 0f;
        currentSteerAngle = 0f;
        //frontLeftWheelCollider.steerAngle = 1.0f;
        //frontRightWheelCollider.steerAngle = 1.0f;
        //rearLeftWheelCollider.steerAngle = 1.0f;
        //rearRightWheelCollider.steerAngle = 1.0f;
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;
        rearLeftWheelCollider.motorTorque = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        frontLeftWheelCollider.brakeTorque = Mathf.Infinity;
        frontRightWheelCollider.brakeTorque = Mathf.Infinity;
        rearLeftWheelCollider.brakeTorque = Mathf.Infinity;
        rearRightWheelCollider.brakeTorque = Mathf.Infinity;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    private void FixedUpdate()
    {
        prevRotationY = transform.rotation.eulerAngles.y;
        // prevPositionX = transform.localPosition.x;
        if (afterReset)
        {
            resetCounter -= 1;
            if (resetCounter == 0)
            {
                afterReset = false;
            }            
        }
        else
        {
            if (!autopilot)
            {
                GetInput();
            }
            HandleMotor();
            HandleSteering();
            UpdateWheels();
        }
    }


    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        // Debug.Log(horizontalInput);
        verticalInput = Input.GetAxis("Vertical");
        // Debug.Log(verticalInput);
        isBraking = Input.GetKey(KeyCode.C);
    }

    public void SetInputs(float horizontalInput, float verticalInput, bool isBraking)
    {
        this.horizontalInput = horizontalInput;
        this.verticalInput = verticalInput;
        this.isBraking = isBraking;
    }

    private void HandleMotor()
    {
        currentbreakForce = isBraking ? breakForce : 0f;

        if (verticalInput == 0)
        {
            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
            frontLeftWheelCollider.brakeTorque = 100f;
            frontRightWheelCollider.brakeTorque = 100f;
            rearLeftWheelCollider.brakeTorque = 100f;
            rearLeftWheelCollider.brakeTorque = 100f;
        }
        else
        {
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce;
            ApplyBraking();
        }
    }


    private void ApplyBraking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}