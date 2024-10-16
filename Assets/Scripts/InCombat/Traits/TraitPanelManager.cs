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

    // ���}���w�����O�A��������L�Ҧ����O
    public void OpenPanel(GameObject panelToOpen)
    {
        foreach (var panel in traitPanels)
        {
            if (panel != panelToOpen)
            {
                panel.SetActive(false);  // ������L���O
            }
        }

        // ���}���w�����O
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
