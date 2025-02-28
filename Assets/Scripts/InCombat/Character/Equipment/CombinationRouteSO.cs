using System.Collections.Generic;
using UnityEngine;
using GameEnum;

[CreateAssetMenu(fileName = "CombinationRoute", menuName = "CombinationRoute")]
public class CombinationRouteSO : ScriptableObject
{
    [System.Serializable]
    public class CombinationEntry
    {
        public EquipmentSO equipmentA;      // �򥻹D��A
        public EquipmentSO equipmentB;      // �򥻹D��B
        public EquipmentSO resultEquipment; // �X���᪺���G�D��
    }

    public List<CombinationEntry> combinationEntries;
    public EquipmentSO GetCombinationResult(IEquipment eq1, IEquipment eq2)
    {
        CustomLogger.Log(this,$"Start finding combination with {eq1},{eq2}");
        if (eq1 is BasicEquipment eqSO1 && eq2 is BasicEquipment eqSO2)
        {

            foreach (var entry in combinationEntries)
            {
                if ((entry.equipmentA.Id == eqSO1.Id && entry.equipmentB.Id == eqSO2.Id) ||
                    (entry.equipmentA.Id == eqSO2.id && entry.equipmentB.Id == eqSO1.Id))
                {
                    CustomLogger.Log(this, $"result = {entry.resultEquipment}");
                    return entry.resultEquipment;
                }
            }
        }
        return null; // �p�G�L�k�X���h��^null
    }
}
