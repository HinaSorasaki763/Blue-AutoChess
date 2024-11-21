using GameEnum;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject characterStatsPopup; // 彈窗物件
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI skillContext;
    public SliderCTRL healthBar, shieldBar, manaBar;
    public Button closeButton;
    public List<Image> equipments = new List<Image>();
    public List<Button> equipmentButttons = new List<Button>();
    private CharacterCTRL currentCharacter;
    public TextMeshProUGUI EquipmentName;
    public TextMeshProUGUI EquipmentDetail;
    public Image equipmentIcon;
    public GameObject EquipmntModal;
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

        for (int i = 0; i < equipmentButttons.Count; i++)
        {
            int index = i; // 建立一個本地變數來保存當前的索引值
            equipmentButttons[index].onClick.AddListener(() => ShowEquipmentDetail(index));
        }
    }

    public void ShowEquipmentDetail(int i)
    {
        EquipmntModal.SetActive(true);
        IEquipment equipment = currentCharacter.equipmentManager.equippedItems[i];
        equipmentIcon.sprite = equipment.Icon;
        EquipmentName.text = equipment.EquipmentName;
        EquipmentDetail.text = equipment.EquipmentDetail;
    }

    public bool TryClose(CharacterCTRL character)
    {
        if (character == currentCharacter)
        {
            CloseCharacterStats();
            return true;
        }
        return false;
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
        // 實時更新彈窗數據
        if (characterStatsPopup.activeSelf && currentCharacter != null)
        {
            UpdateCharacterStats();
        }
    }

    private void UpdateCharacterStats()
    {
        // 根據 currentCharacter 更新彈窗中的數據
        characterNameText.text = currentCharacter.characterStats.CharacterName;
        healthText.text = $"Health: {currentCharacter.GetStat(StatsType.currHealth)} / {currentCharacter.GetStat(StatsType.Health)}";
        shieldText.text = $"Shield: {currentCharacter.GetStat(StatsType.Shield)}";
        manaText.text = $"Mana: {currentCharacter.GetStat(StatsType.Mana)}";
        for (int i = 0; i < currentCharacter.equipmentManager.equippedItems.Count; i++)
        {
            equipments[i].sprite = currentCharacter.equipmentManager.equippedItems[i].Icon;
        }
        for (int i = currentCharacter.equipmentManager.equippedItems.Count; i < 3; i++)
        {
            equipments[i].sprite = null;
        }
        healthBar.UpdateValue(currentCharacter.GetStat(StatsType.currHealth));
        shieldBar.UpdateValue(currentCharacter.GetStat(StatsType.Shield));
        manaBar.UpdateValue(currentCharacter.GetStat(StatsType.Mana));
    }

    private void CloseCharacterStats()
    {
        EquipmntModal.SetActive(false);
        characterStatsPopup.SetActive(false);
        currentCharacter = null;
    }
}
