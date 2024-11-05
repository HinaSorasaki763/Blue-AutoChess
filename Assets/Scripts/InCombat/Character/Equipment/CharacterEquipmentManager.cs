using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;

public class CharacterEquipmentManager : MonoBehaviour
{
    private const int MaxEquipmentSlots = 3;
    public List<IEquipment> equippedItems = new List<IEquipment>();
    private CharacterCTRL Parent;
    public void SetParent(CharacterCTRL parent)
    {
        Parent = parent;
    }
    public bool EquipItem(IEquipment equipment)
    {
        if (equippedItems.Count >= MaxEquipmentSlots)
        {
            return false;
        }
        foreach (var item in equippedItems)
        {
            if (CanCombine(item, equipment, out IEquipment result))
            {
                RemoveEquipment(item);
                AddEquipment(result);
                UpdateStatsForEquipment(result);
                return true;
            }
        }
        AddEquipment(equipment);
        UpdateStatsForEquipment(equipment);
        return true;
    }

    // 移除装备
    public void RemoveEquipment(IEquipment equipment)
    {
        if (equippedItems.Contains(equipment))
        {
            equippedItems.Remove(equipment);
            RemoveStatsForEquipment(equipment);
            // 可以添加移除装备的显示逻辑
        }
    }

    // 获取已装备的装备列表
    public List<IEquipment> GetEquippedItems()
    {
        return new List<IEquipment>(equippedItems);
    }

    // 检查是否可以合成
    public bool CanCombine(IEquipment eq1, IEquipment eq2, out IEquipment result)
    {
        result = ResourcePool.Instance.combinationRoute.GetCombinationResult(eq1, eq2);
        return result != null;
    }


    // 添加装备到列表
    private void AddEquipment(IEquipment equipment)
    {
        equippedItems.Add(equipment);
        // 可以在这里调用显示装备的逻辑
    }

    // 更新属性
    private void UpdateStatsForEquipment(IEquipment equipment)
    {
        foreach (var stat in equipment.GetStats())
        {
            Parent.AddStat(GetStatType(stat.Key), stat.Value);
        }
    }

    // 移除属性
    private void RemoveStatsForEquipment(IEquipment equipment)
    {
        foreach (var stat in equipment.GetStats())
        {
            Parent.AddStat(GetStatType(stat.Key), -stat.Value);
        }
    }

    // 将EquipmentType映射到StatsType
    private StatsType GetStatType(EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Mana:
                return StatsType.Mana;
            case EquipmentType.Attack:
                return StatsType.Attack;
            case EquipmentType.AttackSpeed:
                return StatsType.AttackSpeed;
            case EquipmentType.Defense:
                return StatsType.Resistence;
            case EquipmentType.CriticalRate:
                return StatsType.CritChance;
            case EquipmentType.Health:
                return StatsType.Health;
            default:
                return StatsType.Null;
        }
    }
}
