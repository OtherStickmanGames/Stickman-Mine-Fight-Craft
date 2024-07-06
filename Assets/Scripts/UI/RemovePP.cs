using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemovePP : MonoBehaviour
{
    [SerializeField] Button btnRemove1;
    [SerializeField] Button btnRemove2;

    private void Start()
    {
        btnRemove1.onClick.AddListener(Remove1_Clicked);
        btnRemove2.onClick.AddListener(Remove2_Clicked);
    }

    private void Remove2_Clicked()
    {
        string clientIdKey = ClientIdentifier.ClientIdKeyPrefix + 2;
        PlayerPrefs.DeleteKey(clientIdKey);
    }

    private void Remove1_Clicked()
    {
        string clientIdKey = ClientIdentifier.ClientIdKeyPrefix + 1;
        PlayerPrefs.DeleteKey(clientIdKey);
    }
}
