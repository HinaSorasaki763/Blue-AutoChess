using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleObject : MonoBehaviour
{
    public GameObject objToHandle;
    public Button Button;
    public void Awake()
    {
        Button.onClick.AddListener(Handle);
    }
    public void Handle()
    {
        bool handle = objToHandle.activeInHierarchy;
        objToHandle.SetActive(!handle);
    }
}
