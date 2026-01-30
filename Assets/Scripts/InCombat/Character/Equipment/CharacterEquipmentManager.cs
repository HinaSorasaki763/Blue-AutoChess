using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            List<Traits> t = new List<Traits>(specialEquipment.Traits);
            if (specialEquipment.Traits.Contains(trait))
            {
                t.Remove(trait);
                specialEquipment.OriginalstudentTrait = trait;
                Parent.traitController.RemoveTrait(trait);
                Parent.traitController.AddTrait(t[0]);
            }
            else
            {
                PopupManager.Instance.CreatePopup($"角色{Parent.name}沒有{t[0]}或{t[1]},無法配戴", 2);
                return false;
            }
        }
        AddEquipment(equipment);
        UpdateStatsForEquipment(equipment);
        Parent.RecalculateStats();
        return true;
    }
    public void OnParentCastedSkill()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnCastedSkill(Parent);
        }

    }
    public List<int> GetEquipmentID()
    {
        List<int> ints = new List<int>();
        foreach (var item in equippedItems)
        {
            ints.Add(item.Id);
        }
        return ints;
    }
    public bool BeforeDying()
    {
        bool undying = false;
        foreach (var item in equippedItems)
        {
            if (item.Observer.BeforeDying(Parent))
            {
                CustomLogger.Log(this, $"{item.GetType()} triggering beforedying");
                undying = true;
            }
        }
        return undying;
    }
    public void OnBattleStart()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnBattleStart();
        }
    }
    public void OnParentGethit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource)
    {
        foreach (var item in equippedItems)
        {
            item.Observer.GetHit(character, source, amount, isCrit, detailedSource, true);
        }
    }
    public void OnParentKilledEnemy(string detailedSource, CharacterCTRL characterDies)
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
    public void OnParentDealtDamage(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnDamageDealt(source, target, damage, detailedSource, iscrit);
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
    public void OnCrit()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnCrit(Parent);
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
    public void TriggerOnEnterBattleField()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnEnterBattleField(Parent);
        }
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
    public void OnHealing()
    {
        foreach (var item in equippedItems)
        {
            item.Observer.OnHealing(Parent);
        }
    }
    public int BeforeHealing(CharacterCTRL character, int amount)
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
    public void ReforgeItem()
    {
        var itemsToRemove = new List<IEquipment>(equippedItems);
        foreach (var item in itemsToRemove)
        {
            ReforgeEquipment(item);
        }
        Parent.UpdateEquipmentUI();
    }
    public void ReforgeEquipment(IEquipment equipment)
    {
        if (equippedItems.Contains(equipment))
        {
            equippedItems.Remove(equipment);
            RemoveStatsForEquipment(equipment);
            equipment.OnRemove(Parent);
        }
        ResourcePool.Instance.ally.UpdateTraitEffects();
        if (equipment.Id <= 6)
        {
            Utility.GetMultipleRandomEquipRewards(1, 0, 6, equipment.Id).Item1[0].Award();
        }
        else if (equipment.Id <= 26)
        {
            Utility.GetMultipleRandomEquipRewards(1, 6, 26, equipment.Id).Item1[0].Award();
        }
        else if (equipment is SpecialEquipment sp)
        {
            List<Traits> tr = sp.Traits;

            List<Traits> remainAcademyTraits = Utility.AllAcademyTraits
                .Where(t => !tr.Contains(t))
                .ToList();

            if (remainAcademyTraits.Count == 0)
                return;

            int pickCount = Mathf.Min(2, remainAcademyTraits.Count);

            List<Traits> picked = new List<Traits>();
            for (int i = 0; i < pickCount; i++)
            {
                int index = UnityEngine.Random.Range(0, remainAcademyTraits.Count);
                picked.Add(remainAcademyTraits[index]);
                remainAcademyTraits.RemoveAt(index);
            }

            IEquipment eq = Utility.GetSpecificEquipment(101);
            if (eq is SpecialEquipment special)
            {
                special.Traits.Clear();
                special.Traits.AddRange(picked);

                string traitText = string.Join(" / ", special.Traits);
                special.equipmentDetail = $"{traitText} exchange certificate";
                special.equipmentDescriptionEnglish = $"{traitText} exchange certificate";

                EquipmentManager.Instance.AddEquipmentItem(special);
            }
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
    public void UpdateEquipmentStats()
    {
        foreach (var item in equippedItems)
        {
            foreach (var stat in item.GetStats())
            {
                float val = stat.Value;
                if (stat.Key == EquipmentType.AttackSpeed)
                {
                    val *= 0.01f;
                }
                Parent.AddStat(GetStatType(stat.Key), val, false);
            }
            CustomLogger.Log(this, $"UpdateEquipmentStats: {item.GetStats()}");
        }
        if (equippedItems.Count >= 1)
        {
            if (equippedItems.Count < 3)
            {
                if (SelectedAugments.Instance.CheckAugmetExist(1031, Parent.IsAlly))
                {
                    Effect e1 = EffectFactory.CreateUnStackableStatEffect(
                        0, "Augment1031", 300, StatsType.Health, Parent, true
                    );
                    Parent.effectCTRL.AddEffect(e1, Parent);
                }
                if (SelectedAugments.Instance.CheckAugmetExist(1042, Parent.IsAlly))
                {
                    Effect e1 = EffectFactory.CreateUnStackableStatEffect(
                        0, "Augment1042", 500, StatsType.Health, Parent, true
                    );
                    Parent.effectCTRL.AddEffect(e1, Parent);
                }
            }
            else
            {
                if (SelectedAugments.Instance.CheckAugmetExist(1031, Parent.IsAlly))
                {
                    Effect e1 = EffectFactory.CreateUnStackableStatEffect(
                        0, "Augment1031", 450, StatsType.Health, Parent, true
                    );
                    Parent.effectCTRL.AddEffect(e1, Parent);
                }
                if (SelectedAugments.Instance.CheckAugmetExist(1042, Parent.IsAlly))
                {
                    Effect e1 = EffectFactory.CreateUnStackableStatEffect(
                        0, "Augment1042", 750, StatsType.Health, Parent, true
                    );
                    Parent.effectCTRL.AddEffect(e1, Parent);
                }
            }
        }
        else
        {
            Parent.effectCTRL.ClearEffectWithSource("Augment1031");
            Parent.effectCTRL.ClearEffectWithSource("Augment1042");
        }
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
    public StatsContainer GetEqStats()
    {
        var stats = new StatsContainer();
        foreach (var item in equippedItems)
        {
            foreach (var stat in item.GetStats())
            {
                float val = stat.Value;
                if (stat.Key == EquipmentType.AttackSpeed)
                {
                    val *= 0.01f;
                }
                stats.AddValue(GetStatType(stat.Key), val);
            }
        }
        return stats;
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
            case EquipmentType.LifeSteal:
                return StatsType.Lifesteal;
            default:
                return StatsType.Null;
        }
    }
}
