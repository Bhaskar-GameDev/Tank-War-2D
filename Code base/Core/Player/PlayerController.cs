using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bodyTransform; 
    [SerializeField] private Rigidbody2D rb;          
    [SerializeField] private VariableJoystick joystick;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 4f;  

    private Vector2 previousMovementInput;

    private void Update()
    {
        if (!IsOwner) { return; }

        float horizontalInput = joystick.Horizontal;
        float verticalInput = joystick.Vertical;

        previousMovementInput = new Vector2(horizontalInput, verticalInput);

        if (previousMovementInput.magnitude > 0.2f)
        {
            float targetAngle = Mathf.Atan2(previousMovementInput.x, previousMovementInput.y) * Mathf.Rad2Deg;
            bodyTransform.rotation = Quaternion.Euler(0f, 0f, -targetAngle);
        }

        if (previousMovementInput.magnitude > 0.2f) 
        {
            rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.magnitude * movementSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
