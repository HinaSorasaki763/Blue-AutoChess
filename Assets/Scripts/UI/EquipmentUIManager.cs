using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }
    private List<GameObject> equipmentUIs = new List<GameObject>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void RegisterUI(GameObject obj)
    {
        if (!equipmentUIs.Contains(obj))
        {
            equipmentUIs.Add(obj);
        }
    }
    public void ClearUI()
    {
        equipmentUIs.Clear();
    }
    public void ToggleUI(GameObject panelToToggle)
    {
        foreach (var panel in equipmentUIs)
        {
            if (panel != panelToToggle)
            {
                panel.SetActive(false);  // 關閉其他面板
            }
        }

        // 打開指定的面板
        panelToToggle.SetActive(!panelToToggle.activeInHierarchy);
    }
    public void OpenUI(GameObject obj)
    {
        foreach (var panel in equipmentUIs)
        {
            if (panel != obj)
            {
                panel.SetActive(false);  // 關閉其他面板
            }
        }

        // 打開指定的面板
        obj.SetActive(true);
    }
    public void CloseAllUI()
    {
        foreach (var item in equipmentUIs)
        {
            item.SetActive(false);
        }
    }
}
