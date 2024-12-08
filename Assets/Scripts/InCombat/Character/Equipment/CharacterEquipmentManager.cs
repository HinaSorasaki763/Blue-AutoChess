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
        ResourcePool.Instance.ally.UpdateTraitEffects();
        if (equippedItems.Count >= MaxEquipmentSlots)
        {
            return false;
        }
        if (equipment is SpecialEquipment specialEquipment)
        {
            Traits trait =  Parent.traitController.GetAcademy();
            specialEquipment.OriginalstudentTrait = trait;
            Parent.traitController.RemoveTrait(trait);
            Parent.traitController.AddTrait(specialEquipment.trait);
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
    public void RemoveAllItem()
    {
        // 創建副本來安全地遍歷
        var itemsToRemove = new List<IEquipment>(equippedItems);
        foreach (var item in itemsToRemove)
        {
            RemoveEquipment(item);
        }
        Parent.UpdateEquipmentUI();
    }

    public void RemoveEquipment(IEquipment equipment)
    {
        if (equippedItems.Contains(equipment))
        {
            equippedItems.Remove(equipment);
            RemoveStatsForEquipment(equipment);
            equipment.OnRemove(Parent);
            EquipmentManager.Instance.AddEquipmentItem( equipment);

        }
        ResourcePool.Instance.ally.UpdateTraitEffects();
    }
    public List<IEquipment> GetEquippedItems()
    {
        return new List<IEquipment>(equippedItems);
    }
    public bool CanCombine(IEquipment eq1, IEquipment eq2, out IEquipment result)
    {
        result = ResourcePool.Instance.combinationRoute.GetCombinationResult(eq1, eq2);
        return result != null;
    }

    private void AddEquipment(IEquipment equipment)
    {
        equippedItems.Add(equipment);
    }
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
