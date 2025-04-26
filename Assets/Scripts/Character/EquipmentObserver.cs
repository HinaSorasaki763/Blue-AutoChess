using GameEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ItemObserverFactory
{
    private static Dictionary<int, Func<CharacterObserverBase>> observerMap =
        new Dictionary<int, Func<CharacterObserverBase>>
        {
        { 0, () => new Amulet_Observer() },
        { 1, () => new Bag_Observer() },
        { 2, () => new Cap_Observer() },
        { 3, () => new Gloves_Observer() },
        { 4, () => new Hairband_Observer() },
        { 5, () => new Watch_Observer() },
        { 6, () => new Amulet_AmuletObserver() },
        { 7, () => new Amulet_BagObserver() },
        { 8, () => new Amulet_CapObserver() },
        { 9, () => new Amulet_GlovesObserver() },
        { 10, () => new Amulet_HairbandObserver() },
        { 11, () => new Amulet_WatchObserver() },
        { 12, () => new Bag_BagObserver() },
        { 13, () => new Bag_CapObserver() },
        { 14, () => new Bag_GlovesObserver() },
        { 15, () => new Bag_HairbandObserver() },
        { 16, () => new Bag_WatchObserver() },
        { 17, () => new Cap_CapObserver() },
        { 18, () => new Cap_GlovesObserver() },
        { 19, () => new Cap_HairbandObserver() },
        { 20, () => new Cap_WatchObserver() },
        { 21, () => new Gloves_GlovesObserver() },
        { 22, () => new Gloves_HairbandObserver() },
        { 23, () => new Gloves_WatchObserver() },
        { 24, () => new Hairband_HairbandObserver() },
        { 25, () => new Hairband_WatchObserver() },
        { 26, () => new Watch_WatchObserver() }
    };

    public static CharacterObserverBase GetObserverByIndex(int index)
    {
        if (observerMap.TryGetValue(index, out var constructor))
        {
            return constructor();
        }

        CustomLogger.LogWarning("CompleteItemObserverFactory",
            $"No observer found for index {index}, returning null");
        return null;
    }
}

public class Amulet_Observer : CharacterObserverBase
{

}
public class Bag_Observer : CharacterObserverBase
{
}
public class Cap_Observer : CharacterObserverBase
{
}
public class Gloves_Observer : CharacterObserverBase
{
}
public class Hairband_Observer : CharacterObserverBase
{
}
public class Watch_Observer : CharacterObserverBase
{
}
public class Amulet_AmuletObserver : CharacterObserverBase
{
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        character.Addmana(10);
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }
}
public class Amulet_BagObserver : CharacterObserverBase
{
    private bool trigger = false;
    public override void OnBattleEnd(bool isVictory)
    {
        trigger = false;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        if (character.GetHealthPercentage() <= 0.6f && !trigger)
        {
            trigger = true;
            character.Addmana((int)character.GetStat(StatsType.MaxMana));
        }
    }
}
public class Amulet_CapObserver : CharacterObserverBase
{
    public override void OnCastedSkill(CharacterCTRL character)
    {
        Effect effect = EffectFactory.StatckableStatsEffct(0, "Aumlet_CapObserver", 20, StatsType.Attack, character, true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.Attack, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.Attack, -effect.Value, effect.Source)
        );
        character.effectCTRL.AddEffect(effect, character);
    }
}
public class Amulet_GlovesObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        CustomLogger.Log(this, $"character {source} attacking with Cap_GloveObserver to {target}");
        source.StealMana(target);
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Amulet_HairbandObserver : CharacterObserverBase
{
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {

        int manaAmount = (int)(5 * (amount / (amount + 10)));//TODO: 調整數值
        CustomLogger.Log(this, $"Amulet_HairbandObserver AddMana{manaAmount}");
        character.Addmana(manaAmount);
    }
}
public class Amulet_WatchObserver : CharacterObserverBase
{

