using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class HyakkiyakoObserver : CharacterObserverBase
{
    private CharacterCTRL character;
    private int traitLevel;
    private bool hasTriggered = false;
    private bool augment117_triggered = false;
    private bool augment119_triggered;

    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//攻擊力,攻擊速度,血量閾值
            {0, new TraitLevelStats(0,0,101)},
            {1, new TraitLevelStats(20,30,60)},
            {2, new TraitLevelStats(50,50,60)},
            {3, new TraitLevelStats(130,90,60)}
        };
        return statsByStarLevel;
    }
    public HyakkiyakoObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.character = character;
        traitLevel = level;
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
        if (level == 0) return;
        character.effectCTRL.characterCTRL = character;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        ApplyAttackPowerIncrease();
    }

    public override void CharacterUpdate()
    {
        if (augment119_triggered && !character.effectCTRL.HaveEffect("HyakkiyakoDyingEffect"))
        {
            Effect effect = EffectFactory.HyakkiyakoDyingEffect();
            character.effectCTRL.AddEffect(effect, character);
        }
    }
    public override void DeactivateTrait()
    {
        base.DeactivateTrait();
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
        RemoveAttackPowerIncrease();
        RemoveAttackSpeedIncrease();
    }

    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        if (!activated) return amount;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        float maxHealth = character.GetStat(StatsType.Health);
        float currHealth = character.GetStat(StatsType.currHealth);
        float newHealth = currHealth - amount;
        float newPercentage = newHealth / maxHealth;
        if (SelectedAugments.Instance.CheckAugmetExist(117,character.IsAlly) && !augment117_triggered)
        {
            float SRT_HyakkiyakoThreshold = character.GetStat(StatsType.Health) - BattlingProperties.Instance.GetSRTStats(character.IsAlly).GetStat(StatsType.Health);
            if (newHealth <= SRT_HyakkiyakoThreshold)
            {
                newHealth = SRT_HyakkiyakoThreshold;
                int adjustedAmount = (int)(currHealth - newHealth);
                ForceAttackerToSwitchTarget(source);
                augment117_triggered = true;
                return adjustedAmount;
            }
        }
        if (!hasTriggered && newPercentage <= healthThreshold)
        {
            hasTriggered = true;
            newHealth = maxHealth * healthThreshold;
            int adjustedAmount = (int)(currHealth - newHealth);
            ForceAttackerToSwitchTarget(source);
            RemoveAttackPowerIncrease();
            CustomLogger.Log(this, $"[HyakkiyakoObserver] {character.characterStats.name} 攻擊速度提升。");
            return adjustedAmount;
        }
        if (character.GetHealthPercentage() >= healthThreshold)
        {
            ApplyAttackPowerIncrease();
            RemoveAttackSpeedIncrease();
        }
        else
        {
            ApplyAttackSpeedIncrease();
            RemoveAttackPowerIncrease();
        }
        return amount;
    }

    private void ApplyAttackPowerIncrease()
    {
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
        int attackPowerIncrease = GetTraitObserverLevel()[traitLevel].Data1;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        var effect = EffectFactory.CreateHyakkiyakoObserverEffct(attackPowerIncrease, 0, character);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.Attack, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.Attack, -effect.Value, effect.Source)
        );
        character.effectCTRL.AddEffect(effect, character);

        CustomLogger.Log(this, $"[HyakkiyakoObserver] {character.characterStats.name} 生命值高于 {healthThreshold * 100}%，攻击力提升。");
    }
    private void ApplyAttackSpeedIncrease()
    {
        float attackSpeedIncrease = GetTraitObserverLevel()[traitLevel].Data1 * 0.01f;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        var effect = EffectFactory.CreateHyakkiyakoObserverAttackSpeedEffct(0, 0, character);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.AttackSpeed, attackSpeedIncrease, effect.Source),
            (character) => character.ModifyStats(StatsType.AttackSpeed, -attackSpeedIncrease, effect.Source)
        );
        character.effectCTRL.AddEffect(effect, character);
    }
    private void RemoveAttackSpeedIncrease()
    {
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackSpeed");
    }
    private void RemoveAttackPowerIncrease()
    {
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
    }


    private void ForceAttackerToSwitchTarget(CharacterCTRL source)
    {
        Effect unTargetableEffect = EffectFactory.CreateUnTargetableEffect(3f, character);
        character.effectCTRL.AddEffect(unTargetableEffect, character);
        source.Target = null;
    }
    public override void CharacterStart(CharacterCTRL character)
    {
        augment117_triggered = false;
        augment119_triggered = false;
        hasTriggered = false;
    }
    public override bool BeforeDying(CharacterCTRL parent)
    {
        if (!SelectedAugments.Instance.CheckAugmetExist(119,parent.IsAlly)) return false;
        if (augment119_triggered) return false;
        else
        {
            if (character.OGBody.TryGetComponent<SkinnedMeshRenderer>(out var s))
            {
                var mats = s.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string oldName = mats[i].name;
                    mats[i] = ResourcePool.Instance.BlackMat;
                    CustomLogger.Log(this, $"[Swap] material {i}: {oldName} → {mats[i].name}");
                }
                s.materials = mats;
            }
            augment119_triggered = true;
            character.Invincible = true;
            character.HealToFull(character);
            return true;
        }
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        CustomLogger.Log(this, $"character {source} dealt {damage} to {target} at {Time.time}");
        if (SelectedAugments.Instance.CheckAugmetExist(118,source.IsAlly) && Utility.GetRandfloat(source) >=0.5f)
        {
            TrinityManager.Instance.AddStack(target.transform.position, detailedSource, target.CurrentHex, source);
        }

    }
}
