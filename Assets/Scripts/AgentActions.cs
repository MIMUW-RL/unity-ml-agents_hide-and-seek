using UnityEngine;

public class AgentActions : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;
    [SerializeField] private Rigidbody hands = null;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float drag = 0.3f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool isHiding = true;

    private Vector2 movementInput = Vector2.zero;
    private float rotationInput = 0f;
    private GameObject grabbedBox;
    
    public bool IsHiding { get { return isHiding; } }


    private void FixedUpdate()
    {
        Vector2 direction = movementInput.normalized;
        Vector3 force = direction.x * transform.right + direction.y * transform.forward;
        rigidbody.AddForce(force * runSpeed, ForceMode.Impulse);

        Vector3 currentVel = new Vector3(rigidbody.velocity.x, Mathf.Max(0f, rigidbody.velocity.y), rigidbody.velocity.z);
        Vector3 dragForce = -currentVel * drag;
        rigidbody.AddForce(dragForce, ForceMode.Impulse);

        movementInput = Vector2.zero;
    }

    private void Update()
    {
        float delta = rotationInput * rotationSpeed * Time.deltaTime;
        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(delta, Vector3.up));

        rotationInput = 0f;
    }


    public void ApplyMovement(Vector2 input)
    {
        movementInput += input * Time.deltaTime;
    }

    public void ApplyRotation(float delta)
    {
        rotationInput += delta;
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
