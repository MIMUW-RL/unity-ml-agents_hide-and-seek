using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HideAndSeekAgent : Agent
{
    [SerializeField] private AgentActions agentActions = null;
    [SerializeField] private BufferSensorComponent teamBufferSensor = null;
    [SerializeField] private VectorSensorComponent[] dummyRaycastSensors = null;
    [SerializeField] private int[] dummyRaycastSensorSizes = null;

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 platformCenter = agentActions.GameController.transform.position;

        // self -- 9 floats
        sensor.AddObservation(transform.position - platformCenter);
        sensor.AddObservation(NormalizeAngle(transform.rotation.eulerAngles.y));
        sensor.AddObservation(agentActions.Rigidbody.velocity);
        sensor.AddObservation(agentActions.IsHiding);
        sensor.AddObservation(agentActions.WasCaptured);

        IEnumerable<AgentActions> teamAgents = agentActions.IsHiding
                                             ? agentActions.GameController.GetHiders()
                                             : agentActions.GameController.GetSeekers();

        foreach (AgentActions teamAgent in teamAgents)
        {
            if (teamAgent != agentActions)
            {
                float[] obs = new float[8];
                Vector3 teamAgentPosition = teamAgent.transform.position - platformCenter;
                obs[0] = teamAgentPosition.x;
                obs[1] = teamAgentPosition.y;
                obs[2] = teamAgentPosition.z;
                obs[3] = NormalizeAngle(teamAgent.transform.rotation.eulerAngles.y);
                obs[4] = teamAgent.Rigidbody.velocity.x;
                obs[5] = teamAgent.Rigidbody.velocity.y;
                obs[6] = teamAgent.Rigidbody.velocity.z;
                obs[7] = teamAgent.WasCaptured ? 1.0f : 0.0f;

                teamBufferSensor.AppendObservation(obs);
            }
        }

        for (int i = 0; i < dummyRaycastSensorSizes.Length; i++)
        {
            float[] dummyObs = new float[dummyRaycastSensorSizes[i]];
            dummyRaycastSensors[i].GetSensor().AddObservation(dummyObs);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float movementX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float movementZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        float rotation = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);

        agentActions.ApplyMovement(new Vector2(movementX, movementZ));
        agentActions.ApplyRotation(rotation);

        if (actions.DiscreteActions[0] == 1)
        {
            agentActions.GrabBox();
        }
        else if (agentActions.IsHolding)
        {
            agentActions.ReleaseBox();
        }

        if (actions.DiscreteActions[1] == 1)
        {
            agentActions.LockBox(true);
        }
        else if (actions.DiscreteActions[1] == 2)
        {
            agentActions.LockBox(false);
        }

        if (agentActions.GameController.DebugDrawIndividualReward)
        {
            float reward = GetCumulativeReward();
            Color rewardColor = Color.blue;
            if (reward > 0f) rewardColor = Color.green;
            if (reward < 0f) rewardColor = Color.red;
            Debug.DrawRay(transform.position, 20f * Mathf.Abs(reward) * Vector3.up, rewardColor);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array[1] = 1f;
        actionsOut.ContinuousActions.Array[2] = GetObservations()[7] - 0.5f;
    }


    // Input: angle in degrees
    // Output: angle in radians in range [-pi; pi]
    private float NormalizeAngle(float angle)
    {
        angle = (angle + 180f) % 360f - 180f;
        angle *= Mathf.Deg2Rad;
        return angle;
    }
}
