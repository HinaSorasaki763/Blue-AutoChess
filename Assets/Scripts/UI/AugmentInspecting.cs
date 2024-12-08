using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentInspecting : MonoBehaviour
{
    public GameObject[] targetObjects;

    // 按下按鈕時執行
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

    // 鬆開按鈕時執行
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