    //在CharacterCTRL-public void Heal(int amount, CharacterCTRL source)之中已完成
}
public class Bag_BagObserver : CharacterObserverBase
{
    private int count = 20;
    public override void OnBattleEnd(bool isVictory)
    {
        count = 20;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        int healamount = (int)(character.GetStat(StatsType.Health) * 0.02f);
        if (character.GetHealthPercentage() <= 0.4f && count > 0)
        {
            count--;
            character.Heal(healamount, character);

        }
    }
}
public class Bag_CapObserver : CharacterObserverBase
{
    private bool trigger = false;
    public override void OnBattleEnd(bool isVictory)
    {
        trigger = false;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource = "default",bool recursion = true)
    {
        int threshold = (int)(character.GetStat(StatsType.Health)*0.5f);
        if (!trigger && character.GetStat(StatsType.currHealth) <= threshold)
        {
            trigger = true;
            Effect effect = EffectFactory.UnStatckableStatsEffct(0, "Bag_CapObserver", 20, StatsType.PercentageResistence, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.PercentageResistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.PercentageResistence, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
        }
    }
}
public class Bag_GlovesObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource == "NormalAttack")
        {
            target.GetHit((int)(target.GetStat(StatsType.currHealth) * 0.04f), source, "Amulet_WatchObserver", iscrit);
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Bag_HairbandObserver : CharacterObserverBase
{
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource = "default",bool recursion = true)
    {
        if (isCrit)
        {
            source.Heal((int)(amount * 0.1f), character);
        }
    }
}
public class Bag_WatchObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        Effect effect = EffectFactory.CreateAntiHealEffect(5, target);
        target.effectCTRL.AddEffect(effect,target);
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Cap_CapObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource != "Cap_CapObserver")
        {
            target.GetHit((int)(damage * 0.12f), source, "Cap_CapObserver", iscrit);
        }
    }
}
public class Cap_GlovesObserver : CharacterObserverBase
{
    private int additionHitCount = 0;
    readonly int dmg = 100;
    public override void OnCastedSkill(CharacterCTRL character)
    {
        additionHitCount += 5;
        base.OnCastedSkill(character);
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource == "NormalAttack" && additionHitCount > 0)
        {
            additionHitCount--;
            target.GetHit(dmg, source, "Cap_GlovesObserver", iscrit);
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Cap_HairbandObserver : CharacterObserverBase
{
    private bool trigger = false;
    public override void OnBattleEnd(bool isVictory)
    {
        trigger = false;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        if (character.GetHealthPercentage() <= 0.6f && !trigger)
        {
            trigger = true;
            character.AddShield((int)(character.GetStat(StatsType.Health) * 0.2f), 5, character);
            Effect effect = EffectFactory.StatckableStatsEffct(0, "Cap_HairbandObserver", 20, StatsType.Attack, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Attack, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Attack, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
        }
    }
}
public class Cap_WatchObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource == "NormalAttack" && iscrit)
        {
            Effect effect = EffectFactory.StatckableStatsEffct(0, "Cap_WatchObserver", 1, StatsType.CritRatio, source, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.CritRatio, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.CritRatio, -effect.Value, effect.Source)
            );
            source.effectCTRL.AddEffect(effect,source);
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Gloves_GlovesObserver : CharacterObserverBase
{
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        Effect effect = EffectFactory.StatckableStatsEffct(0, "Gloves_GlovesObserver", 50, StatsType.AttackSpeed, character, true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
        );
        character.effectCTRL.AddEffect(effect, character);
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }
}
public class Gloves_HairbandObserver : CharacterObserverBase
{
    public override void OnCastedSkill(CharacterCTRL character)
    {
        character.AddShield((int)character.GetStat(StatsType.MaxMana) * 10, 5, character);
        base.OnCastedSkill(character);
    }
}
public class Gloves_WatchObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (iscrit)
        {
            if (detailedSource == "NormalAttack")
            {
                Effect effect = EffectFactory.StatckableStatsEffct(0, "Gloves_GlovesObserver", 5, StatsType.AttackSpeed, source, true);
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
                    (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
                );
                source.effectCTRL.AddEffect(effect,source);

            }
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class Hairband_HairbandObserver : CharacterObserverBase
{
    public float time;
    readonly int cooldown = 1;
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource = "default",bool recursion = true)
    {
        if (Time.time - time > cooldown)
        {
            time = Time.time;
            foreach (var item in character.CurrentHex.GetCharacterOnNeighborHex(1, false))
            {
                if (item.IsAlly != character.IsAlly)
                {
                    item.GetHit((int)(amount * 0.1f), character, "Hairband_HairbandObserver", isCrit);
                }
            }
        }
    }
}
public class Hairband_WatchObserver : CharacterObserverBase
{
    private int count;
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (iscrit)
        {
            count++;
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource = "default", bool recursion = true)
    {
        if (count > 0)
        {
            count--;
            character.Heal((int)(amount * 0.15f), character);
        }
    }
}
public class Watch_WatchObserver : CharacterObserverBase
{
    //爆擊傷害提升，透過數值加成完成
}