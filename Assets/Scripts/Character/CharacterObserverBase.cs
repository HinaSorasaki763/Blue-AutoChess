
using GameEnum;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public abstract class CharacterObserverBase
{
    public virtual void CharacterUpdate()
    {

    }
    public virtual void CharacterStart(CharacterCTRL character)
    {

    }
    public virtual void OnAttacking(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"Character : {character.characterStats.name} OnAttacking.");
    }
    public virtual void OnTraitLevelChanged(int level,CharacterCTRL character)
    {
        CustomLogger.Log(this, $"Character : {character.characterStats.name} Trait level changed to {level}.");
    }

    public virtual void OnManaFull(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} has full mana.");
    }

    public virtual void OnCrit(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} landed a critical hit.");
    }
    public virtual void OnHealthChanged(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} Health Changed");
    }
    public virtual int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        // 默认实现，不修改伤害值
        return amount;
    }
    public virtual void GetHit(CharacterCTRL character,CharacterCTRL source,float amount)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} get hit by {source.characterStats.name} for {amount}");
        character.GetHit((int)amount,source);
    }
    public virtual void OnKilledEnemy(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} killed an enemy.");
    }

    public virtual void OnDying(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is dying.");
    }

    public virtual void OnCastedSkill(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} casted a skill.");
    }
    public virtual void OnSkillFinished(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} skill finished.");
    }

    public virtual void OnReceivingAssistance(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is receiving assistance.");
    }

    public virtual void OnGettingCrowdControlled(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is being crowd controlled.");
    }

    public virtual void OnDamaging(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} dealt damage.");
    }
    public virtual void OnLogistic(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} Logistic.");
    }
    public virtual void OnBattleEnd(bool isVictory)
    {
        string result = isVictory ? "victory" : "defeat";
        CustomLogger.Log(this,$"Battle ended with {result}.");
    }
    public virtual void ResetVaribles(CharacterCTRL characterCTRL)
    {
        CustomLogger.Log(this, $"ResetVaribles()");
    }
}

