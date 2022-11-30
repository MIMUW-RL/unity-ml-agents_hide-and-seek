using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AgentActions agentMovement;
    [SerializeField] private new Camera camera;

    private Vector2 rotationInput = Vector2.zero;
    private Vector2 movementInput = Vector2.zero;
    private const float mouseSensitivityY = 360f;


    private void Update()
    {
        movementInput = Vector2.zero;
        rotationInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) movementInput += Vector2.up;
        if (Input.GetKey(KeyCode.S)) movementInput += Vector2.down;
        if (Input.GetKey(KeyCode.A)) movementInput += Vector2.left;
        if (Input.GetKey(KeyCode.D)) movementInput += Vector2.right;
        movementInput.Normalize();

        rotationInput += Input.GetAxis("Mouse X") * Vector2.right;
        rotationInput += Input.GetAxis("Mouse Y") * Vector2.up;

        agentMovement.ApplyRotation(rotationInput.x * Time.deltaTime);
        camera.transform.Rotate(-rotationInput.y * mouseSensitivityY * Time.deltaTime, 0f, 0f, Space.Self);

        if (Input.GetKeyDown(KeyCode.E)) agentMovement.GrabBox();
    }

    private void FixedUpdate()
    {
        agentMovement.ApplyMovement(movementInput);
    }
}
