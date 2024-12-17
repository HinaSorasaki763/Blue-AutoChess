using UnityEngine;
using System.Collections.Generic;
using GameEnum;

[CreateAssetMenu(fileName = "Equipment", menuName = "Equipment")]
public class EquipmentSO : ScriptableObject, IEquipment
{
    [TextArea(3, 10)]
    public string equipmentDescription;
    public string equipmentName;
    public Sprite icon;
    public bool isSpecial;
    public bool isConsumable;
    public Traits Traits;
    public List<EquipmentType> Attributes;
    public List<int> Value;
    public Dictionary<EquipmentType, int> combinedStats;
    public ConsumableEffectType effectType;

    public string EquipmentDetail => equipmentDescription;
    public string EquipmentName => equipmentName;
    public Sprite Icon => icon;
    public bool IsSpecial => isSpecial;
    public bool IsConsumable => isConsumable;

    public Dictionary<EquipmentType, int> GetStats()
    {
        return CalculateCombinedStats();
    }

    // 您可以在编辑器中手动设置 combinedStats，或者在代码中计算
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
