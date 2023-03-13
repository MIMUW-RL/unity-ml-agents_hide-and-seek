using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HideAndSeekAgent : Agent
{
    [SerializeField] private AgentActions agentActions = null;

    public override void CollectObservations(VectorSensor sensor)
    {
        // self -- 8 floats
        sensor.AddObservation(transform.position);
        sensor.AddObservation(NormalizeAngle(transform.rotation.eulerAngles.y));
        sensor.AddObservation(agentActions.Rigidbody.velocity);
        sensor.AddObservation(agentActions.IsHiding);
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
            agentActions.LockBox();
        }

        //AddReward(GameController.Instance.HidersReward * (agentActions.IsHiding ? 1 : -1));
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
