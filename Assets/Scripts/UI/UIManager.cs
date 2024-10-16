using GameEnum;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject characterStatsPopup; // �u������
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI skillContext;
    public SliderCTRL healthBar, shieldBar, manaBar;
    public Button closeButton;

    private CharacterCTRL currentCharacter;

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

    private void Start()
    {
        closeButton.onClick.AddListener(CloseCharacterStats);
        characterStatsPopup.SetActive(false);
    }

    public void ShowCharacterStats(CharacterCTRL character)
    {
        currentCharacter = character;
        characterStatsPopup.SetActive(true);
        UpdateCharacterStats();
        healthBar.SetMaxValue(character.GetStat(StatsType.Health));
        healthBar.SetMinValue(0);
        shieldBar.SetMaxValue(character.GetStat(StatsType.Health));
        shieldBar.SetMinValue(0);
        manaBar.SetMaxValue(character.GetStat(StatsType.MaxMana));
        manaBar.SetMinValue(0);
        skillContext.text = character.characterStats.skillTooltip;
    }

    private void Update()
    {
        // ��ɧ�s�u���ƾ�
        if (characterStatsPopup.activeSelf && currentCharacter != null)
        {
            UpdateCharacterStats();
        }
    }

    private void UpdateCharacterStats()
    {
        // �ھ� currentCharacter ��s�u�������ƾ�
        characterNameText.text = currentCharacter.characterStats.CharacterName;
        healthText.text = $"Health: {currentCharacter.GetStat(StatsType.currHealth)} / {currentCharacter.GetStat(StatsType.Health)}";
        shieldText.text = $"Shield: {currentCharacter.GetStat(StatsType.Shield)}";
        manaText.text = $"Mana: {currentCharacter.GetStat(StatsType.Mana)}";
        healthBar.UpdateValue(currentCharacter.GetStat(StatsType.currHealth));
        shieldBar.UpdateValue(currentCharacter.GetStat(StatsType.Shield));
        manaBar.UpdateValue(currentCharacter.GetStat(StatsType.Mana));
    }

    private void CloseCharacterStats()
    {
        // �����u��
        characterStatsPopup.SetActive(false);
        currentCharacter = null;
    }
}
