using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentInspecting : MonoBehaviour
{
    public GameObject[] targetObjects;

    // ���U���s�ɰ���
    public void OnButtonDown()
    {
        foreach (var obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    // �P�}���s�ɰ���
    public void OnButtonUp()
    {
        foreach (var obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}
