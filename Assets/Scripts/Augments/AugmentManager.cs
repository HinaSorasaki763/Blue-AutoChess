using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

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

    public Button refreshButton; // �s�W����s���s
    public List<int> DisableAugmentsIndex = new List<int>();
    private Queue<int> recentAugments = new Queue<int>();
    private Dictionary<int, int> augmentHistoryCount = new Dictionary<int, int>();
    private void OnEnable()
    {
        allAugments = Resources.LoadAll<AugmentConfig>("Augments/SkillAugments");
    }

    private void Start()
    {
        availableAugments = new List<AugmentConfig>(allAugments); // ��l��
        currentAugments = new Augment[optionButtons.Length];
        SetupOptionButtons();

        // ����s���s�K�[�I���ƥ�
        refreshButton.onClick.AddListener(GenerateNewOptions);
    }

    private void SetupOptionButtons()
    {
        GenerateNewOptions();
    }

    // �H���ͦ� 3 �ӱj�ƿﶵ
    private void GenerateNewOptions()
    {
        availableAugments = new List<AugmentConfig>(allAugments);
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
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int selectedIndex = GetWeightedRandomIndex(availableAugments, selectedIndices);
            selectedIndices.Add(selectedIndex);

            var config = availableAugments[selectedIndex];
            currentAugments[i] = AugmentFactory.CreateAugment(config);
            optionIcons[i].sprite = config.augmentIcon;
            optionDescriptions[i].text = config.description;

            recentAugments.Enqueue(config.augmentIndex);
            if (recentAugments.Count > 10)
            {
                int removed = recentAugments.Dequeue();
                augmentHistoryCount[removed]--;
            }

            if (!augmentHistoryCount.ContainsKey(config.augmentIndex))
                augmentHistoryCount[config.augmentIndex] = 0;
            augmentHistoryCount[config.augmentIndex]++;

            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectAugment(index));
        }
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
        if (currentAugments[index].config.CharacterSkillEnhanceIndex!= -1)
        {
            ResourcePool.Instance.ally.AddEnhancedSkill(currentAugments[index].config.CharacterSkillEnhanceIndex);
            SelectedAugments.Instance.AddAugment(currentAugments[index]);

            currentAugments[index].Apply();
            Debug.Log($"��ܤF�j�ơG{currentAugments[index].Name}");
        }
        Parent.SetActive(false);
    }
}
