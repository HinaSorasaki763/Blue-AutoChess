using GameEnum;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    [Header("UI �]�w")]
    public GameObject rewardEntryPrefab;    // �w�s����A���t RewardEntryView �P Option �ե�
    public Transform entriesContainer;      // ��m RewardEntryView ���e��
    public Button confirmButton;            // �T�{���s
    public GameObject rewardPanel;          // ��Ӽ��y���O

    [Header("��L�ѷ�")]
    public EquipmentManager equipmentManager; // �i���o�Ҧ��i�θ˳�

    [Header("�������")]
    public int maxSelectable = 1;  // ����̦h�i����� toggle ��

    private RewardContext rewardContext;                // �޲z�Ҧ� RewardEntry
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
        // �ΧA�쥻�� RewardPopup ���
        rewardPanel.SetActive(true);
        this.rewardContext = context;
        selectedRewardEntries.Clear();
        maxSelectable = maxSelect;

        PopulateRewardUI(); // �ͦ�UI
    }
    /// <summary>
    /// ���U��k�G���w�d��A�q equipmentManager �̩���h��˳ơA�æ^�ǹ����� IReward �M��
    /// </summary>
    /// 
    

    /// <summary>
    /// �ھ� rewardContext ���� RewardEntry �إ� UI ����
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
    /// �� RewardEntryView �� Toggle ���A���ܮɡA��s����C��
    /// �W�L maxSelectable �ɡA�|�۰ʨ����̦����������
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
                    CustomLogger.Log(this, $"�۰ʨ���: {earliest.GetName()}");
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

        // �o��M�w�u�X�Ӯɭԥi�H���U�T�{�v
        confirmButton.interactable = (selectedRewardEntries.Count == maxSelectable);
    }

    /// <summary>
    /// ���U�T�{��A�o��Ҧ��Q����� RewardEntry
    /// </summary>
    void OnConfirmButtonClicked()
    {
        foreach (var entry in selectedRewardEntries)
        {
            CustomLogger.Log(this, $"���a��ܼ��y: {entry.GetName()}");
            entry.AwardAll();
        }
        rewardPanel.SetActive(false);
    }

}
