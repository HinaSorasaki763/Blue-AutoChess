using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    public GameObject ItemToToggle;
    public Button button;
    void Start()
    {
        button.onClick.AddListener(Toggle);
    }
    public void Toggle()
    {

        ItemToToggle.SetActive(!ItemToToggle.activeSelf);
    }
}
