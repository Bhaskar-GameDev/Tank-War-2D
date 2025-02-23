using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarTransform : MonoBehaviour
{
    public Transform turrentTransform;

    private void FixedUpdate()
    {
        transform.position = turrentTransform.position;
        transform.rotation = turrentTransform.rotation;
    }
}
