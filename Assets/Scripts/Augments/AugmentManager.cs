using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void OnEnable()
    {
        allAugments = Resources.LoadAll<AugmentConfig>("Augments");
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
        // �C���ͦ��s�ﶵ�ɡA���m availableAugments
        availableAugments = new List<AugmentConfig>(allAugments);

        if (availableAugments.Count < optionButtons.Length)
        {
            Debug.LogWarning("�i�Ϊ��j�ƿﶵ�����I");
            return;
        }

        // �M�z���s����ť���M���A
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = true;
            optionButtons[i].onClick.RemoveAllListeners();
        }

        // �H������s���j�ƿﶵ
        List<int> selectedIndices = new List<int>();
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, availableAugments.Count);
            } while (selectedIndices.Contains(randomIndex)); // �T�O�P�@�դ��L����

            selectedIndices.Add(randomIndex);
            AugmentConfig config = availableAugments[randomIndex];
            currentAugments[i] = AugmentFactory.CreateAugment(config); // �Ы� Augment ���
            optionIcons[i].sprite = config.augmentIcon;
            optionDescriptions[i].text = config.description;

            int index = i; // ����]���D
            optionButtons[i].onClick.AddListener(() => SelectAugment(index));
        }
    }

    // ��ܱj��
    private void SelectAugment(int index)
    {
        if (currentAugments[index] == null) return;
        currentAugments[index].Apply();
        Debug.Log($"��ܤF�j�ơG{currentAugments[index].Name}");
        Parent.SetActive(false);
    }
}
