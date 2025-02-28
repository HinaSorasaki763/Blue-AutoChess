using GameEnum;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
    public List<Image> equipments = new List<Image>();
    public List<Button> equipmentButttons = new List<Button>();
    private CharacterCTRL currentCharacter;
    public TextMeshProUGUI EquipmentName;
    public TextMeshProUGUI EquipmentDetail;
    public Image equipmentIcon;
    public GameObject EquipmntModal;
    public GameObject EquipmentPreComposingModal;
    public Image Sprite1, Sprite2, CompleteItemSprite;
    public TextMeshProUGUI CompleteItemText;
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
            int index = i; // �إߤ@�ӥ��a�ܼƨӫO�s��e�����ޭ�
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
    public void ShowEquipmentPreComposing(IEquipment eq1,IEquipment eq2,IEquipment result)
    {
        EquipmentPreComposingModal.SetActive(true);
        EquipmentPreComposingModal.transform.position = Input.mousePosition;
        Sprite1.sprite = eq1.Icon;
        Sprite2.sprite = eq2.Icon;
        CompleteItemSprite.sprite = result.Icon;
        CompleteItemText.text = result.EquipmentDetail;
    }
    public void DisableEquipmentPreComposing()
    {
        EquipmentPreComposingModal.SetActive(false);
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
        int level = character.star;
        int language = PlayerSettings.SelectedDropdownValue;
        var replacements = StringPlaceholderReplacer.BuildPlaceholderDictionary(character, level, language);
        string rawTooltip = character.characterStats.Tooltips[language];
        string finalTooltip = StringPlaceholderReplacer.ReplacePlaceholders(rawTooltip, replacements);
        skillContext.text = finalTooltip;
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
    public void OnQuit()
    {
        Application.Quit();
    }
    private void CloseCharacterStats()
    {
        EquipmntModal.SetActive(false);
        characterStatsPopup.SetActive(false);
        currentCharacter = null;
    }
}
public static class StringPlaceholderReplacer
{
    private static readonly Regex PlaceholderRegex = new Regex(@"\{([^}]+)\}");

    public static string ReplacePlaceholders(string source, Dictionary<string, string> replacements)
    {
        // ���Ҧ� {xxx} �����q
        return PlaceholderRegex.Replace(source, match =>
        {
            // match.Groups[1].Value �|�O "xxx"�]���t�j�A���^
            string key = match.Groups[1].Value;
            // �Yreplacements�̦��o��key�A�N�����F�_�h��˪�^
            if (replacements.TryGetValue(key, out string value))
            {
                CustomLogger.Log(value,$"getting {key} key , returing {value } value");
                return value;
            }
            else
            {
                // �o�̤]�i�H��ܪ�^�Ŧr��B�ΫO�d�쥻�� {xxx} ����
                return match.Value;
            }
        });
    }
    public static Dictionary<string, string> BuildPlaceholderDictionary(CharacterCTRL character, int level,int language)
    {
        StarLevelStats stats = character.ActiveSkill.GetCharacterLevel()[level];
        bool isChinese = (language == 0);
        return new Dictionary<string, string>()
    {
        {"data1", stats.Data1.ToString()},
        {"data2", stats.Data2.ToString()},
        {"data3", stats.Data3.ToString()},
        {"data4", stats.Data4.ToString()},
        {"data5", stats.Data5.ToString("F1")},
        {"Attack", isChinese? $"�����O ({character.GetStat(StatsType.Attack)})" : $"Atk({character.GetStat(StatsType.Attack)}) "},
        {"Health", isChinese? $"�����O ({character.GetStat(StatsType.Health)})" : $"Atk({character.GetStat(StatsType.Health)}) "},
        {"Final", character.ActiveSkill.GetAttackCoefficient(character.GetSkillContext()).ToString()}
    };
    }

}
