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
    private int maxAllyDamage = 0;
    private int maxEnemyDamage = 0;
    public void Awake()
    {
        Instance = this;
    }
    private void UpdateUIForTeam(Dictionary<CharacterCTRL, int> damageDict, Transform parent)
    {
        // ���ˮ`�ȱƧ�
        var sortedCharacters = new List<KeyValuePair<CharacterCTRL, int>>(damageDict);
        sortedCharacters.Sort((a, b) => b.Value.CompareTo(a.Value));

        // ��sUI����
        for (int i = 0; i < sortedCharacters.Count; i++)
        {
            var character = sortedCharacters[i].Key;
            var damage = sortedCharacters[i].Value;

            // �T�OUI����s�b
            var entry = parent.Find(character.name);
            if (entry != null)
            {
                // �ʺA�վ㶶��
                entry.SetSiblingIndex(i);

                // ��sSlider
                var slider = entry.GetComponentInChildren<Slider>();
                slider.maxValue = damageDict.Values.Max();
                slider.value = damage;
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

    private void CreateUIEntry(CharacterCTRL character)
    {
        var parent = character.IsAlly ? allyParent : enemyParent;
        var entry = Instantiate(damageEntryPrefab, parent);
        entry.name = character.name;
        entry.GetComponentInChildren<Image>().sprite = character.characterStats.Sprite;
        entry.GetComponentInChildren<Slider>().maxValue = 1; // ��l�ȡA�y��|�ʺA��s
    }

    private void UpdateUI()
    {
        foreach (var kvp in allyDamage)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxAllyDamage, allyParent);
        }
        UpdateUIForTeam(allyDamage,allyParent);

        foreach (var kvp in enemyDamage)
        {
            UpdateSlider(kvp.Key, kvp.Value, maxEnemyDamage, enemyParent);
        }
        UpdateUIForTeam(enemyDamage, enemyParent);
    }

    private void UpdateSlider(CharacterCTRL character, float damage, float maxDamage, Transform parent)
    {
        var entry = parent.Find(character.name);
        if (entry != null)
        {
            var slider = entry.GetComponentInChildren<Slider>();
            slider.maxValue = maxDamage;
            slider.value = damage;
        }
    }
}
