using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DamageStatisticsManager : MonoBehaviour
{
    [SerializeField] private Transform allyParent;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private GameObject damageEntryPrefab;
    public static DamageStatisticsManager Instance { get; private set; }

    private Dictionary<CharacterCTRL, int> allyDamage = new Dictionary<CharacterCTRL, int>();
    private Dictionary<CharacterCTRL, int> enemyDamage = new Dictionary<CharacterCTRL, int>();
    private Dictionary<CharacterCTRL, int> allyDamageTaken = new Dictionary<CharacterCTRL, int>();
    private Dictionary<CharacterCTRL, int> enemyDamageTaken = new Dictionary<CharacterCTRL, int>();

    private int maxAllyDamage = 0;
    private int maxEnemyDamage = 0;
    private int maxAllyDamageTaken = 0;
    private int maxEnemyDamageTaken = 0;

    public void Awake()
    {
        Instance = this;
    }

    private void UpdateUIForTeam(Dictionary<CharacterCTRL, int> damageDict, Transform parent)
    {
        var sortedCharacters = new List<KeyValuePair<CharacterCTRL, int>>(damageDict);
        sortedCharacters.Sort((a, b) => b.Value.CompareTo(a.Value));

        for (int i = 0; i < sortedCharacters.Count; i++)
        {
            var character = sortedCharacters[i].Key;
            var damage = sortedCharacters[i].Value;

            var entry = parent.Find(character.name);
            if (entry != null)
            {
                entry.SetSiblingIndex(i);

                var sliders = entry.GetComponentsInChildren<Slider>();
                if (sliders.Length > 0)
                {
                    sliders[0].maxValue = damageDict.Values.Max();
                    sliders[0].value = damage;
                }
            }
        }
    }

    public void UpdateDamage(CharacterCTRL character, int amount)
    {
        var targetDict = character.IsAlly ? allyDamage : enemyDamage;

        if (!targetDict.ContainsKey(character))
        {
            targetDict[character] = 0;
            CreateUIEntry(character);
        }

        targetDict[character] += amount;

        if (character.IsAlly)
            maxAllyDamage = Mathf.Max(maxAllyDamage, targetDict[character]);
        else
            maxEnemyDamage = Mathf.Max(maxEnemyDamage, targetDict[character]);

        UpdateUI();
    }

    public void UpdateDamageTaken(CharacterCTRL character, int amount)
    {
        var targetDict = character.IsAlly ? allyDamageTaken : enemyDamageTaken;

        if (!targetDict.ContainsKey(character))
        {
            targetDict[character] = 0;
            CreateUIEntry(character);
        }

        targetDict[character] += amount;

        if (character.IsAlly)
            maxAllyDamageTaken = Mathf.Max(maxAllyDamageTaken, targetDict[character]);
        else
            maxEnemyDamageTaken = Mathf.Max(maxEnemyDamageTaken, targetDict[character]);

        UpdateUI();
    }

    private void CreateUIEntry(CharacterCTRL character)
    {
        var parent = character.IsAlly ? allyParent : enemyParent;
        var entry = Instantiate(damageEntryPrefab, parent);
        entry.name = character.name;
        entry.GetComponentInChildren<Image>().sprite = character.characterStats.Sprite;

        var sliders = entry.GetComponentsInChildren<Slider>();
        foreach (var slider in sliders)
        {
            slider.maxValue = 1;
        }
    }

    private void UpdateUI()
    {
        foreach (var kvp in allyDamage)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxAllyDamage, allyParent);
        }
        UpdateUIForTeam(allyDamage, allyParent);

        foreach (var kvp in enemyDamage)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxEnemyDamage, enemyParent);
        }
        UpdateUIForTeam(enemyDamage, enemyParent);

        foreach (var kvp in allyDamageTaken)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxAllyDamageTaken, allyParent);
        }
        UpdateUIForTeam(allyDamageTaken, allyParent);

        foreach (var kvp in enemyDamageTaken)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxEnemyDamageTaken, enemyParent);
        }
        UpdateUIForTeam(enemyDamageTaken, enemyParent);
    }

    private void UpdateSlider(CharacterCTRL character, float damage, float maxDamage, Transform parent)
    {
        var entry = parent.Find(character.name);
        if (entry != null)
        {
            var sliders = entry.GetComponentsInChildren<Slider>();
            if (sliders.Length > 0)
            {
                sliders[0].maxValue = maxDamage;
                sliders[0].value = damage;
            }
        }
    }

    public CharacterCTRL GetTopDamageDealer(bool isAlly)
    {
        var targetDict = isAlly ? allyDamage : enemyDamage;
        return targetDict
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key.characterStats.logistics)
            .FirstOrDefault().Key;
    }
    public CharacterCTRL GetTopDamageTaken(bool isAlly)
    {
        var targetDict = isAlly ? allyDamageTaken : enemyDamageTaken;
        return targetDict.OrderByDescending(x => x.Value).FirstOrDefault().Key;
    }
}
