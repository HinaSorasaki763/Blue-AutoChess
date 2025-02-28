using GameEnum;
using System.Collections.Generic;

public class HyakkiyakoObserver : CharacterObserverBase
{
    private CharacterCTRL character;
    private int traitLevel;
    private bool hasTriggered = false;

    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//攻擊力,攻擊速度,血量閾值
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(20,30,60)},
            {2, new TraitLevelStats(50,50,60)},
            {3, new TraitLevelStats(130,90,60)}
        };
        return statsByStarLevel;
    }
    public HyakkiyakoObserver(int level, CharacterCTRL character)
    {
        this.character = character;
        traitLevel = level;
        character.effectCTRL.characterCTRL = character;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        if (character.GetHealthPercentage() > healthThreshold)
        {
            ApplyAttackPowerIncrease();
        }
    }
    public override void DeactivateTrait()
    {
        base.DeactivateTrait();
        RemoveAttackPowerIncrease();
    }

    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        if (!activated) return amount;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        float maxHealth = character.GetStat(StatsType.Health);
        float currHealth = character.GetStat(StatsType.currHealth);
        float newHealth = currHealth - amount;
        float newPercentage = newHealth / maxHealth;
        if (!hasTriggered && newPercentage <= healthThreshold)
        {
            hasTriggered = true;
            newHealth = maxHealth * healthThreshold;
            int adjustedAmount = (int)(currHealth - newHealth);
            ForceAttackerToSwitchTarget(source);
            CustomLogger.Log(this, $"[HyakkiyakoObserver] {character.characterStats.name} 攻擊速度提升。");
            return adjustedAmount;
        }
        if (character.GetHealthPercentage() >= healthThreshold)
        {
            ApplyAttackPowerIncrease();
        }
        else
        {
            RemoveAttackPowerIncrease();
        }
        return amount;
    }

    private void ApplyAttackPowerIncrease()
    {
        int attackPowerIncrease = GetTraitObserverLevel()[traitLevel].Data1;
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        var effect = EffectFactory.CreateHyakkiyakoObserverEffct(attackPowerIncrease, 0, character);
        character.effectCTRL.AddEffect(effect);
        CustomLogger.Log(this, $"[HyakkiyakoObserver] {character.characterStats.name} 生命值高于 {healthThreshold * 100}%，攻击力提升。");
    }

    private void RemoveAttackPowerIncrease()
    {
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
        float healthThreshold = GetTraitObserverLevel()[traitLevel].Data3 * 0.01f;
        CustomLogger.Log(this, $"{character.characterStats.name} 生命值低于 {healthThreshold * 100}%，攻击力恢复。");
    }


    private void ForceAttackerToSwitchTarget(CharacterCTRL source)
    {
        CustomLogger.Log(this, $"试图将 {source.characterStats.name} 转移目标");
        Effect unTargetableEffect = EffectFactory.CreateUnTargetableEffect(3f, character);
        character.effectCTRL.AddEffect(unTargetableEffect);
        source.Target = null;
    }
}
