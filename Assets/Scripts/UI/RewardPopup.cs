using GameEnum;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    [Header("UI �]�w")]
    public GameObject rewardEntryPrefab;
    public Transform entriesContainer;
    public Button confirmButton;
    public GameObject rewardPanel;
    public GameObject Description;
    public TextMeshProUGUI limit;
    public Button Inventory;
    [Header("�������")]
    public int maxSelectable = 1;
    private Queue<Rewards> pendingRewardQueue = new Queue<Rewards>();
    private Rewards currentContext;
    private List<RewardEntry> selectedRewards = new List<RewardEntry>();    
    private Dictionary<RewardEntry, RewardEntryView> entryViewDict = new Dictionary<RewardEntry, RewardEntryView>();

    void Start()
    {
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        rewardPanel.SetActive(false);
    }
    public void Update()
    {
        Inventory.gameObject.SetActive(pendingRewardQueue.Count > 0);
    }
    public void AddRewards(RewardContext context, int maxSelect)
    {
        Rewards rewards = new Rewards();
        rewards.RewardEntries = context.RewardEntries;
        rewards.MaxSelectable = maxSelect;
        pendingRewardQueue.Enqueue(rewards);
        CustomLogger.Log(this, $"�s�W���y�妸�A�@ {rewards.RewardEntries.Count} ���A�̦h��� {rewards.MaxSelectable} ��");
        if (!rewardPanel.activeSelf)
        {
            ShowNextRewardBatch();
        }
    }
    private void ShowNextRewardBatch()
    {
        if (pendingRewardQueue.Count == 0)
        {
            // �w�g�S�����󥼻�����y
            Description.SetActive(false);
            rewardPanel.SetActive(false);
            return;
        }
        // �o�̥u Peek�A�� Dequeue
        currentContext = pendingRewardQueue.Peek();
        maxSelectable = currentContext.MaxSelectable;
        selectedRewards.Clear();
        entryViewDict.Clear();
        RebuildUI(currentContext.RewardEntries, maxSelectable);
        //rewardPanel.SetActive(true);
    }
    private void RebuildUI(List<RewardEntry> rewards, int maxSelect)
    {
        // �M�� Container
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
        limit.text = $"choose {maxSelect} ";
        foreach (var reward in rewards)
        {
            GameObject entryObj = Instantiate(rewardEntryPrefab, entriesContainer);
            var view = entryObj.GetComponent<RewardEntryView>();
            if (view != null)
            {
                view.Setup(reward, OnRewardSelected);
                entryViewDict.Add(reward, view);
            }

            var option = entryObj.GetComponent<Option>();
            if (option != null)
            {
                option.Image.sprite = reward.Sprite;
                if (PlayerSettings.SelectedDropdownValue == 0)
                {
                    option.Description = reward.Name;
                }
                else
                {
                    option.Description = reward.EnglishName;
                }
                option.optionIndex = reward.Index;
            }
        }
        confirmButton.interactable = false;
    }

    private void OnRewardSelected(RewardEntry entry, bool isSelected)
    {
        if (isSelected)
        {
            if (selectedRewards.Count >= maxSelectable)
            {
                var earliest = selectedRewards[0];
                if (entryViewDict.TryGetValue(earliest, out var earliestView))
                {
                    earliestView.selectToggle.isOn = false;
                    CustomLogger.Log(this, $"�۰ʨ���: {earliest.GetName()}");
                }
            }
            if (!selectedRewards.Contains(entry))
                selectedRewards.Add(entry);
        }
        else
        {
            if (selectedRewards.Contains(entry))
                selectedRewards.Remove(entry);
        }

        confirmButton.interactable = (selectedRewards.Count == maxSelectable);
    }

    private void OnConfirmButtonClicked()
    {
        // �� currentContext ���Q�諸 RewardEntry �����
        foreach (var reward in selectedRewards)
        {
            CustomLogger.Log(this, $"���a��ܼ��y: {reward.GetName()}");
            reward.AwardAll();
        }
        // �⧹��~��o����y�q queue ����
        pendingRewardQueue.Dequeue();
        // ������ܤU�@��
        ShowNextRewardBatch();
    }
}
public class Rewards
{
    public List<RewardEntry> RewardEntries;
    public int MaxSelectable;
}