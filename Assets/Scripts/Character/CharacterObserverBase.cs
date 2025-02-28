
using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CharacterObserverBase
{
    public bool activated;
    public virtual void ActivateTrait()
    {
        activated = true;
    }
    public virtual void DeactivateTrait()
    {
        activated = false;
    }
    public virtual Dictionary<int, TraitLevelStats>  GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> traitLevelStats = new Dictionary<int, TraitLevelStats>()
        {
            {1, new TraitLevelStats(0,0,0)},
            {2, new TraitLevelStats(0,0,0)},
            {3, new TraitLevelStats(0,0,0)}
        };
        return traitLevelStats;
    }
    public virtual void CharacterUpdate()
    {

    }
    public virtual void ManualUpdate(CharacterCTRL character) 
    {
    
    }
    
    public virtual void CharacterStart(CharacterCTRL character)
    {

    }
    public virtual void OnAttacking(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"Character : {character.characterStats.name} OnAttacking.");
    }
    public virtual void OnTraitLevelChanged(int level, CharacterCTRL character)
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
    public virtual void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {

    }

    public virtual void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} get hit by {source.characterStats.name} for {amount}");
    }
    public virtual void OnKilledEnemy(CharacterCTRL character, string detailedSource ,CharacterCTRL characterDies)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} killed an enemy {characterDies} with source {detailedSource}.");
    }

    public virtual void OnDying(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is dying.");
    }

    public virtual void OnCastedSkill(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.CharacterName} casted a skill.");
    }
    public virtual void OnSkillFinished(CharacterCTRL character)
    {
        character.ManaLock = false;
        character.ReleaseLockDirection();
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
        CustomLogger.Log(this, $"Battle ended with {result}.");
    }
    public virtual void OnEnterBattleField(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character} OnEnterBattleField.");
    }
    public virtual void OnLeaveBattleField(CharacterCTRL c)
    {
        CustomLogger.Log(this, $"{c} OnLeaveBattleField.");
    }
    public virtual void ResetVaribles(CharacterCTRL characterCTRL)
    {
        CustomLogger.Log(this, $"ResetVaribles()");
    }
    public virtual (float, float) AdjustNegetiveEffect(float length, float ratio)
    {
        return (length, ratio);
    }
    public virtual int BeforeHealing(CharacterCTRL characterCTRL,int amount)
    {
        return amount;
    }
    /// </summary>
    /// 這裡回傳的數值為最終傷害
    /// </summary>
    public virtual int DamageModifier(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        return damage;
    }
    /// </summary>
    /// 這裡回傳的是"減去的數值"，並且在外部進行加總，一次扣除所有減去的數值，以方便計算。
    /// </summary>
    public virtual int BeforeDealtDmg(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        return 0;
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
            Effect critChance = EffectFactory.CreateAkoCritChanceBuff(20, 2f, character);
            highestAttackCharacter.effectCTRL.AddEffect(critChance);
            Effect critRatio = EffectFactory.CreateAkoCritRatioBuff(20, 2f, character);
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
            Effect resistance = EffectFactory.CreateAyaneResistanceBuff(20, 2f, character);
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
        foreach (var item in targetHex.GetCharacterOnNeighborHex(2, true))
        {
            item.Heal(10, character);
        }
    }
}
public class MikaObserver : CharacterObserverBase
{
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        if (detailedSource == DamageSourceType.Skill.ToString())
        {
            int level = character.star;
            StarLevelStats stats = character.ActiveSkill.GetCharacterLevel()[level];
            int BaseDamage = stats.Data1;
            int DamageRatio = stats.Data2;
            int dmg = BaseDamage + DamageRatio * (int)character.GetStat(StatsType.Attack);
            (bool iscrit, int dmg1) = character.CalculateCrit(dmg);
            foreach (var item in characterDies.CurrentHex.Neighbors)
            {
                if (item.OccupyingCharacter!= null)
                {
                    item.OccupyingCharacter.GetHit(dmg1,character, DamageSourceType.Skill.ToString(),iscrit);
                }
            }
        }
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }
}
public class SerinaObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        base.OnLogistic(character);
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL LowestHealthCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.currHealth))
            .Last();
        LowestHealthCharacter.Heal(10, character);
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
        LowestHealthCharacter.AddShield(100, 5.0f, character);
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
public class IzunaObserver :CharacterObserverBase
{
    public int count = 0;
    public override void OnAttacking(CharacterCTRL character)
    {
        character.UpdateSkillContext();
        CustomLogger.Log(this,$"IzunaObserver{count}");
        count++;
        if (count >= 5)
        {
            count -= 5;
            character.customAnimator.ChangeState(CharacterState.CastSkill);
            character.AudioManager.PlayCastExSkillSound();

        }
        base.OnAttacking(character);
    }
}
public class MisakiObserver : CharacterObserverBase
{
    public override void OnAttacking(CharacterCTRL character)
    {
        GameObject target = character.Target;
        GameObject bullet = GameObject.Instantiate(ResourcePool.Instance.MisslePrefab);

        List<HitEffect> hitEffect = new List<HitEffect> { new MisakiSkillEffect() };
        (bool iscrit, int dmg1) = character.CalculateCrit(character.GetAttack());

        bullet.GetComponent<NormalBullet>().Initialize(
            dmg1,
            character.GetTargetLayer(),
            character,
            15f,
            target,
            true,
            iscrit,
            hitEffect,
            5
        );

        CustomLogger.Log(this, "OnAttacking - Fired Misaki Cannon");
    }

