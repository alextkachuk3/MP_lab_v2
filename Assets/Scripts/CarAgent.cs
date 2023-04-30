using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private BehaviorParameters behaviorParameters;
    private CarController carController;

    private float horizontal;
    private float vertical;

    public override void Initialize()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        behaviorParameters = GetComponent<BehaviorParameters>();
        carController = GetComponent<CarController>();
        carController.autopilot = behaviorParameters.BehaviorType == BehaviorType.Default;
    }

    public override void Heuristic(in ActionBuffers actionsOut) { }

    public override void OnEpisodeBegin()
    {
        carController.Reset();

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localRotation.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        switch (actions.DiscreteActions[0])
        {
            case 0:
                horizontal = -1f;
                break;
            case 1:
                horizontal = 0f;
                break;
            case 2:
                horizontal = 1f;
                break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0:
                vertical = -1f;
                break;
            case 1:
                vertical = 0f;
                break;
            case 2:
                vertical = 1f;
                break;
        }

        bool isBraking = actions.DiscreteActions[2] == 1;

        carController.SetInputs(horizontal, vertical, isBraking);

        if (carController.prevRotationY > transform.localRotation.y)
        {
            // Debug.Log("Meow");
            AddReward(1.0f);
        }
        else if (carController.prevRotationY < transform.localRotation.y)
        {
            // AddReward(-45.0f);
        }
        else
        {
            // AddReward(0.1f);
        }

        if (carController.transform.localRotation.y < 0.02)
        {
            AddReward(40.0f);
            Debug.Log("Rotation success!");
            EndEpisode();
        }

        AddReward(-0.0001f * StepCount);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "WallHorizontal")
        {
            //Debug.Log("Horizontal wall hit!");
            // AddReward(-2500.0f);
            EndEpisode();
        }
        else if (other.transform.tag == "WallVertical")
        {
            //Debug.Log("Vertical wall hit!");
            // AddReward(-2500.0f);
            EndEpisode();
        }
    }
}
