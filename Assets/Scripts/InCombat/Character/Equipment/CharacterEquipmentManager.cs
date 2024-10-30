using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEnum;

public class CharacterEquipmentManager : MonoBehaviour
{
    private const int MaxEquipmentSlots = 3;
    private List<IEquipment> equippedItems = new List<IEquipment>();
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

        // 检查是否有可以与之合成的基础装备
        BasicEquipment combinableEquipment = null;
        foreach (var item in equippedItems)
        {
            if (item is BasicEquipment basicItem)
            {
                if (CanCombine(basicItem, equipment))
                {
                    combinableEquipment = basicItem;
                    break;
                }
            }
        }

        if (combinableEquipment != null)
        {
            if (equipment is BasicEquipment basic)
            {
                CombinedEquipment combinedEquipment = new CombinedEquipment(combinableEquipment, basic);

                // 移除原有的基础装备
                RemoveEquipment(combinableEquipment);

                // 添加合成装备
                AddEquipment(combinedEquipment);

                // 更新属性
                UpdateStatsForEquipment(combinedEquipment);

                return true;
            }
            return false;
        }
        else
        {
            // 直接装备基础装备
            AddEquipment(equipment);

            // 更新属性
            UpdateStatsForEquipment(equipment);

            return true;
        }
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
    public bool CanCombine(IEquipment eq1, IEquipment eq2)
    {
        // 检查 eq1 和 eq2 是否都是 BasicEquipment 类型
        if (eq1 is BasicEquipment basicEq1 && eq2 is BasicEquipment basicEq2)
        {
            // 现在可以访问 basicEq1 和 basicEq2 的 combinableWith 属性
            return basicEq1.combinableWith.Contains(basicEq2.equipmentType) &&
                   basicEq2.combinableWith.Contains(basicEq1.equipmentType);
        }
        else
        {
            // 如果其中一个不是 BasicEquipment，则无法合成
            return false;
        }
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
