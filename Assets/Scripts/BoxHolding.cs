using UnityEngine;

public class BoxHolding : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;

    [SerializeField] private Material materialDefault = null;
    [SerializeField] private Material materialLockHider = null;
    [SerializeField] private Material materialLockSeeker = null;

    private MeshRenderer meshRenderer = null;

    private AgentActions owner = null;
    private AgentActions lockOwner = null;


    public AgentActions Owner { get { return owner; } }
    public AgentActions LockOwner { get { return lockOwner; } }
    public Rigidbody Rigidbody { get { return rigidbody; } }


    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }


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
            rigidbody.isKinematic = true;
            lockOwner = agent;
            meshRenderer.material = agent.IsHiding ? materialLockHider : materialLockSeeker;
        }
        else if (lockOwner != null && lockOwner.IsHiding == agent.IsHiding)
        {
            rigidbody.isKinematic = false;
            lockOwner = null;
            meshRenderer.material = materialDefault;
        }
    }
}
