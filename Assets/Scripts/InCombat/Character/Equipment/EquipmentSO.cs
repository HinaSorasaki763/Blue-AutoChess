using UnityEngine;
using System.Collections.Generic;
using GameEnum;

[CreateAssetMenu(fileName = "Equipment", menuName = "Equipment")]
public class EquipmentSO : ScriptableObject, IEquipment
{
    public int Index;
    public int Id => Index;
    [TextArea(3, 10)]
    public string equipmentDescription;
    [TextArea(3, 10)]
    public string equipmentDescriptionEnglish;
    public string equipmentName;
    public Sprite icon;
    public bool isSpecial;
    public bool isConsumable;
    public List<Traits> Traits;
    public List<EquipmentType> Attributes;
    public List<int> Value;
    public Dictionary<EquipmentType, int> combinedStats;
    public ConsumableEffectType effectType;
    public CharacterObserverBase observer;
    public CharacterObserverBase Observer
    {
        get => observer;
        set => observer = value;
    }

    public IEquipment Clone()
    {
        return new EquipmentSO();
    }
    public string EquipmentDetail => equipmentDescription;
    public string EquipmentName => equipmentName;
    public string EquipmentDescriptionEnglish => equipmentDescriptionEnglish;
    public Sprite Icon => icon;
    public bool IsSpecial => isSpecial;
    public bool IsConsumable => isConsumable;

    public Dictionary<EquipmentType, int> GetStats()
    {
        return CalculateCombinedStats();
    }

    public Dictionary<EquipmentType, int> CalculateCombinedStats()
    {
        combinedStats = new Dictionary<EquipmentType, int>();
        for (int i = 0; i < Attributes.Count; i++)
        {
            combinedStats[Attributes[i]] = Value[i];
        }
        return combinedStats;   

    }
    public void OnRemove()
    {

    }
}
