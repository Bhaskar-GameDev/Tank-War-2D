using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLock : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f,0f,0f);
    }
}
