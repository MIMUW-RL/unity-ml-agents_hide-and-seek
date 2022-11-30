using UnityEngine;

public class AgentActions : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;
    [SerializeField] private Rigidbody hands = null;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool isHiding = true;

    private GameObject grabbedBox;
    
    public bool IsHiding { get { return isHiding; } }


    public void ApplyMovement(Vector2 directionLocal)
    {
        Vector3 force = directionLocal.x * transform.right + directionLocal.y * transform.forward;
        force *= movementForce;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

    public void ApplyRotation(float delta)
    {
        float normalizedDelta = delta * rotationSpeed;
        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(normalizedDelta, Vector3.up));
    }


    public void GrabBox()
    {
        if (grabbedBox == null)
        {
            if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Box"))
                {
                    grabbedBox = hit.collider.gameObject;
                    SpringJoint joint = grabbedBox.AddComponent<SpringJoint>();
                    joint.connectedBody = hands;
                    joint.anchor = hit.point - hit.collider.transform.position;
                }
            }
        }
        else
        {
            Destroy(grabbedBox.GetComponent<SpringJoint>());
            grabbedBox = null;
        }
    }



    private void OnDrawGizmos()
    {
        if (grabbedBox != null)
        {
            Gizmos.DrawLine(hands.transform.position, grabbedBox.transform.position);
        }
    }
}
