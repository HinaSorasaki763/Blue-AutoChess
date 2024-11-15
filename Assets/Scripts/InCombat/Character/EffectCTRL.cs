using GameEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectCTRL : MonoBehaviour
{
    private List<Effect> activeEffects = new List<Effect>();
    public List<string> effectNames = new List<string>(); // 用於顯示效果名稱
    private ModifierCTRL modifierCTRL;
    public CharacterCTRL characterCTRL;

    private void Awake()
    {
        modifierCTRL = GetComponent<ModifierCTRL>();
        characterCTRL = GetComponent<CharacterCTRL>();
        CustomLogger.Log(this, $"Getting {gameObject.name}'s ctrl");

        if (characterCTRL == null)
        {
            CustomLogger.LogError(this, "No ctrl");
        }
    }

    // 添加效果
    public void AddEffect(Effect effect)
    {
        var existingEffect = activeEffects.FirstOrDefault(e => e.Source == effect.Source);

        if (existingEffect != null)
        {
            if (existingEffect.IsPermanent)
            {
                CustomLogger.Log(this, $"Effect from {effect.Source} is already permanent. Ignored.");
                if (effect.Value > existingEffect.Value)
                {
                    CustomLogger.Log(this, $"Effect from {effect.Source} is permanent, but new value {effect.Value} is higher than existing value {existingEffect.Value}. Overwriting.");
                    existingEffect.UpdateValue(effect.Value);
                    UpdateEffectNames();
                }
                else
                {
                    CustomLogger.Log(this, $"Effect from {effect.Source} is permanent, and new value {effect.Value} is not higher. Ignored.");
                }
                return;
            }
            else
            {
                if (effect.EffectType == EffectType.Positive)
                {
                    characterCTRL.AudioManager.PlayBuffedSound();
                }

                float newDuration = Mathf.Max(existingEffect.Duration, effect.Duration);
                CustomLogger.Log(this, $"Updated duration of {effect.Source} to {newDuration:F2} seconds.");
                existingEffect.Duration = newDuration;
                UpdateEffectNames(); // 更新效果名稱列表
                return;
            }

        }
        if (effect.EffectType == EffectType.Positive)
        {
            characterCTRL.AudioManager.PlayBuffedSound();
        }
        activeEffects.Add(effect);
        UpdateEffectNames(); 
        if (effect.SpecialType == SpecialEffectType.None)
        {
            modifierCTRL.AddStatModifier(effect.ModifierType, effect.Value, effect.Source, effect.IsPermanent, effect.Duration);
        }
        effect.OnApply.Invoke(characterCTRL);
    }

    // 移除效果
    public void RemoveEffect(Effect effect)
    {
        CustomLogger.Log(this, $"try removing {gameObject.name}'s {effect.SpecialType} effect");
        effect.OnRemove.Invoke(characterCTRL);
        activeEffects.Remove(effect);
        UpdateEffectNames();

        if (effect.SpecialType == SpecialEffectType.None)
        {
            modifierCTRL.RemoveStatModifier(effect.ModifierType, effect.Source);
        }
    }
    private void UpdateEffectNames()
    {
        effectNames = activeEffects.OrderByDescending(e => e.Source.Length).Select(e => $" {e.Source} : {e.SpecialType}  ({e.Duration:F2}s)").ToList();
    }
    public void ClearEffectWithSource(String source)
    {
        var effectsToRemove = activeEffects.Where(e => e.Source == source).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }
    public void ClearEffects(EffectType effectType)
    {
        var effectsToRemove = activeEffects.Where(e => e.EffectType == effectType).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }

    public void ClearEffects(SpecialEffectType effectType)
    {
        var effectsToRemove = activeEffects.Where(e => e.SpecialType == effectType).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }
    public Effect GetEffect(string source)
    {
        return activeEffects.FirstOrDefault(e => e.Source == source);
    }

    private void Update()
    {
        List<Effect> expiredEffects = new List<Effect>();

        foreach (var effect in activeEffects)
        {
            if (!effect.IsPermanent)
            {
                effect.Duration -= Time.deltaTime;
                if (effect.Duration <= 0f)
                {
                    expiredEffects.Add(effect);
                }
            }
        }

        foreach (var expiredEffect in expiredEffects)
        {
            RemoveEffect(expiredEffect);
        }
    }
}

