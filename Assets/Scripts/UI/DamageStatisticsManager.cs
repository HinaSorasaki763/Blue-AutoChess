using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DamageStatisticsManager : MonoBehaviour
{
    public static DamageStatisticsManager Instance { get; private set; }

    public Transform DealtAllyParent;
    public Transform DealtEnemyParent;
    public Transform TakenAllyParent;
    public Transform TakenEnemyParent;
    private readonly Dictionary<CharacterCTRL, Transform> dealtEntries = new Dictionary<CharacterCTRL, Transform>();
    private readonly Dictionary<CharacterCTRL, Transform> takenEntries = new Dictionary<CharacterCTRL, Transform>();

    [SerializeField] private GameObject damageEntryPrefab;

    private readonly List<CharacterCTRL> allCharacters = new List<CharacterCTRL>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterCharacter(CharacterCTRL character)
    {
        allCharacters.Add(character);

        var dealtParent = character.IsAlly ? DealtAllyParent : DealtEnemyParent;
        var dealtEntry = Instantiate(damageEntryPrefab, dealtParent);
        dealtEntry.name = character.name;  // 名字可隨意，不影響後續查找
        dealtEntries[character] = dealtEntry.transform;

        var dealtUI = dealtEntry.GetComponent<StatisticUIPrefab>();
        dealtUI.icon.sprite = character.characterStats.Sprite;
        dealtUI.slider.maxValue = 1;

        var takenParent = character.IsAlly ? TakenAllyParent : TakenEnemyParent;
        var takenEntry = Instantiate(damageEntryPrefab, takenParent);
        takenEntry.name = character.name;
        takenEntries[character] = takenEntry.transform;

        var takenUI = takenEntry.GetComponent<StatisticUIPrefab>();
        takenUI.icon.sprite = character.characterStats.Sprite;
        takenUI.slider.maxValue = 1;
    }


    public void UpdateDamage(CharacterCTRL character, int amount)
    {
        CustomLogger.Log(this, $"UpdateDamage, {character}, {amount}");
        character.DealtDamageThisRound += amount;
        RefreshDealtUI();
    }

    public void UpdateDamageTaken(CharacterCTRL character, int amount)
    {
        CustomLogger.Log(this, $"UpdateDamageTaken, {character}, {amount}");
        character.TakeDamageThisRound += amount;
        RefreshTakenUI();
    }

    public void RefreshDealtUI()
    {
        // 排序
        var allyChars = allCharacters.Where(c => c.IsAlly).OrderByDescending(c => c.DealtDamageThisRound).ToList();
        var enemyChars = allCharacters.Where(c => !c.IsAlly).OrderByDescending(c => c.DealtDamageThisRound).ToList();
        float allyMax = allyChars.Any() ? allyChars.Max(c => c.DealtDamageThisRound) : 1;
        float enemyMax = enemyChars.Any() ? enemyChars.Max(c => c.DealtDamageThisRound) : 1;

        // Ally
        for (int i = 0; i < allyChars.Count; i++)
        {
            var c = allyChars[i];
            if (!dealtEntries.ContainsKey(c)) continue;

            var entry = dealtEntries[c];
            entry.SetSiblingIndex(i);

            var ui = entry.GetComponent<StatisticUIPrefab>();
            ui.slider.maxValue = allyMax;
            ui.slider.value = c.DealtDamageThisRound;
            ui.amount.text = c.DealtDamageThisRound.ToString();
        }
        // Enemy
        for (int i = 0; i < enemyChars.Count; i++)
        {
            var c = enemyChars[i];
            if (!dealtEntries.ContainsKey(c)) continue;

            var entry = dealtEntries[c];
            entry.SetSiblingIndex(i);

            var ui = entry.GetComponent<StatisticUIPrefab>();
            ui.slider.maxValue = enemyMax;
            ui.slider.value = c.DealtDamageThisRound;
            ui.amount.text = c.DealtDamageThisRound.ToString();
        }
    }

    public void RefreshTakenUI()
    {
        var allyChars = allCharacters
            .Where(c => c.IsAlly)
            .OrderByDescending(c => c.TakeDamageThisRound)
            .ToList();
        var enemyChars = allCharacters
            .Where(c => !c.IsAlly)
            .OrderByDescending(c => c.TakeDamageThisRound)
            .ToList();

        float allyMax = allyChars.Any() ? allyChars.Max(c => c.TakeDamageThisRound) : 1;
        float enemyMax = enemyChars.Any() ? enemyChars.Max(c => c.TakeDamageThisRound) : 1;

        // Ally
        for (int i = 0; i < allyChars.Count; i++)
        {
            var c = allyChars[i];
            if (!takenEntries.ContainsKey(c)) continue;

            var entry = takenEntries[c];
            entry.SetSiblingIndex(i);

            var ui = entry.GetComponent<StatisticUIPrefab>();
            ui.slider.maxValue = allyMax;
            ui.slider.value = c.TakeDamageThisRound;
            ui.amount.text = c.TakeDamageThisRound.ToString();
        }

        // Enemy
        for (int i = 0; i < enemyChars.Count; i++)
        {
            var c = enemyChars[i];
            if (!takenEntries.ContainsKey(c)) continue;

            var entry = takenEntries[c];
            entry.SetSiblingIndex(i);

            var ui = entry.GetComponent<StatisticUIPrefab>();
            ui.slider.maxValue = enemyMax;
            ui.slider.value = c.TakeDamageThisRound;
            ui.amount.text = c.TakeDamageThisRound.ToString();
        }
    }

    public CharacterCTRL GetTopDamageDealer(bool isAlly)
    {
        return allCharacters
            .Where(c => c.IsAlly == isAlly)
            .OrderByDescending(c => c.DealtDamageThisRound)
            .FirstOrDefault();
    }

    public CharacterCTRL GetTopDamageTaken(bool isAlly)
    {
        return allCharacters
            .Where(c => c.IsAlly == isAlly)
            .OrderByDescending(c => c.TakeDamageThisRound)
            .FirstOrDefault();
    }

    public void ResetDamage()
    {
        foreach (var c in allCharacters)
        {
            c.DealtDamageThisRound = 0;
            c.TakeDamageThisRound = 0;
        }
        RefreshDealtUI();
        RefreshTakenUI();
    }

    public void ClearAll()
    {
        allCharacters.Clear();
        foreach (Transform child in DealtAllyParent) Destroy(child.gameObject);
        foreach (Transform child in DealtEnemyParent) Destroy(child.gameObject);
        foreach (Transform child in TakenAllyParent) Destroy(child.gameObject);
        foreach (Transform child in TakenEnemyParent) Destroy(child.gameObject);
    }
}
