using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedAugments : MonoBehaviour
{
    public List<Augment> selectedAugments = new List<Augment>(); // 儲存已選擇的強化選項
    public List<Button> buttons = new List<Button>();
    public static SelectedAugments Instance; // 單例模式
    public Sprite LockedSprite;
    public List<TextMeshProUGUI> descriptions = new List<TextMeshProUGUI>();
    public List<GameObject> panels = new List<GameObject>();
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => ShowAugment(index));
        }
    }
    public void AddAugment(Augment augment)
    {
        selectedAugments.Add(augment);
        for (int i = 0; i < 3; i++)
        {
            buttons[i].interactable = false;
            panels[i].SetActive(false);
            buttons[i].GetComponent<Image>().sprite = LockedSprite;
        }
        for (int i = 0; i < selectedAugments.Count; i++)
        {
            buttons[i].interactable = true;
            buttons[i].GetComponent<Image>().sprite = selectedAugments[i].Icon;
        }
    }
    public void ShowAugment(int index)
    {
        panels[index].SetActive(true);
        int language = PlayerSettings.SelectedDropdownValue;
        string description = language == 0 ? selectedAugments[index].Description : selectedAugments[index].DescriptionEnglish;
        descriptions[index].text = description;
    }
}
