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
    private BehaviorParameters behaviorParameters;
    private CarController carController;
    private int changeCounter;
    private int? prevDiscreteAction0;
    private int? prevDiscreteAction1;

    private float horizontal;
    private float vertical;

    public override void Initialize()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        carController = GetComponent<CarController>();
        carController.autopilot = behaviorParameters.BehaviorType == BehaviorType.Default || behaviorParameters.BehaviorType == BehaviorType.InferenceOnly;
    }

    public override void Heuristic(in ActionBuffers actionsOut) { }

    public override void OnEpisodeBegin()
    {
        carController.Reset();
        changeCounter = 0;
        prevDiscreteAction0 = null;
        prevDiscreteAction1 = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localRotation.y);
        sensor.AddObservation(changeCounter);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (prevDiscreteAction0 != null && prevDiscreteAction1 != null)
        {
            if (prevDiscreteAction0 != actions.DiscreteActions[0] && prevDiscreteAction1 != actions.DiscreteActions[1])
            {
                changeCounter++;
                prevDiscreteAction0 = actions.DiscreteActions[0];
                prevDiscreteAction1 = actions.DiscreteActions[1];
                AddReward(-0.4f * changeCounter);
            }
        }
        else
        {
            prevDiscreteAction0 = actions.DiscreteActions[0];
            prevDiscreteAction1 = actions.DiscreteActions[1];
            changeCounter++;
        }

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


        if (carController.prevRotationY > transform.localRotation.eulerAngles.y)
        {
            // Debug.Log("Y");
            AddReward(1.0f);
        }
        //else if (carController.prevRotationY < transform.localRotation.y)
        //{
        //    AddReward(-45.0f);
        //}
        //else
        //{
        //    AddReward(0.1f);
        //}

        //if (carController.prevPositionX < transform.localPosition.x)
        //{
        //    Debug.Log("X");
        //    AddReward(0.2f);
        //}

        if (carController.transform.localRotation.eulerAngles.y < 1)
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
