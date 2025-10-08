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
    public void AddEffect(Effect effect, CharacterCTRL c)
    {
        if (effect.EffectType == EffectType.Negative && effect.Parent != null)
        {
            (float length, float effectiveness) = effect.Parent.BeforeApplyingNegetiveEffect(effect.Duration, effect.Value);
        }
        var existingEffect = activeEffects.FirstOrDefault(e => e.Source == effect.Source);

        if (existingEffect != null)
        {
            if (existingEffect.IsPermanent)
            {
                if (existingEffect.Stackable)
                {
                    existingEffect.UpdateValue(existingEffect.Value + effect.Value, c);
                    UpdateEffectNames();
                }
                if (!existingEffect.Stackable)
                {
                    existingEffect.UpdateValue(effect.Value, c);
                    UpdateEffectNames();
                }
                return;
            }
            else
            {
                if (effect.EffectType == EffectType.Positive && !effect.IsLogisticBuff)
                {
                    characterCTRL.AudioManager.PlayBuffedSound();
                }

                float newDuration = Mathf.Max(existingEffect.Duration, effect.Duration);
                CustomLogger.Log(this, $"Updated duration of {effect.Source} to {newDuration:F2} seconds.");
                if (existingEffect.Source != "WakamoEnhancedMark")
                {
                    existingEffect.Duration = newDuration;
                }
                UpdateEffectNames();
                return;
            }

        }
        if (effect.EffectType == EffectType.Positive && !effect.IsLogisticBuff)
        {
            characterCTRL.AudioManager.PlayBuffedSound();
        }
        activeEffects.Add(effect);
        UpdateEffectNames();
        if (effect.SpecialType == SpecialEffectType.None)
        {
            //modifierCTRL.AddStatModifier(effect.ModifierType, effect.Value, effect.Source, effect.IsPermanent, effect.Duration);
        }
        effect.OnApply.Invoke(characterCTRL);
    }

    // 移除效果
    public void RemoveEffect(Effect effect)
    {
        CustomLogger.Log(this, $"try removing {gameObject.name}'s {effect.SpecialType} effect");
        activeEffects.Remove(effect);
        effect.OnRemove.Invoke(characterCTRL);

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
    public int ClearEffects(EffectType effectType)
    {
        var effectsToRemove = activeEffects.Where(e => e.EffectType == effectType).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
        return effectsToRemove.Count;
    }

    public void ClearEffects(SpecialEffectType effectType)
    {
        var effectsToRemove = activeEffects.Where(e => e.SpecialType == effectType).ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }
    public bool HaveEffect(string source)
    {
        return activeEffects.FirstOrDefault(e => e.Source == source) != null;
    }
    public Effect GetEffect(string source)
    {
        return activeEffects.FirstOrDefault(e => e.Source == source);
    }
    public void OnParentCastSkillFinished()
    {
        // 先收集需要移除的效果
        List<Effect> effectsToRemove = new List<Effect>();

        foreach (var effect in activeEffects)
        {
            if (effect.ClearEffectCondition == ClearEffectCondition.OnSkillCastFinished)
            {
                effectsToRemove.Add(effect);
            }
        }

        // 遍歷收集到的效果並移除
        foreach (var effect in effectsToRemove)
        {
            CustomLogger.LogWarning(this, $"removing effect {effect.GetType()}");
            RemoveEffect(effect);
        }
    }
    public void ClearAllEffect()
    {
        List<Effect> effectsToRemove = new List<Effect>();
        foreach (var item in activeEffects)
        {
            effectsToRemove.Add(item);
        }
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }
    public List<Effect> GetStatsEffects()
    {
        return activeEffects.Where(e => e.StatsEffect).ToList();
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
        UpdateEffectNames();
        foreach (var expiredEffect in expiredEffects)
        {
            RemoveEffect(expiredEffect);
        }
    }
}

