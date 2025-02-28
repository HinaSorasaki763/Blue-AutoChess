using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WarningPanel : MonoBehaviour
{
    public List<Button> buttons = new List<Button>();
    public List<GameObject> panels = new List<GameObject>();
    public void Awake()
    {
        int i = 0;
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(ResetColor);
            button.onClick.AddListener(() =>  SetColor(button));

            int index = i;
            button.onClick.AddListener(()=> SetActivePanel(index));
            i++;
        }
    }
    public void ResetColor()
    {
        foreach (var button in buttons) 
        {
            button.targetGraphic.color = Color.white;
            
        }
        foreach (var item in panels)
        {
            item.SetActive(false);
        }
    }
    public void SetColor(Button button)
    {
        button.targetGraphic.color = Color.gray;
    }
    public void SetActivePanel(int i )
    {
        panels[i].SetActive(true);
    }
}
