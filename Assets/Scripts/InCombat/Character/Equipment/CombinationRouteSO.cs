using System.Collections.Generic;
using UnityEngine;
using GameEnum;

[CreateAssetMenu(fileName = "CombinationRoute", menuName = "CombinationRoute")]
public class CombinationRouteSO : ScriptableObject
{
    [System.Serializable]
    public class CombinationEntry
    {
        public EquipmentSO equipmentA;      // 基本道具A
        public EquipmentSO equipmentB;      // 基本道具B
        public EquipmentSO resultEquipment; // 合成後的結果道具
    }

    public List<CombinationEntry> combinationEntries;
    public EquipmentSO GetCombinationResult(IEquipment eq1, IEquipment eq2)
    {
        CustomLogger.Log(this,$"Start finding combination with {eq1},{eq2}");
        if (eq1 is EquipmentSO eqSO1 && eq2 is EquipmentSO eqSO2)
        {
            foreach (var entry in combinationEntries)
            {
                if ((entry.equipmentA == eqSO1 && entry.equipmentB == eqSO2) ||
                    (entry.equipmentA == eqSO2 && entry.equipmentB == eqSO1))
                {
                    CustomLogger.Log(this, $"result = {entry.resultEquipment}");
                    return entry.resultEquipment;
                }
            }
        }
        return null; // 如果無法合成則返回null
    }
}
