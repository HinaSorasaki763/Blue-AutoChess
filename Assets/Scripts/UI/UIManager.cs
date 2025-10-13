using GameEnum;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


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
    public GameObject EquipmentPreComposingModal;
    public Image Sprite1, Sprite2, CompleteItemSprite;
    public TextMeshProUGUI CompleteItemText;
    public Button BugReportButton;
    public TextMeshProUGUI Attack, Accuracy, Attackspeed, CritChance, CritDamage, Def, Dodge, Lifesteal, HealEffectiveness, PercentageDef,Range;
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
    public void ShowEquipmentPreComposing(IEquipment eq1, IEquipment eq2, IEquipment result)
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
        bool isEnhance = GameController.Instance.CheckCharacterEnhance(character.characterStats.CharacterId, character);
        string rawTooltip;
        if (isEnhance)
        {
            rawTooltip = character.characterStats.EnhancedSkillTooltips[language];
        }
        else
        {
            rawTooltip = character.characterStats.Tooltips[language];
        }

        string finalTooltip = StringPlaceholderReplacer.ReplacePlaceholders(rawTooltip, replacements);
        skillContext.text = finalTooltip;
    }

    private void Update()
    {
        // 實時更新彈窗數據
        if (characterStatsPopup.activeSelf && currentCharacter != null)
        {
            UpdateCharacterStats();
            UpdateModal();
            int level = currentCharacter.star;
            int language = PlayerSettings.SelectedDropdownValue;
            var replacements = StringPlaceholderReplacer.BuildPlaceholderDictionary(currentCharacter, level, language);
            bool isEnhance = GameController.Instance.CheckCharacterEnhance(currentCharacter.characterStats.CharacterId, currentCharacter);
            string rawTooltip;
            if (isEnhance)
            {
                rawTooltip = currentCharacter.characterStats.EnhancedSkillTooltips[language];
            }
            else
            {
                rawTooltip = currentCharacter.characterStats.Tooltips[language];
            }

            string finalTooltip = StringPlaceholderReplacer.ReplacePlaceholders(rawTooltip, replacements);
            skillContext.text = finalTooltip;
        }
    }
    public void ReSetBattleData()
    {
        GameStageManager.Instance.ResetBattleData();
    }
    public void ShowBugReport()
    {
        int randKey = ResourcePool.Instance.RandomKeyThisGame;
        int round = GameStageManager.Instance.GetRound();
        List<CharacterCTRL> allies = ResourcePool.Instance.ally.GetBattleFieldCharacter();
        List<CharacterCTRL> enemies = ResourcePool.Instance.enemy.GetBattleFieldCharacter();
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Round {round},randkey = {randKey}");
        stringBuilder.AppendLine("Allies");
        foreach (var ally in allies)
        {
            stringBuilder.AppendLine($"{ally.characterStats.CharacterName} at  hex {ally.CurrentHex.Index}");
            stringBuilder.AppendLine("Equipments");
            foreach (var item in ally.equipmentManager.equippedItems)
            {
                stringBuilder.AppendLine(item.EquipmentName);
            }
        }
        stringBuilder.AppendLine("Enemies");
        foreach (var enemy in enemies)
        {
            stringBuilder.AppendLine($"{enemy.characterStats.CharacterName} at  hex {enemy.CurrentHex.Index}");
            stringBuilder.AppendLine("Equipments");
            foreach (var item in enemy.equipmentManager.equippedItems)
            {
                stringBuilder.AppendLine(item.EquipmentName);
            }
        }
        CustomLogger.Log(this, stringBuilder.ToString());
        GUIUtility.systemCopyBuffer = stringBuilder.ToString();
    }
    public void ResumeTime()
    {
        Time.timeScale = 1;
    }
    public void StopTime()
    {
        Time.timeScale = 0;
    }
    private void UpdateCharacterStats()
    {
        // 根據 currentCharacter 更新彈窗中的數據
        characterNameText.text = currentCharacter.characterStats.CharacterName;
        healthText.text = $"Health:{currentCharacter.GetStat(StatsType.currHealth)}/{currentCharacter.GetStat(StatsType.Health)}";
        shieldText.text = $"Shield: {currentCharacter.GetShieldStats()}";
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
        shieldBar.UpdateValue(currentCharacter.GetShieldStats());
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
    private void UpdateModal()
    {
        Attack.text = ((int)currentCharacter.GetStat(StatsType.Attack)).ToString();
        Accuracy.text = ((int)currentCharacter.GetStat(StatsType.Accuracy)).ToString();
        Attackspeed.text = currentCharacter.GetStat(StatsType.AttackSpeed).ToString("F2");
        CritChance.text = ((int)currentCharacter.GetStat(StatsType.CritChance)).ToString();
        CritDamage.text = ((int)currentCharacter.GetStat(StatsType.CritRatio)).ToString();
        Def.text = ((int)currentCharacter.GetStat(StatsType.Resistence)).ToString();
        Dodge.text = ((int)currentCharacter.GetStat(StatsType.DodgeChance)).ToString();
        Lifesteal.text = ((int)currentCharacter.GetStat(StatsType.Lifesteal)).ToString();
        PercentageDef.text = ((int)currentCharacter.GetStat(StatsType.PercentageResistence)).ToString();
        Range.text = ((int)currentCharacter.GetStat(StatsType.Range)).ToString();
    }
}
public static class StringPlaceholderReplacer
{
    private static readonly Regex PlaceholderRegex = new Regex(@"\{([^}]+)\}");

    public static string ReplacePlaceholders(string source, Dictionary<string, string> replacements)
    {
        // 找到所有 {xxx} 的片段
        return PlaceholderRegex.Replace(source, match =>
        {
            // match.Groups[1].Value 會是 "xxx"（不含大括號）
            string key = match.Groups[1].Value;
            // 若replacements裡有這個key，就替換；否則原樣返回
            if (replacements.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                // 這裡也可以選擇返回空字串、或保留原本的 {xxx} 不動
                return match.Value;
            }
        });
    }
    public static Dictionary<string, string> BuildPlaceholderDictionary(CharacterCTRL character, int level, int language)
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
            {"Attack", isChinese? $"攻擊力 ({character.GetStat(StatsType.Attack):F1})" : $"Atk({character.GetStat(StatsType.Attack):F1}) "},
            {"Health", isChinese? $"生命 ({character.GetStat(StatsType.Health)})" : $"Health({character.GetStat(StatsType.Health)}) "},
            {"Final", character.ActiveSkill.GetAttackCoefficient(character.GetSkillContext()).ToString()},
            {"Logistic",character.ActiveSkill.GetLogisticCoefficient(character.GetSkillContext()).ToString() },
            {"KzusaAddedAttackSpeed", GameController.Instance.KazusaEnhancedSkill_KillCount.ToString()},
            { "BarrageInitAngle",character.characterStats.BarrageInitAngle.ToString()},
            { "BarrageIntervalAngle",character.characterStats.BarrageIntervalAngle.ToString()},
            {"SumireAddedHealth",GameController.Instance.SumireAddedHealth.ToString() },
            {"AzusaAddedAttack",GameController.Instance.AzusaAddAttack.ToString() },
            {"AkoAddedCrit",character.AkoAddedCrit.ToString() },
            {"Pressure",isChinese? $"壓力 ({PressureManager.Instance.GetPressure(character.IsAlly)})" : $"pressure({PressureManager.Instance.GetPressure(character.IsAlly)}) " }
        };
    }

}
