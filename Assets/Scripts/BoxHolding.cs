using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxHolding : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;

    private AgentActions owner = null;
    private AgentActions lockOwner = null;


    public AgentActions Owner { get { return owner; } }
    public AgentActions LockOwner { get { return lockOwner; } }
    public Rigidbody Rigidbody { get { return rigidbody; } }

    public bool TryGrab(AgentActions agent)
    {
        if (lockOwner != null || owner != null)
        {
            return false;
        }
        owner = agent;
        return true;
    }

    public void Release()
    {
        owner = null;
    }

    public void TryLockUnlock(AgentActions agent)
    {
        if (lockOwner == null && owner == null)
        {
            lockOwner = agent;
        }
        else if (lockOwner != null && lockOwner.IsHiding == agent.IsHiding)
        {
            lockOwner = null;
        }
    }


    private void OnDrawGizmos()
    {
        if (lockOwner != null)
        {
            // TODO: replace with some proper visual indicator
            Gizmos.DrawMesh(GetComponent<MeshFilter>().mesh, transform.position + 0.02f * Vector3.down, transform.rotation, transform.localScale - Vector3.one * 0.1f);
        }
    }
}
