using System.Collections.Generic;
using System.Linq;
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
    public List<int> SelectedIndex = new List<int>(); // 儲存已選擇的強化選項索引
    public List<int> TriggeredIndex = new List<int>();
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
    public bool CheckAugmetExist(int index)
    {
        return selectedAugments.Exists(item => item.config.augmentIndex == index);
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
        SelectedIndex.Add(augment.config.augmentIndex);
    }
    public void ShowAugment(int index)
    {
        panels[index].SetActive(true);
        int language = PlayerSettings.SelectedDropdownValue;
        string description = language == 0 ? selectedAugments[index].Description : selectedAugments[index].DescriptionEnglish;
        descriptions[index].text = description;
    }
    public void TriggerAugment(int index)
    {
        if (TriggeredIndex.Contains(index)) return;
        foreach (Augment augment in selectedAugments)
        {
            if (augment.config.augmentIndex == index)
            {
                TriggeredIndex.Add(augment.config.augmentIndex);
                augment.Trigger();

            }
        }
    }
    /// <summary>
    /// 部分方法仍要先檢查有沒有該強化
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool CheckIfConditionMatch(int index)
    {
        
        if (!CheckAugmetExist(index))
        {
            return false;
        }

        return selectedAugments
            .FirstOrDefault(a => a.config.augmentIndex == index)
            ?.OnConditionMatch() ?? false;
    }
}
