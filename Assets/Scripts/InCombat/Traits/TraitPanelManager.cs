using UnityEngine;
using System.Collections.Generic;

public class TraitPanelManager : MonoBehaviour
{
    public static TraitPanelManager Instance { get; private set; }
    private List<GameObject> traitPanels = new List<GameObject>();
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
    public void RegisterPanel(GameObject panel)
    {
        if (!traitPanels.Contains(panel))
        {
            traitPanels.Add(panel);
        }
    }

    // 打開指定的面板，並關閉其他所有面板
    public void OpenPanel(GameObject panelToOpen)
    {
        foreach (var panel in traitPanels)
        {
            if (panel != panelToOpen)
            {
                panel.SetActive(false);  // 關閉其他面板
            }
        }

        // 打開指定的面板
        panelToOpen.SetActive(true);
    }
    public void ClearAllPanels()
    {
        traitPanels.Clear();
    }
    public void CloseAllPanels()
    {
        foreach (var panel in traitPanels)
        {
            panel.SetActive(false);
        }
    }
}
