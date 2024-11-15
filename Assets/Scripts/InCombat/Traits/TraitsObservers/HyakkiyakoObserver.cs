using GameEnum;
using UnityEngine;

public class HyakkiyakoObserver : CharacterObserverBase
{
    private CharacterCTRL character;
    private float healthThreshold = 0.6f; // 60%
    private bool hasTriggered = false;
    private int attackPowerIncrease = 20; // 攻击力提升 20%
//    private int attackSpeedBonus = 30; // 攻击速度提升 30%

    public HyakkiyakoObserver(int level, CharacterCTRL character)
    {
        this.character = character;
        character.effectCTRL.characterCTRL = character;
        if (character.GetHealthPercentage() > healthThreshold)
        {
            ApplyAttackPowerIncrease();
        }
    }

    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
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
            Debug.Log($"[HyakkiyakoObserver] {character.characterStats.name} 攻擊速度提升。");
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
        var effect = EffectFactory.CreateHyakkiyakoObserverEffct(attackPowerIncrease,0, character);
        character.effectCTRL.AddEffect(effect);
        Debug.Log($"[HyakkiyakoObserver] {character.characterStats.name} 生命值高于 {healthThreshold * 100}%，攻击力提升。");
    }

    private void RemoveAttackPowerIncrease()
    {
        character.effectCTRL.ClearEffectWithSource("HyakkiyakoObserver_AttackPower");
        Debug.Log($"[HyakkiyakoObserver] {character.characterStats.name} 生命值低于 {healthThreshold * 100}%，攻击力恢复。");
    }


    private void ForceAttackerToSwitchTarget(CharacterCTRL source)
    {
        Debug.Log($"[HyakkiyakoObserver] 试图将 {source.characterStats.name} 转移目标");
        Effect unTargetableEffect = EffectFactory.CreateUnTargetableEffect(3f, character);
        character.effectCTRL.AddEffect(unTargetableEffect);
        source.Target = null;
    }
}
