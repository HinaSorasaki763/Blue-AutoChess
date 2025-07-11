using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

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
    public List<int> DisableAugmentsIndex = new List<int>();
    public int forcedIndex;// 預設 -1 表示無內定
    private Queue<int> recentAugments = new Queue<int>();

    private Dictionary<int, int> augmentHistoryCount = new Dictionary<int, int>();
    private void OnEnable()
    {
        var list = new List<AugmentConfig>();
        list.AddRange(Resources.LoadAll<AugmentConfig>("Augments/SkillAugments"));
        list.AddRange(Resources.LoadAll<AugmentConfig>("Augments/AcademyAugments"));
        allAugments = list.ToArray();
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
        availableAugments = new List<AugmentConfig>(allAugments);
        availableAugments.RemoveAll(item => DisableAugmentsIndex.Contains(item.augmentIndex));

        if (availableAugments.Count < optionButtons.Length)
        {
            CustomLogger.LogWarning(this, "可用的強化選項不足！");
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = true;
            optionButtons[i].onClick.RemoveAllListeners();
        }

        List<int> selectedIndices = new List<int>();

        // 強制放入內定 index
        if (forcedIndex != -1)
        {
            int forcedAvailableIndex = availableAugments.FindIndex(a => a.augmentIndex == forcedIndex);
            if (forcedAvailableIndex != -1)
            {
                SetOptionAt(0, forcedAvailableIndex, selectedIndices);
            }
            else
            {
                CustomLogger.LogWarning(this, $"找不到內定強化 index: {forcedIndex}");
                SetOptionAt(0, GetWeightedRandomIndex(availableAugments, selectedIndices), selectedIndices);
            }
        }
        else
        {
            SetOptionAt(0, GetWeightedRandomIndex(availableAugments, selectedIndices), selectedIndices);
        }

        // 其餘選項
        for (int i = 1; i < optionButtons.Length; i++)
        {
            SetOptionAt(i, GetWeightedRandomIndex(availableAugments, selectedIndices), selectedIndices);
        }
    }
    private void SetOptionAt(int optionSlot, int augmentPoolIndex, List<int> selectedIndices)
    {
        selectedIndices.Add(augmentPoolIndex);
        var config = availableAugments[augmentPoolIndex];
        currentAugments[optionSlot] = AugmentFactory.CreateAugment(config);
        optionIcons[optionSlot].sprite = config.augmentIcon;
        int language = PlayerSettings.SelectedDropdownValue;
        string description = language == 0 ? config.description : config.descriptionEnglish;
        optionDescriptions[optionSlot].text = description;
        CustomLogger.Log(config, $"選擇了強化：{config.augmentName}，描述：{description},語言為{language}");

        recentAugments.Enqueue(config.augmentIndex);
        if (recentAugments.Count > 10)
        {
            int removed = recentAugments.Dequeue();
            augmentHistoryCount[removed]--;
        }

        if (!augmentHistoryCount.ContainsKey(config.augmentIndex))
            augmentHistoryCount[config.augmentIndex] = 0;
        augmentHistoryCount[config.augmentIndex]++;

        optionButtons[optionSlot].onClick.AddListener(() => SelectAugment(optionSlot));
    }

    private int GetWeightedRandomIndex(List<AugmentConfig> pool, List<int> alreadySelected)
    {
        List<float> weights = new List<float>();
        float totalWeight = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            if (alreadySelected.Contains(i))
            {
                weights.Add(0f);
                continue;
            }
            int augmentIndex = pool[i].augmentIndex;
            int seenCount = augmentHistoryCount.ContainsKey(augmentIndex) ? augmentHistoryCount[augmentIndex] : 0;
            float weight = 1f / (1 + seenCount);
            weights.Add(weight);
            totalWeight += weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        for (int i = 0; i < weights.Count; i++)
        {
            if (randomValue < weights[i])
                return i;
            randomValue -= weights[i];
        }
        return 0;
    }
    private void SelectAugment(int index)
    {
        if (currentAugments[index] == null) return;
        currentAugments[index].Apply();
        Debug.Log($"選擇了強化：{currentAugments[index].Name}");
        if (currentAugments[index].config.CharacterSkillEnhanceIndex != -1)
        {
            ResourcePool.Instance.ally.AddEnhancedSkill(currentAugments[index].config.CharacterSkillEnhanceIndex);

        }
        SelectedAugments.Instance.AddAugment(currentAugments[index]);
        foreach (var item in ResourcePool.Instance.ally.GetAllCharacter())
        {
            item.OnCharaterEnabled();
        }
        Parent.SetActive(false);
    }
}
