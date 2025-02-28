using GameEnum;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    [Header("UI 設定")]
    public GameObject rewardEntryPrefab;    // 預製物件，內含 RewardEntryView 與 Option 組件
    public Transform entriesContainer;      // 放置 RewardEntryView 的容器
    public Button confirmButton;            // 確認按鈕
    public GameObject rewardPanel;          // 整個獎勵面板

    [Header("其他參照")]
    public EquipmentManager equipmentManager; // 可取得所有可用裝備

    [Header("選取限制")]
    public int maxSelectable = 1;  // 限制最多可選取的 toggle 數

    private RewardContext rewardContext;                // 管理所有 RewardEntry
    private List<RewardEntry> selectedRewardEntries = new List<RewardEntry>();
    private Dictionary<RewardEntry, RewardEntryView> entryViewDict = new Dictionary<RewardEntry, RewardEntryView>();

    void Start()
    {
        
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        rewardPanel.SetActive(false);
    }
    public void ShowRewards(RewardContext context, int maxSelect)
    {
        // 用你原本的 RewardPopup 欄位
        rewardPanel.SetActive(true);
        this.rewardContext = context;
        selectedRewardEntries.Clear();
        maxSelectable = maxSelect;

        PopulateRewardUI(); // 生成UI
    }
    /// <summary>
    /// 輔助方法：給定範圍，從 equipmentManager 裡抽取多件裝備，並回傳對應的 IReward 清單
    /// </summary>
    /// 
    

    /// <summary>
    /// 根據 rewardContext 內的 RewardEntry 建立 UI 項目
    /// </summary>
    private void PopulateRewardUI()
    {
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
        entryViewDict.Clear();

        foreach (var entry in rewardContext.RewardEntries)
        {
            GameObject entryObj = Instantiate(rewardEntryPrefab, entriesContainer);
            Option option = entryObj.GetComponent<Option>();
            if (option != null)
            {
                option.Image.sprite = entry.Sprite;
                option.Description = entry.Description;
                option.optionIndex = entry.Index;
            }

            RewardEntryView view = entryObj.GetComponent<RewardEntryView>();
            if (view != null)
            {
                view.Setup(entry, OnRewardEntrySelected);
                entryViewDict.Add(entry, view);
            }
        }
    }

    /// <summary>
    /// 當 RewardEntryView 的 Toggle 狀態改變時，更新選取列表
    /// 超過 maxSelectable 時，會自動取消最早的選取項目
    /// </summary>
    void OnRewardEntrySelected(RewardEntry entry, bool isSelected)
    {
        if (isSelected)
        {
            if (selectedRewardEntries.Count >= maxSelectable)
            {
                RewardEntry earliest = selectedRewardEntries[0];
                if (entryViewDict.TryGetValue(earliest, out RewardEntryView earliestView))
                {
                    earliestView.selectToggle.isOn = false;
                    CustomLogger.Log(this, $"自動取消: {earliest.GetName()}");
                }
            }
            if (!selectedRewardEntries.Contains(entry))
                selectedRewardEntries.Add(entry);
        }
        else
        {
            if (selectedRewardEntries.Contains(entry))
                selectedRewardEntries.Remove(entry);
        }

        // 這行決定「幾個時候可以按下確認」
        confirmButton.interactable = (selectedRewardEntries.Count == maxSelectable);
    }

    /// <summary>
    /// 按下確認後，發放所有被選取的 RewardEntry
    /// </summary>
    void OnConfirmButtonClicked()
    {
        foreach (var entry in selectedRewardEntries)
        {
            CustomLogger.Log(this, $"玩家選擇獎勵: {entry.GetName()}");
            entry.AwardAll();
        }
        rewardPanel.SetActive(false);
    }

}
