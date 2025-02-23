using UnityEngine;
using Unity.Netcode;

public class LookAt : NetworkBehaviour
{
    public static LookAt Instance {get ; private set;}
    public bool isFiring = false;
    public float rotationSpeed = 200f; 
    public Transform bodyRotation;
    void Update()
    {
        if(!IsOwner)
        {
           
            return;
        }
        float horizontalInput = AimBase.Instance.Horizontal;
        float verticalInput = AimBase.Instance.Vertical;
        Vector2 direction  = new Vector2(horizontalInput,verticalInput).normalized;

        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            isFiring = true;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, bodyRotation.rotation, rotationSpeed * Time.deltaTime);
            isFiring = false;
        }
    }

    public bool FiringCheck()
    {
        return isFiring;
    }
}
