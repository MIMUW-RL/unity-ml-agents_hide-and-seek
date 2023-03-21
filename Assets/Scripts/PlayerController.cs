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

    private float pitch = 0f;
    private const float maxPitch = 80f;


    private void Update()
    {
        movementInput = Vector2.zero;
        rotationInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) movementInput += Vector2.up;
        if (Input.GetKey(KeyCode.S)) movementInput += Vector2.down;
        if (Input.GetKey(KeyCode.A)) movementInput += Vector2.left;
        if (Input.GetKey(KeyCode.D)) movementInput += Vector2.right;
        movementInput.Normalize();
        agentMovement.ApplyMovement(movementInput * Time.deltaTime);

        rotationInput += Input.GetAxis("Mouse X") * Vector2.right;
        rotationInput += Input.GetAxis("Mouse Y") * Vector2.up;

        agentMovement.ApplyRotation(rotationInput.x);
        pitch = Mathf.Clamp(pitch - rotationInput.y * mouseSensitivityY * Time.deltaTime, -maxPitch, maxPitch);
        camera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.E)) agentMovement.GrabBox();

        if (!Input.GetKey(KeyCode.E) && agentMovement.IsHolding)
        {
            agentMovement.ReleaseBox();
        }

        if (Input.GetKeyDown(KeyCode.L)) agentMovement.LockBox(true);
        if (Input.GetKeyDown(KeyCode.U)) agentMovement.LockBox(false);
    }
}
