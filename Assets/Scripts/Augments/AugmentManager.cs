using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentManager : MonoBehaviour
{
    private AugmentConfig[] allAugments;    // 所有強化選項
    public Button[] optionButtons;          // UI 按鈕（3 個）
    public Image[] optionIcons;             // 圖片
    public TextMeshProUGUI[] optionDescriptions; // 描述文字
    public GameObject selectedUIPrefab;     // 已選擇的 UI 項模板
    public GameObject Parent;
    private List<AugmentConfig> availableAugments; // 可用的強化選項
    private Augment[] currentAugments;             // 當前顯示的強化選項

    public Button refreshButton; // 新增的刷新按鈕

    private void OnEnable()
    {
        allAugments = Resources.LoadAll<AugmentConfig>("Augments");
    }

    private void Start()
    {
        availableAugments = new List<AugmentConfig>(allAugments); // 初始化
        currentAugments = new Augment[optionButtons.Length];
        SetupOptionButtons();

        // 為刷新按鈕添加點擊事件
        refreshButton.onClick.AddListener(GenerateNewOptions);
    }

    private void SetupOptionButtons()
    {
        GenerateNewOptions();
    }

    // 隨機生成 3 個強化選項
    private void GenerateNewOptions()
    {
        // 每次生成新選項時，重置 availableAugments
        availableAugments = new List<AugmentConfig>(allAugments);

        if (availableAugments.Count < optionButtons.Length)
        {
            Debug.LogWarning("可用的強化選項不足！");
            return;
        }

        // 清理按鈕的監聽器和狀態
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = true;
            optionButtons[i].onClick.RemoveAllListeners();
        }

        // 隨機選取新的強化選項
        List<int> selectedIndices = new List<int>();
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, availableAugments.Count);
            } while (selectedIndices.Contains(randomIndex)); // 確保同一組內無重複

            selectedIndices.Add(randomIndex);
            AugmentConfig config = availableAugments[randomIndex];
            currentAugments[i] = AugmentFactory.CreateAugment(config); // 創建 Augment 實例
            optionIcons[i].sprite = config.augmentIcon;
            optionDescriptions[i].text = config.description;

            int index = i; // 防止閉包問題
            optionButtons[i].onClick.AddListener(() => SelectAugment(index));
        }
    }

    // 選擇強化
    private void SelectAugment(int index)
    {
        if (currentAugments[index] == null) return;
        currentAugments[index].Apply();
        Debug.Log($"選擇了強化：{currentAugments[index].Name}");
        Parent.SetActive(false);
    }
}