public static class EffectFactory
{
    public static Effect StatckableStatsEffct(float duration, string source, float amount, StatsType statsType, CharacterCTRL parent, bool isPermanent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            $"{source} adjust {statsType} {amount}",
            isPermanent,
            null,
            null,
            duration,
            SpecialEffectType.None,
            parent,
            true,
            ClearEffectCondition.Never,
            false,
            true
        );
    }



    public static Effect UnStatckableStatsEffct(float duration, string source, float amount, StatsType statsType, CharacterCTRL parent, bool isPermanent, bool isLogistic = false)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            $"{source}",
            isPermanent,
            null,
            null,
            duration,
            SpecialEffectType.None,
            parent,
            false,
            ClearEffectCondition.Never,
            isLogistic,
            true
        );
    }

    public static Effect CreateStunEffect(float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            0,
            "Stun",
            false,
            (character) => character.Stun(true),
            (character) => character.Stun(false),
            duration,
            SpecialEffectType.Stun,
            parent
        );
    }

    public static Effect CreateMarkedEffect(CharacterCTRL parent)
    {

        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            0,
            "MarkSkill",
            true,
            (character) => character.SetMarked(true),
            (character) => character.SetMarked(false),
            0,
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
            false,
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
            $"HyakkiyakoObserver_AttackPower",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            duration,
            SpecialEffectType.None,
            parent,
            false,
            default,
            false,
            true
        );
    }
    public static Effect CreateHyakkiyakoObserverAttackSpeedEffct(int attackPowerIncrease, float duration, CharacterCTRL parent)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.DamageDealt,
            attackPowerIncrease,
            "HyakkiyakoObserver_AttackSpeed",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            duration,
            SpecialEffectType.None,
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
            parent,
            false,
            ClearEffectCondition.Never,
            true,
            true
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
            parent,
            false,
            ClearEffectCondition.Never,
            true,
            true
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
            (character) => character.OnWakamoMarkCountedDown(),
            duration,
            SpecialEffectType.None,
            parent
        );
    }


    public static Effect CreateTsubakiTauntEffct(int amount, float duration, CharacterCTRL parent)
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
    public static Effect CreateAkoActiveSkillBuff(int amount, float duration, CharacterCTRL parent)
    {
        var modifiers = new Dictionary<StatsType, float>
        {
            { StatsType.CritChance, amount },
            { StatsType.CritRatio, amount },
        };


        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            "AkoActiveSkillBuff",
            true,
            (character) => character.ModifyMultipleStats(modifiers, "AkoBuffEffect"),
            (character) => character.ModifyMultipleStats(modifiers, "AkoBuffEffect", isRevert: true),
            duration,
            SpecialEffectType.None,
            parent,
            false,
            ClearEffectCondition.OnSkillCastFinished
        );

    }
    public static Effect CreateAkoEnhancedActiveSkillBuff(int amount, float duration, CharacterCTRL parent)
    {
        var modifiers = new Dictionary<StatsType, float>
        {
            { StatsType.CritChance, amount },
            { StatsType.CritRatio, amount },
        };


        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            "AkoEnhancedSkillBuff",
            true,
            (character) => character.ModifyMultipleStats(modifiers, "BuffEffect"),
            (character) => character.ModifyMultipleStats(modifiers, "BuffEffect", isRevert: true),
            duration,
            SpecialEffectType.None,
            parent,
            false,
            ClearEffectCondition.OnSkillCastFinished
        );

    }
    public static Effect CreateAntiHealEffect(float duration, CharacterCTRL parent)
    {

        return new Effect(
            EffectType.Negative,
            ModifierType.None,
            0,
            "AntiHealEffect",
            false,
            (character) => character.SetTaunt(true),
            (character) => character.SetTaunt(false),
            duration,
            SpecialEffectType.None,
            parent
        );

    }
    public static Effect CreateAbydosEffect(bool isAbydos, int level, bool isally)
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(8,30,2)},
            {2, new TraitLevelStats(10,45,3)},
            {3, new TraitLevelStats(12,70,5)},
            {4, new TraitLevelStats(20,100,10)}
        };
        int effectiveness = statsByStarLevel[level].Data2 * (isAbydos ? 1 : -1);
        if (SelectedAugments.Instance.CheckAugmetExist(100, isally))
        {
            effectiveness = 0;
        }
        return new Effect(
            isAbydos ? EffectType.Positive : EffectType.Negative,
            ModifierType.None,
            effectiveness,
            "AbydosEffect",
            false,
            (character) => character.AbydosBuff(isAbydos, effectiveness, statsByStarLevel[level].Data3, true),
            (character) => character.AbydosBuff(isAbydos, -effectiveness, statsByStarLevel[level].Data3, false),
            1,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect CreateAbydosMillenniumEffect(int level)
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {1, new TraitLevelStats(2)},
            {2, new TraitLevelStats(3)},
            {3, new TraitLevelStats(5)},
            {4, new TraitLevelStats(10)}
        };
        int percent = statsByStarLevel[level].Data1;
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "AbydosMillenniumEffect",
            false,
            (character) => character.AbydosMillenniumBuff(percent, true),
            (character) => character.AbydosMillenniumBuff(percent, false),
            1,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect SakurakoBuff()
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "SakurakoBuff",
            false,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            5,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect AbydosEnhancedMark()
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "AbydosMark",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            100,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect CreateAriusEffect()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "AriusTraitEffect",
            true,
            (character) => character.AddStat(StatsType.PercentageResistence, 50),
            (character) => character.AddStat(StatsType.PercentageResistence, -50),
            0,
            SpecialEffectType.None,
            null
        );


    }
    public static Effect HinaEffect()
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "HinaMark",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            0,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect HiyoriEffect()
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "HiyoriMark",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            0,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect IzunaAttackSpeedBuff()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            3,
            "IzunaAttackSpeedBuff",
            true,
            (character) => character.AddStat(StatsType.AttackSpeed, 0.5f),
            (character) => character.EmptyEffectFunction(),
            0,
            SpecialEffectType.None,
            null,
            true,
            ClearEffectCondition.Never,
            false,
            true
        );

    }
    public static Effect KazusaMark()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "KazusaMark",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            0,
            SpecialEffectType.None,
            null
        );

    }
    public static Effect SerikaAddGold()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "SerikaAddGold",
            true,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            5,
            SpecialEffectType.None,
            null
        );

    }
    public static Effect CreateSeiyaEnhancedSkillBuff(int amount, float duration, CharacterCTRL parent)
    {
        var modifiers = new Dictionary<StatsType, float>
        {
            { StatsType.Attack, amount },
        };


        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            amount,
            "SeiyaEnhancedSkillBuff",
            true,
            (character) => character.ModifyMultipleStats(modifiers, "SeiyaBuffEffect"),
            (character) => character.ModifyMultipleStats(modifiers, "SeiyaBuffEffect", isRevert: true),
            duration,
            SpecialEffectType.None,
            parent,
            false,
            ClearEffectCondition.OnSkillCastFinished
        );

    }
    public static Effect MiyuEnhancedSkillEffect()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            5,
            "MiyuEnhancedSkillEffect",
            false,
            (character) => character.EmptyEffectFunction(),
            (character) => character.EmptyEffectFunction(),
            5,
            SpecialEffectType.None,
            null
        );

    }
    public static Effect TsurugiEnhancedSkill(CharacterCTRL parent, int ratio)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "TsurugiEnhancedSkill",
            true,
            (character) => character.EmptyEffectFunction(),//這裡有錯
            (character) => character.EmptyEffectFunction(),
            0,
            SpecialEffectType.None,
            null
        );
    }
    public static Effect WakamoEnhancedMark(CharacterCTRL parent, int ratio)
    {
        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "WakamoEnhancedMark",
            false,
            (character) => character.SetWakamoMark(ratio, parent),
            (character) => character.OnWakamoEnhancedMarkEnd(),
            5,
            SpecialEffectType.None,
            null
        );
    }

    public static Effect HyakkiyakoDyingEffect()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "HyakkiyakoDyingEffect",
            false,
            (character) => character.HyakkiyakoDyingEffectStart(),
            (character) => character.HyakkiyakoDyingEffectEnd(),
            1,
            SpecialEffectType.None,
            null
        );

    }
    public static Effect OverTimeEffect()
    {

        return new Effect(
            EffectType.Positive,
            ModifierType.None,
            0,
            "OverTimeEffect",
            true,
            (character) => character.BattleOverTime(),
            (character) => character.BattleOverTime(),
            0,
            SpecialEffectType.None,
            null
        );

    }
}

