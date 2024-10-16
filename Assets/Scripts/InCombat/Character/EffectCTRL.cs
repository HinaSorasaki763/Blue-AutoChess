using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameEnum;

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
                // 如果效果是永久的，則不再添加
                CustomLogger.Log(this, $"Effect from {effect.Source} is already permanent. Ignored.");
                return;
            }
            else
            {
                // 如果效果不是永久的，則延長持續時間
                existingEffect.Duration += effect.Duration;
                CustomLogger.Log(this, $"Extended duration of {effect.Source} by {effect.Duration:F2} seconds.");
                UpdateEffectNames(); // 更新效果名稱列表
                return;
            }
        }

        // 如果沒有相同來源的效果，則添加新效果
        activeEffects.Add(effect);
        UpdateEffectNames(); // 更新效果名稱列表

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
        effectNames = activeEffects.Select(e => $" {e.Source} : {e.SpecialType}  ({e.Duration:F2}s)").ToList();
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
    public static Effect CreateStunEffect(float duration)
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
            SpecialEffectType.Stun

        );
    }

    public static Effect CreateMarkedEffect(float duration)
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
            SpecialEffectType.Marked

        );
    }

    public static Effect CreateCCImmunityEffect(float duration)
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
            SpecialEffectType.CCImmune

        );
    }
    public static Effect CreateUnTargetableEffect(float duration)
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
            SpecialEffectType.UnTargetable

        );
    }
    public static Effect CreateInvincibleEffect(float duration)
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
            SpecialEffectType.Invincible

        );
    }
    public static Effect CreateHyakkiyakoObserverEffct(int attackPowerIncrease, float duration)
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
            SpecialEffectType.Invincible

        );
    }
}
