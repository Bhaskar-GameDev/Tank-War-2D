using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UIForLocalPlayer : NetworkBehaviour
{
    public GameObject[] UIComponents;

    private void Start()
    {
        for (int i = 0; i < UIComponents.Length; i++)
        {
            UIComponents[i].SetActive(IsOwner); 
        }
    }
}
