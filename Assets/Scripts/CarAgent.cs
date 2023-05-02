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
    private int isBrakingChangeCounter;
    private int actionChangeCounter;
    private int? prevDiscreteAction0;
    private int? prevDiscreteAction1;
    private int? prevDiscreteAction2;


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
        actionChangeCounter = 0;
        prevDiscreteAction0 = null;
        prevDiscreteAction1 = null;
        prevDiscreteAction2 = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(actionChangeCounter);
        sensor.AddObservation(carController.GetRPM());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (prevDiscreteAction0 != null && prevDiscreteAction1 != null)
        {
            if (prevDiscreteAction0 != actions.DiscreteActions[0] || prevDiscreteAction1 != actions.DiscreteActions[1])
            {
                actionChangeCounter++;
                prevDiscreteAction0 = actions.DiscreteActions[0];
                prevDiscreteAction1 = actions.DiscreteActions[1];
                AddReward(-0.6f * actionChangeCounter);
            }
        }
        else
        {
            actionChangeCounter++;
            prevDiscreteAction0 = actions.DiscreteActions[0];
            prevDiscreteAction1 = actions.DiscreteActions[1];
        }

        if (prevDiscreteAction2 == null)
        {
            isBrakingChangeCounter++;
            prevDiscreteAction2 = actions.DiscreteActions[2];
        }
        else
        {
            if (prevDiscreteAction2 != actions.DiscreteActions[2])
            {
                isBrakingChangeCounter++;
                prevDiscreteAction2 = actions.DiscreteActions[2];
                AddReward(-0.02f * isBrakingChangeCounter);
            }
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
            AddReward(3.2f);
        }

        if (carController.transform.localRotation.eulerAngles.y < 1 && carController.transform.localPosition.x > 0)
        {
            AddReward(45.0f);
            Debug.Log("Rotation success!");
            EndEpisode();
        }

        AddReward(-0.00025f * StepCount);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "WallHorizontal")
        {
            EndEpisode();
        }
        else if (other.transform.tag == "WallVertical")
        {
            EndEpisode();
        }
    }
}