public class Example : CharacterObserverBase
{

}
public class ArisObserver : CharacterObserverBase//1
{
    int critTime;
    public override void OnCrit(CharacterCTRL character)
    {
        base.OnCrit(character);
    }
    public override void OnCastedSkill(CharacterCTRL character)
    {
        base.OnCastedSkill(character);
    }
}
public class AkoObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        List<CharacterCTRL> characters = character.GetAllies();

        CharacterCTRL highestAttackCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.Attack))
            .FirstOrDefault();
        if (highestAttackCharacter != null)
        {
            Effect critChance = EffectFactory.CreateAkoCritChanceBuff(20,2f);
            highestAttackCharacter.effectCTRL.AddEffect(critChance);
            Effect critRatio = EffectFactory.CreateAkoCritRatioBuff(20, 2f);
            highestAttackCharacter.effectCTRL.AddEffect(critRatio);
        }
    }
}
public class AyaneObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL highestAttackCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.Attack))
            .FirstOrDefault();
        if (highestAttackCharacter != null)
        {
            Effect resistance = EffectFactory.CreateAyaneResistanceBuff(20, 2f);
            highestAttackCharacter.effectCTRL.AddEffect(resistance);
        }
    }
}
public class FuukaObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(character, 2, false, false, character.CurrentHex);
        foreach (var item in targetHex.GetCharacterOnNeighborHex(2,true))
        {
            item.Heal(10, character);
        }
    }
}
public class SerinaObserver :CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL LowestHealthCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.currHealth))
            .Last();
        LowestHealthCharacter.Heal(10,character);
    }
}
public class ShizukoObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL LowestHealthCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.currHealth))
            .Last();
        LowestHealthCharacter.AddShield(10,5.0f,character);
    }
}
public class HinaObserver : CharacterObserverBase
{

}
public class HoshinoObserver : CharacterObserverBase
{
    private Transform shield;
    public override void CharacterStart(CharacterCTRL character)
    {
        base.CharacterStart(character);
       shield = character.transform.Find("Hoshino_Original_Shield_Weapon");
       shield.gameObject.SetActive(false);
    }
    public override void OnCastedSkill(CharacterCTRL character)
    {
        base.OnCastedSkill(character);
        shield.gameObject.SetActive(true);
    }
    public override void OnSkillFinished(CharacterCTRL character)
    {
        base.OnSkillFinished(character);
        shield.gameObject.SetActive(false);
    }
    public override void OnDying(CharacterCTRL character)
    {
        base.OnDying(character);
        shield.gameObject.SetActive(false);
    }
}
public class ShirokoObserver : CharacterObserverBase
{
    private ShirokoActiveSkill skillCTRL;
    public override void CharacterStart(CharacterCTRL character)
    {
        skillCTRL = character.GetComponent<ShirokoActiveSkill>();
        base.CharacterStart(character);
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        if (skillCTRL.droneCTRL != null)
        {
            skillCTRL.droneCTRL.AssistAttack(character.Target.GetComponent<CharacterCTRL>(), character);
        }

        CustomLogger.Log(this, $"Character : {character.characterStats.name} OnAttacking.");
    }
}
public class TsurugiObserver : CharacterObserverBase
{
    private TsurugiActiveSkill skill;
    public override void CharacterStart(CharacterCTRL character)
    {
        skill = character.GetComponent<TsurugiActiveSkill>();
        skill.Parent = character;
        base.CharacterStart(character);
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        CustomLogger.Log(this,$"Attacking");
        if (skill.Enhancing)
        {
            skill.SpecialAttackCount--;
            if (skill.SpecialAttackCount <= 0)
            {
                skill.ResetAttackType();
            }
        }

    }
}
public class Shiroko_Terror_Observer : CharacterObserverBase
{
    private float healthThreshold70 = 0.7f; // 70%
    private float healthThreshold30 = 0.3f; // 30%
    private bool hasTriggered70 = false;
    private bool hasTriggered30 = false;
    private Shiroko_Terror_SkillCTRL skillCTRL;
    public Shiroko_Terror_Observer()
    {

    }
    public override void CharacterStart(CharacterCTRL character)
    {
        skillCTRL = character.GetComponent<Shiroko_Terror_SkillCTRL>();
        base.CharacterStart(character);
    }
    public override void ResetVaribles(CharacterCTRL characterCTRL)
    {
        base.ResetVaribles(characterCTRL);
        Shiroko_Terror_SkillCTRL skillController = characterCTRL.gameObject.GetComponent<Shiroko_Terror_SkillCTRL>();
        skillController.droneRef.SetActive(false);
        skillController.droneCTRL.Dmg = 0;
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        if (skillCTRL.droneCTRL != null)
        {
            skillCTRL.droneCTRL.AssistAttack(character.Target.GetComponent<CharacterCTRL>(),character);
        }

        CustomLogger.Log(this, $"[Shiroko_Terror_Observer] Character : {character.characterStats.name} OnAttacking.");
    }
    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        float maxHealth = character.GetStat(StatsType.Health);
        float currHealth = character.GetStat(StatsType.currHealth);
        float newHealth = currHealth - amount;
        float newPercentage = newHealth / maxHealth;
        if (!hasTriggered70 && newPercentage <= healthThreshold70)
        {
            Effect effect = EffectFactory.CreateInvincibleEffect(0);
            character.effectCTRL.AddEffect(effect);
            hasTriggered70 = true;
            newHealth = maxHealth * healthThreshold70;
            int adjustedAmount = (int)(currHealth - newHealth);
            CustomLogger.Log(this, $" {character.characterStats.name} 血量降至 70%，觸發效果。");
            skillCTRL.forcepick ++;
            amount = adjustedAmount;
        }
        if (!hasTriggered30 && newPercentage <= healthThreshold30)
        {
            Effect effect = EffectFactory.CreateInvincibleEffect(0);
            character.effectCTRL.AddEffect(effect);
            hasTriggered30 = true;
            newHealth = maxHealth * healthThreshold30;
            int adjustedAmount = (int)(currHealth - newHealth);
            CustomLogger.Log(this, $"[Shiroko_Terror_Observer] {character.characterStats.name} 血量降至 30%，觸發效果。");
            skillCTRL.forcepick ++;
            amount = adjustedAmount;
        }
        if (newPercentage > healthThreshold70)
        {
            hasTriggered70 = false; // 如果血量回到門檻以上，重置觸發狀態
        }
        if (newPercentage > healthThreshold30)
        {
            hasTriggered30 = false; // 如果血量回到門檻以上，重置觸發狀態
        }

        return amount;
    }
}