public static class EffectFactory
{
    public static Effect StatckableIncreaseStatsEffct(float duration, string source, float amount, StatsType statsType, CharacterCTRL parent, bool isPermanent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            $"{source}Increase{statsType}",
            isPermanent,
            null, // 暫不設置委派
            null,
            duration,
            SpecialEffectType.None,
            parent
        );
    }



    public static Effect UnStatckableIncreaseStatsEffct(float duration, int amount, StatsType statsType, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            $"Increase{statsType}",
            false,
            (character) => character.ModifyStats(statsType, amount),
            (character) => character.ModifyStats(statsType, -amount),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateStunEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            0,
            "StunSkill",
            false,
            (character) => character.Stun(true),
            (character) => character.Stun(false),
            duration,
            SpecialEffectType.Stun,
            parent
        );
    }

    public static Effect CreateMarkedEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            0,
            "MarkSkill",
            false,
            (character) => character.SetMarked(true),
            (character) => character.SetMarked(false),
            duration,
            SpecialEffectType.Marked,
            parent
        );
    }

    public static Effect ClarityEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "ClarityEffect",
            false,
            (character) => character.Clarity(),
            (character) => character.SetCCImmune(false),
            duration,
            SpecialEffectType.CCImmune,
            parent
        );
    }

    public static Effect CreateCCImmunityEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "ImmunitySkill",
            false,
            (character) => character.SetCCImmune(true),
            (character) => character.SetCCImmune(false),
            duration,
            SpecialEffectType.CCImmune,
            parent
        );
    }

    public static Effect CreateUnTargetableEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "UnTargetable",
            false,
            (character) => character.SetUnTargetable(false),
            (character) => character.SetUnTargetable(true),
            duration,
            SpecialEffectType.UnTargetable,
            parent
        );
    }

    public static Effect CreateInvincibleEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "InvincibleSkill",
            true,
            (character) => character.SetInvincible(true),
            (character) => character.SetInvincible(false),
            duration,
            SpecialEffectType.Invincible,
            parent
        );
    }

    public static Effect CreateHyakkiyakoObserverEffct(int attackPowerIncrease, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            attackPowerIncrease,
            "HyakkiyakoObserver_AttackPower",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            duration,
            SpecialEffectType.Invincible,
            parent
        );
    }

    public static Effect CreateAkoCritChanceBuff(int critChance, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            critChance,
            "AkoCritChanceBuff",
            false,
            (character) => character.ModifyStats(StatsType.CritChance, critChance),
            (character) => character.ModifyStats(StatsType.CritChance, -critChance),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateAkoCritRatioBuff(int critRatio, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            critRatio,
            "AkoCritRatioBuff",
            false,
            (character) => character.ModifyStats(StatsType.CritRatio, critRatio),
            (character) => character.ModifyStats(StatsType.CritRatio, -critRatio),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateAyaneResistanceBuff(int resistance, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            resistance,
            "AyaneResistanceBuff",
            false,
            (character) => character.ModifyStats(StatsType.Resistence, resistance),
            (character) => character.ModifyStats(StatsType.Resistence, -resistance),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateHarukaMinusAtkEffect(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.DamageDealt,
            amount,
            "HarukaMinusAtkEffect",
            false,
            (character) => character.ModifyStats(StatsType.Attack, -amount),
            (character) => character.ModifyStats(StatsType.Attack, amount),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateSerikaRageEffect(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            amount,
            "SerikaRageEffect",
            false,
            (character) => character.ModifyStats(StatsType.Attack, amount),
            (character) => character.ModifyStats(StatsType.Attack, -amount),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateShizukoEffect(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            amount,
            "ShizukoEffect",
            false,
            (character) => character.ModifyStats(StatsType.Accuracy, amount),
            (character) => character.ModifyStats(StatsType.Accuracy, -amount),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateWakamoEffect(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            amount,
            "WakamoEffect",
            false,
            (character) => character.SetWakamoMark(50, parent), // TODO: 用真實數據代替
            (character) => character.WakamoMarkEnd(),
            duration,
            SpecialEffectType.None,
            parent
        );
    }

    public static Effect CreateKayokoFearEffct(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            amount,
            "KayokoFearEffect",
            false,
            (character) => character.Stun(true), // TODO: 記得新增"恐懼"效果!
            (character) => character.Stun(false), // TODO: 記得新增"恐懼"效果!
            duration,
            SpecialEffectType.Fear,
            parent
        );
    }

    public static Effect CreateTsubakiFearEffct(int amount, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            amount,
            "TsubakiTauntEffect",
            false,
            (character) => character.SetTaunt(true),
            (character) => character.SetTaunt(false),
            duration,
            SpecialEffectType.Taunt,
            parent
        );
    }
}

