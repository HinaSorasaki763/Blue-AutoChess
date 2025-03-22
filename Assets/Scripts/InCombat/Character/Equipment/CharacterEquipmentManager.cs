using GameEnum;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
        foreach (var item in equippedItems)
        {
            if (CanCombine(item, equipment, out IEquipment result))
            {
                UIManager.Instance.DisableEquipmentPreComposing();
                RemoveEquipment(item, true);
                AddEquipment(result);
                UpdateStatsForEquipment(result);
                return true;
            }
        }
        if (equippedItems.Count >= MaxEquipmentSlots)
        {
            return false;
        }
        if (equipment is SpecialEquipment specialEquipment)
        {
            Traits trait = Parent.traitController.GetAcademy();
            specialEquipment.OriginalstudentTrait = trait;
            Parent.traitController.RemoveTrait(trait);
            Parent.traitController.AddTrait(specialEquipment.trait);
        }
        AddEquipment(equipment);
        UpdateStatsForEquipment(equipment);
        return true;
    }
    public void OnParentCastedSkill()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnCastedSkill(Parent);
        }

    }
    public void OnParentGethit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit,string detailedSource)
    {
        foreach (var item in equippedItems)
        {
            item.Observer.GetHit(character, source, amount, isCrit,detailedSource);
        }
    }
    public void OnParentKilledEnemy(string detailedSource,CharacterCTRL characterDies)
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnKilledEnemy(Parent, detailedSource, characterDies);
        }
    }
    public bool HaveSpecificEquipment(int index)
    {
        foreach (var item in equippedItems)
        {
            if (item.Id == index)
            {
                return true;
            }
        }
        return false;
    }
    public void OnParentDealtDamage(CharacterCTRL source,CharacterCTRL target,int damage,string detailedSource,bool iscrit)
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnDamageDealt(source,target,damage, detailedSource,iscrit);
        }
    }
    public void OnParentDodged()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnDodged(Parent);
        }
    }
    public void OnParentAttack()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnAttacking(Parent);

        }
    }
    public (float, float) BeforeApplyingNegetiveEffect(float length, float effectiveness)
    {
        float finallength = length;
        float finaleffectiveness = effectiveness;
        foreach (var item in equippedItems)
        {
            (finallength, finaleffectiveness) = item.Observer.AdjustNegetiveEffect(finallength, finaleffectiveness);
        }
        return (finallength, finaleffectiveness);
    }
    public void CharacterUpdate()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.CharacterUpdate();
        }
    }
    public int ObserverDamageModifier(CharacterCTRL sourceCharacter, CharacterCTRL target, int amount, string detailedSource, bool isCrit)
    {
        foreach (var item in equippedItems)
        {
            amount = item.Observer.DamageModifier(sourceCharacter, target, amount, detailedSource, isCrit);
        }
        return amount;
    }
    public int BeforeDealtDamage(CharacterCTRL sourceCharacter, CharacterCTRL target, int amount, string detailedSource, bool isCrit)
    {
        int reduced = 0;
        foreach (var item in equippedItems)
        {
            reduced += item.Observer.BeforeDealtDmg(sourceCharacter, target, amount, detailedSource, isCrit);
        }
        return reduced;
    }
    public int BeforeHealing(CharacterCTRL character,int amount)
    {
        int final = amount;
        foreach (var item in equippedItems)
        {
            final = item.Observer.BeforeHealing(character, final);
        }
        return final;
    }
    public void TriggerCharacterStart()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.CharacterStart(Parent);
        }
    }
    public void TriggerManualUpdate()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.ManualUpdate(Parent);

        }
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
    public void RemoveEquipment(IEquipment equipment, bool destroy = false)
    {
        if (equippedItems.Contains(equipment))
        {
            equippedItems.Remove(equipment);
            RemoveStatsForEquipment(equipment);
            equipment.OnRemove(Parent);
            if (!destroy)
            {
                EquipmentManager.Instance.AddEquipmentItem(equipment);
            }
        }
        ResourcePool.Instance.ally.UpdateTraitEffects();
    }
    public List<IEquipment> GetEquippedItems()
    {
        return new List<IEquipment>(equippedItems);
    }
    public static bool CanCombine(IEquipment eq1, IEquipment eq2, out IEquipment result)
    {
        result = ResourcePool.Instance.combinationRoute.GetCombinationResult(eq1, eq2);
        if (result == null)
        {
            return false;
        }
        CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(result.Id);
        result.Observer = c;
        return true;
    }

    private void AddEquipment(IEquipment equipment)
    {
        equippedItems.Add(equipment);
    }
    private void UpdateStatsForEquipment(IEquipment equipment)
    {
        foreach (var stat in equipment.GetStats())
        {
            float val = stat.Value;
            if (stat.Key == EquipmentType.AttackSpeed)
            {
                val *= 0.01f;
            }
            Parent.AddStat(GetStatType(stat.Key), val);
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
