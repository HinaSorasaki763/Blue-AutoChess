using UnityEngine;
using System.Collections.Generic;
using GameEnum;

[CreateAssetMenu(fileName = "Equipment", menuName = "Equipment")]
public class EquipmentSO : ScriptableObject, IEquipment
{
    public string equipmentName;
    public Sprite icon;
    public EquipmentSO equipment1;
    public EquipmentSO equipment2;

    public List<EquipmentType> Attributes;
    public List<int> Value;
    public Dictionary<EquipmentType, int> combinedStats;
    [TextArea(3,10)]
    public string Description;
    public string EquipmentName => equipmentName;
    public Sprite Icon => icon;

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
}
