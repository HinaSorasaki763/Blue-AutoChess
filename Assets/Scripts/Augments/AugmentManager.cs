using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
public class AugmentManager : MonoBehaviour
{
    private AugmentConfig[] allAugments;    // �Ҧ��j�ƿﶵ
    public Button[] optionButtons;          // UI ���s�]3 �ӡ^
    public Image[] optionIcons;             // �Ϥ�
    public TextMeshProUGUI[] optionDescriptions; // �y�z��r
    public GameObject selectedUIPrefab;     // �w��ܪ� UI ���ҪO
    public GameObject Parent;
    private List<AugmentConfig> availableAugments; // �i�Ϊ��j�ƿﶵ
    private Augment[] currentAugments;             // ��e��ܪ��j�ƿﶵ
    public static AugmentManager instance;
    public Button refreshButton; // �s�W����s���s
    public List<int> DisableAugmentsIndex = new List<int>();
    public int forcedIndex;// �w�] -1 ��ܵL���w
    private Queue<int> recentAugments = new Queue<int>();
    private System.Random rng = new System.Random();
    public int stage = 1;
    public Button stage1Btn;
    public Button stage2Btn;
    public Button stage3Btn;
    public GameObject stageSelectPanel;
    private Dictionary<int, int> augmentHistoryCount = new Dictionary<int, int>();
    private void OnEnable()
    {
        var list = new List<AugmentConfig>();
        list.AddRange(Resources.LoadAll<AugmentConfig>("Augments/SkillAugments"));
        list.AddRange(Resources.LoadAll<AugmentConfig>("Augments/AcademyAugments"));
        allAugments = list.ToArray();
    }
    public void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        availableAugments = new List<AugmentConfig>(allAugments); // ��l��
        currentAugments = new Augment[optionButtons.Length];
        SetupOptionButtons();
        stage1Btn.onClick.AddListener(() => OnStageSelected(1));
        stage2Btn.onClick.AddListener(() => OnStageSelected(2));
        stage3Btn.onClick.AddListener(() => OnStageSelected(3));
        // ����s���s�K�[�I���ƥ�
        refreshButton.onClick.AddListener(GenerateNewOptions);
    }
    private void OnStageSelected(int s)
    {
        stage = s;
        stageSelectPanel.SetActive(false);
        GenerateNewOptions();
        GetTeamInBackground();
    }
    private void GetTeamInBackground()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                CustomLogger.Log(this, $"Getting currentRound {GameStageManager.Instance.currentRound},stage {stage}");
                var opponents = await GameStageManager.Instance.uploader.GetRandomOpponentsAsync(GameStageManager.Instance.currentRound, stage, 3);
                GameStageManager.Instance.temp = opponents;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Background upload failed: {ex}");
            }
        });
    }
    private void SetupOptionButtons()
    {
        GenerateNewOptions();
    }

    // �H���ͦ� 3 �ӱj�ƿﶵ
    private void GenerateNewOptions()
    {
        // �z��ŦX���� Augments
        List<AugmentConfig> filtered = new List<AugmentConfig>();
        foreach (var aug in allAugments)
        {
            if (aug == null) continue;
            if (aug.CharacterSkillEnhanceIndex > 100) continue;

            bool include = false;
            switch (stage)
            {
                case 1:
                    include = (aug.cost == 1 || aug.cost == 2);
                    break;
                case 2:
                    include = (aug.cost == 3 || aug.cost == 4);
                    break;
                case 3:
                    include = (aug.cost == 5);
                    break;
            }

            if (include) filtered.Add(aug);
        }

        availableAugments = new List<AugmentConfig>(filtered);
        availableAugments.RemoveAll(item => DisableAugmentsIndex.Contains(item.augmentIndex));

        if (availableAugments.Count < optionButtons.Length)
        {
            CustomLogger.LogWarning(this, "�i�Ϊ��j�ƿﶵ�����I");
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = true;
            optionButtons[i].onClick.RemoveAllListeners();
        }

        List<int> selectedIndices = new List<int>();

        // �Ĥ@�ӿﶵ�G�Y���j�� index
        if (forcedIndex != -1)
        {
            int forcedAvailableIndex = availableAugments.FindIndex(a => a.augmentIndex == forcedIndex);
            if (forcedAvailableIndex != -1)
            {
                SetOptionAt(0, forcedAvailableIndex, selectedIndices);
            }
            else
            {
                CustomLogger.LogWarning(this, $"�䤣�줺�w�j�� index: {forcedIndex}");
                SetOptionAt(0, GetWeightedRandomIndex(availableAugments, selectedIndices), selectedIndices);
            }
        }
        else
        {
            SetOptionAt(0, GetWeightedRandomIndex(availableAugments, selectedIndices), selectedIndices);
        }

        // ��l�ﶵ
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
        CustomLogger.Log(config, $"��ܤF�j�ơG{config.augmentName}�A�y�z�G{description},�y����{language}");

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
        float randomValue = (float)(rng.NextDouble() * totalWeight);
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
        Debug.Log($"��ܤF�j�ơG{currentAugments[index].Name}");
        if (currentAugments[index].config.CharacterSkillEnhanceIndex != -1)
        {
            ResourcePool.Instance.ally.AddEnhancedSkill(currentAugments[index].config.CharacterSkillEnhanceIndex);

        }
        SelectedAugments.Instance.AddAugment(currentAugments[index]);
        foreach (var item in ResourcePool.Instance.ally.GetAllCharacter())
        {
            item.OnCharaterEnabled();
        }
        Shop.Instance.GoldLessRefresh();
        Parent.SetActive(false);
    }
}
