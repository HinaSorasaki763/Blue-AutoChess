using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class References : MonoBehaviour
{
    public static References Instance;
    public GameObject DescriptionPanel;
    public GameObject DescriptionPanelParent;
    public TMPro.TextMeshProUGUI DescriptionText;
    public static int DescriptionIndex;
    public void Awake()
    {
        Instance = this;
    }
}
