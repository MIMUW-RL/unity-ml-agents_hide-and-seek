using UnityEngine;

public class AgentActions : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float drag = 0.3f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private bool isHiding = true;

    private Vector2 movementInput = Vector2.zero;
    private float rotationInput = 0f;

    private BoxHolding grabbedBox;
    private Quaternion targetRelativeRotation;

    public bool IsHiding { get { return isHiding; } }


    private void FixedUpdate()
    {
        // Apply movement
        Vector2 direction = movementInput.normalized;
        Vector3 force = direction.x * transform.right + direction.y * transform.forward;
        rigidbody.AddForce(force * runSpeed, ForceMode.Impulse);

        // Additional movement drag
        Vector3 currentVel = new Vector3(rigidbody.velocity.x, Mathf.Max(0f, rigidbody.velocity.y), rigidbody.velocity.z);
        Vector3 dragForce = -currentVel * drag;
        rigidbody.AddForce(dragForce, ForceMode.Impulse);

        movementInput = Vector2.zero;

        // Adjust grabbed object position / rotation
        if (grabbedBox != null)
        {
            // Adjust position
            Vector3 targetPosition = transform.position + transform.forward * 5f;
            Vector3 towards = targetPosition - grabbedBox.Rigidbody.position;
            grabbedBox.Rigidbody.velocity = towards * 10f;

            // Adjust rotation
            Quaternion targetRotation = transform.rotation * targetRelativeRotation;
            Vector3 angularTowards = ShortestPathFromTo(grabbedBox.transform.rotation, targetRotation);
            grabbedBox.Rigidbody.angularVelocity = angularTowards * 0.1f;

            // Break in case the object is too far from holder
            if (Vector3.Distance(grabbedBox.Rigidbody.position, transform.position) > 8f)
            {
                grabbedBox.Release();
                grabbedBox = null;
            }
        }
    }

    private void Update()
    {
        float delta = rotationInput * rotationSpeed * Time.deltaTime;
        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.AngleAxis(delta, Vector3.up));

        rotationInput = 0f;
    }


    // Shortest path rotation from one quaternion to another, returned as euler angles
    private Vector3 ShortestPathFromTo(Quaternion from, Quaternion to)
    {
        Quaternion q = Quaternion.Inverse(from) * to;
        Vector3 v = q.eulerAngles;
        float x = v.x > 180f ? v.x - 360f : v.x;
        float y = v.y > 180f ? v.y - 360f : v.y;
        float z = v.z > 180f ? v.z - 360f : v.z;
        return new Vector3(x, y, z);
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
                    BoxHolding boxHolding = hit.collider.gameObject.GetComponent<BoxHolding>();
                    if (boxHolding.TryGrab(this))
                    {
                        grabbedBox = boxHolding;
                        // Keep rotation of the object relative to the agent
                        targetRelativeRotation = Quaternion.Inverse(transform.rotation) * grabbedBox.transform.rotation;
                    }
                }
            }
        }
        else
        {
            grabbedBox.Release();
            grabbedBox = null;
        }
    }

    public void LockBox()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Box"))
            {
                BoxHolding boxHolding = hit.collider.gameObject.GetComponent<BoxHolding>();
                boxHolding.TryLockUnlock(this);
            }
        }
    }



    private void OnDrawGizmos()
    {
        if (grabbedBox != null)
        {
            Gizmos.DrawLine(transform.position, grabbedBox.transform.position);
        }
    }
}