    public override void OnSkillFinished(CharacterCTRL character)
    {
        Detonate(character);
        base.OnSkillFinished(character);
    }

    void Detonate(CharacterCTRL character)
    {
        Misaki_Skill misaki_Skill = character.ActiveSkill as Misaki_Skill;
        if (misaki_Skill == null)
        {
            CustomLogger.LogWarning(this, "No Misaki_Skill found on Detonate");
            return;
        }
        foreach (var item in misaki_Skill.FragmentNodes)
        {
            Explode(item.Key, item.Value,character);
        }
        misaki_Skill.Fragments.Clear();
        misaki_Skill.FragmentNodes.Clear();
        CustomLogger.Log(this, "All fragments detonated and cleared");
    }

    void Explode(GameObject fragment, HexNode node, CharacterCTRL character)
    {
        CustomLogger.Log(this, "Fragment exploded");
        foreach (var item in Utility.GetCharacterInrange(node,1,character,false))
        {
            int dmg = character.ActiveSkill.GetAttackCoefficient(character.GetSkillContext());
            (bool iscrit, int dmg1) = character.CalculateCrit(dmg);
            item.GetHit(dmg1, character, DamageSourceType.Skill.ToString(), iscrit);
        }
        GameObject.Destroy(fragment);
    }
}
public class MiyuObserver : CharacterObserverBase
{
    public override void CharacterStart(CharacterCTRL character)
    {
        CustomLogger.Log(this, "MiyuObserver CharacterStart");
        SetFlag(character);
        base.CharacterStart(character);
    }
    public override void ManualUpdate(CharacterCTRL character)
    {
        SetFlag(character);
        CustomLogger.Log(this, "MiyuObserver ManualUpdate");
    }
    private void SetFlag(CharacterCTRL character)
    {
        var characterPool = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        if (characterPool.CheckAlliesOnBoard() > 1)
        {
            character.isTargetable = false;
        }
        else
        {
            character.isTargetable = true;
        }
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
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        character.Heal((int)character.GetStat(StatsType.Health),character);
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }
    public override void CharacterStart(CharacterCTRL character)
    {
        skill = character.GetComponent<TsurugiActiveSkill>();
        skill.Parent = character;
        base.CharacterStart(character);
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        List<HexNode> affectedHex = character.CurrentHex.Neighbors.Intersect(character.Target.GetComponent<CharacterCTRL>().CurrentHex.Neighbors).ToList();
        base.OnAttacking(character);
        foreach (var item in Utility.GetCharacterInSet(affectedHex,character,false))
        {
            (bool iscrit, int dmg1) = character.CalculateCrit((int)character.GetStat(StatsType.Attack));
            item.GetHit(dmg1, character,DamageSourceType.NormalAttack.ToString(), iscrit);
        }
        CustomLogger.Log(this, $"Attacking");
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
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        if (skillCTRL.droneCTRL != null)
        {
            skillCTRL.droneCTRL.AssistAttack(character.Target.GetComponent<CharacterCTRL>(), character);
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
            Effect effect = EffectFactory.CreateInvincibleEffect(0, character);
            character.effectCTRL.AddEffect(effect);
            hasTriggered70 = true;
            newHealth = maxHealth * healthThreshold70;
            int adjustedAmount = (int)(currHealth - newHealth);
            CustomLogger.Log(this, $" {character.characterStats.name} 血量降至 70%，觸發效果。");
            skillCTRL.forcepick++;
            amount = adjustedAmount;
        }
        if (!hasTriggered30 && newPercentage <= healthThreshold30)
        {
            Effect effect = EffectFactory.CreateInvincibleEffect(0, character);
            character.effectCTRL.AddEffect(effect);
            hasTriggered30 = true;
            newHealth = maxHealth * healthThreshold30;
            int adjustedAmount = (int)(currHealth - newHealth);
            CustomLogger.Log(this, $"[Shiroko_Terror_Observer] {character.characterStats.name} 血量降至 30%，觸發效果。");
            skillCTRL.forcepick++;
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
public class GlobalBaseObserver : CharacterObserverBase
{
    public override void CharacterUpdate()
    {
        base.CharacterUpdate();
    }

    public override void CharacterStart(CharacterCTRL character)
    {
        base.CharacterStart(character);
    }

    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
    }

    public override void OnTraitLevelChanged(int level, CharacterCTRL character)
    {
        base.OnTraitLevelChanged(level, character);
    }

    public override void OnManaFull(CharacterCTRL character)
    {
        base.OnManaFull(character);
    }

    public override void OnCrit(CharacterCTRL character)
    {
        base.OnCrit(character);
    }

    public override void OnHealthChanged(CharacterCTRL character)
    {
        base.OnHealthChanged(character);
    }

    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        return base.OnDamageTaken(character, source, amount);
    }

    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }

    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource)
    {
        base.GetHit(character, source, amount, isCrit, detailedSource);
    }

    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }

    public override void OnDying(CharacterCTRL character)
    {
        base.OnDying(character);
    }

    public override void OnCastedSkill(CharacterCTRL character)
    {
        base.OnCastedSkill(character);
    }
    public override void OnSkillFinished(CharacterCTRL character)
    {
        character.ManaLock = false;
        character.ReleaseLockDirection();
        CustomLogger.Log(this, $"{character.characterStats.name} skill finished.");
    }

    public override void OnReceivingAssistance(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is receiving assistance.");
    }

    public override void OnGettingCrowdControlled(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is being crowd controlled.");
    }

    public override void OnDamaging(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} dealt damage.");
    }
    public override void OnLogistic(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} Logistic.");
    }
    public override void OnBattleEnd(bool isVictory)
    {
        string result = isVictory ? "victory" : "defeat";
        CustomLogger.Log(this, $"Battle ended with {result}.");
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character} OnEnterBattleField.");
    }
    public override void OnLeaveBattleField(CharacterCTRL c)
    {
        CustomLogger.Log(this, $"{c} OnLeaveBattleField.");
    }
    public override void ResetVaribles(CharacterCTRL characterCTRL)
    {
        CustomLogger.Log(this, $"ResetVaribles()");
    }
}
