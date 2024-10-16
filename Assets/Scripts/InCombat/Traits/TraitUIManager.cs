using GameEnum;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitUIManager : MonoBehaviour
{
    public static TraitUIManager Instance { get; private set; }
    public GameObject traitUIPrefab; // 預製件包含圖標和文字
    public Transform traitUIContainer; // 用來放置 UI 的父物件
    private Dictionary<Traits, GameObject> activeTraitUIs = new Dictionary<Traits, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 根據角色數量更新羈絆UI
    public void UpdateTraitUI(Dictionary<Traits, int> traitCounts)
    {
        // 清空現有的顯示
        foreach (Transform child in traitUIContainer)
        {
            Destroy(child.gameObject);
        }
        activeTraitUIs.Clear();
        TraitPanelManager.Instance.ClearAllPanels();
        List<KeyValuePair<Traits, int>> sortedTraits = new List<KeyValuePair<Traits, int>>(traitCounts);
        sortedTraits.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); // 按照角色數量排序

        // 生成新的羈絆 UI
        foreach (var trait in sortedTraits)
        {
            GameObject traitUI = Instantiate(traitUIPrefab, traitUIContainer);
            TraitUI uiComponent = traitUI.GetComponent<TraitUI>();

            if (uiComponent != null)
            {
                // 設置 trait 屬性，確保 UI 項目對應正確的羈絆
                uiComponent.trait = trait.Key;
            }

            activeTraitUIs[trait.Key] = traitUI;

            // 設置圖標和文字
            Image icon = traitUI.transform.Find("Icon").GetComponent<Image>(); // 假設圖標是命名為 "Icon" 的子物件
            TextMeshProUGUI text = traitUI.transform.Find("Text").GetComponent<TextMeshProUGUI>(); // 假設文字是命名為 "Text" 的子物件
            icon.sprite = GetTraitIcon(trait.Key); // 根據羈絆類型設置圖標
            text.text = $"{trait.Key} ({trait.Value})"; // 顯示羈絆名稱和角色數量
        }
    }


    // 根據 trait 獲取對應的圖標
    private Sprite GetTraitIcon(Traits trait)
    {
        /*
        switch (trait)
        {
            case Traits.Abydos:
                return Resources.Load<Sprite>("Icons/AbydosIcon"); // 假設你有一個資源文件夾裡的圖標
            case Traits.Gehenna:
                return Resources.Load<Sprite>("Icons/GehennaIcon");
            // 其他羈絆圖標...
            default:
                return null;
        }*/
        return null;
    }
}
