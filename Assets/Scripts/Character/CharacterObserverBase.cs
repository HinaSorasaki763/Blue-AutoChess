
using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
    public virtual Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
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
    public virtual void OnCharacterDisabled(CharacterCTRL character)
    {

    }
    public virtual void OncharacterEnabled(CharacterCTRL character)
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
        if (GameController.Instance.CheckCharacterEnhance(39, source.IsAlly))
        {
            int threshold = (int)(target.GetStat(StatsType.Health) * 15 * 0.01f);
            if (target.GetStat(StatsType.currHealth) <= threshold)
            {
                target.Executed(source, detailedSource);
            }
        }
    }

    public virtual void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} get hit by {source.characterStats.name} for {amount} as {detailedSource}");

    }
    public virtual void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        if (character.effectCTRL.GetEffect("AkoEnhancedSkillBuff") != null)
        {
            character.AddExtraStat(StatsType.CritChance, 1,"AkoCritChance",true);
            character.AddExtraStat(StatsType.CritRatio, 1, "AkoCritRatio", true);
            character.AkoAddedCrit++;
        }

        CustomLogger.Log(this, $"{character.characterStats.name} killed an enemy {characterDies} with source {detailedSource}.");
    }

    public virtual void OnDying(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is dying.");
    }
    public virtual bool BeforeDying()
    {
        return false;
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
    public virtual void OnDodged(CharacterCTRL character)
    {
        if (GameController.Instance.CheckCharacterEnhance(32, character.IsAlly))
        {
            Effect effect = EffectFactory.StatckableStatsEffct(5, "Atsuko", 1, StatsType.DodgeChance, character, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.DodgeChance, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.DodgeChance, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
        }
        CustomLogger.Log(this, $"{character.characterStats.name} dodged.");
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
    public virtual void OnHealing(CharacterCTRL characterCTRL)
    {

    }
    public virtual int BeforeHealing(CharacterCTRL characterCTRL, int amount)
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
        List<CharacterCTRL> characters = character.GetAllies();

        CharacterCTRL highestAttackCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.Attack))
            .FirstOrDefault();
        if (highestAttackCharacter != null)
        {
            Effect critChance = EffectFactory.CreateAkoCritChanceBuff(20, 2f, character);
            highestAttackCharacter.effectCTRL.AddEffect(critChance, highestAttackCharacter);
            Effect critRatio = EffectFactory.CreateAkoCritRatioBuff(20, 2f, character);
            highestAttackCharacter.effectCTRL.AddEffect(critRatio, highestAttackCharacter);
        }
    }
}
public class AyaneObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL highestAttackCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.Attack))
            .FirstOrDefault();
        if (highestAttackCharacter != null)
        {
            Effect resistance = EffectFactory.CreateAyaneResistanceBuff(20, 2f, character);
            highestAttackCharacter.effectCTRL.AddEffect(resistance, highestAttackCharacter);
        }
    }
}
public class AzusaObserver : CharacterObserverBase
{
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        if (GameController.Instance.CheckCharacterEnhance(13, character.IsAlly))
        {
            GameController.Instance.AzusaAddAttack += 2;
            character.AddExtraStat(StatsType.Attack, 2, "AzusaAttack", true);
        }
    }
}
public class FuukaObserver : CharacterObserverBase
{
    public int count = 0;
    public override void OnLogistic(CharacterCTRL character)
    {
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(character, 2, false, false, character.CurrentHex);
        foreach (var item in targetHex.GetCharacterOnNeighborHex(2, true))
        {
            if (item.IsAlly == character.IsAlly)
            {
                item.Heal((int)(character.GetStat(StatsType.Attack) * 0.25f), character);
                CustomLogger.Log(this, $"Fuuka Heal {item.characterStats.name} for {(int)(character.GetStat(StatsType.Attack) * 0.25f)}");
            }

        }
    }
    public override void OnHealing(CharacterCTRL characterCTRL)
    {
        if (GameController.Instance.CheckCharacterEnhance(15, characterCTRL.IsAlly) && characterCTRL.characterStats.CharacterId == 15)
        {
            count++;
            if (count >= characterCTRL.ActiveSkill.GetCharacterLevel()[characterCTRL.star].Data4)
            {
                int rand = Utility.GetRand(characterCTRL);
                if (rand <= 10)
                {
                    ResourcePool.Instance.GetGoldPrefab(characterCTRL.transform.position);
                }
                else if (rand <= 75)
                {
                    ResourcePool.Instance.GetGoldPrefab(characterCTRL.transform.position);
                }
                else
                {
                    ResourcePool.Instance.GetRandRewardPrefab(characterCTRL.transform.position);
                }
                count -= characterCTRL.ActiveSkill.GetCharacterLevel()[characterCTRL.star].Data4;
            }
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
            foreach (var item in Utility.GetCharacterInrange(characterDies.CurrentHex, 1, character, false))
            {
                item.GetHit(dmg1, character, DamageSourceType.Skill.ToString(), iscrit);
            }
        }
    }
}
public class SerikaObserver : CharacterObserverBase
{
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        if (character.effectCTRL.GetEffect("SerikaAddGold") != null)
        {
            if (Utility.GetRand(character) <= 50)
            {
                ResourcePool.Instance.GetGoldPrefab(character.CurrentHex.Position);
            }
        }
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
        LowestHealthCharacter.Heal((int)character.GetStat(StatsType.Attack), character);
    }
}
public class ShizukoObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        List<CharacterCTRL> characters = character.GetAllies();
        CharacterCTRL LowestHealthCharacter = characters
            .OrderByDescending(item => item.GetStat(StatsType.currHealth))
            .Last();
        LowestHealthCharacter.AddShield(character.GetAttack(), 5.0f, character);
    }
    public override void OnCharacterDisabled(CharacterCTRL character)
    {
        if (character.gameObject.GetComponent<ShizukoActiveSkill>().Reference != null)
        {
            character.gameObject.GetComponent<ShizukoActiveSkill>().Reference.SetActive(false);
        }
    }
}
public class HinaObserver : CharacterObserverBase
{

}
public class HiyoriObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        CharacterCTRL target = Utility.GetSpecificCharacters(characterParent.GetBattleFieldCharacter(), StatsType.currHealth, false, 1, true)[0];
        if (target != null)
        {
            Effect effect = EffectFactory.StatckableStatsEffct(1.5f, "HiyoriEnhancedSkillEffect", -10, StatsType.Resistence, character, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            target.effectCTRL.AddEffect(effect, target);
        }
        else
        {
            CustomLogger.LogWarning(this, "No target found for Hiyori Enhanced Skill Effect.");
        }

    }
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
    public override void OncharacterEnabled(CharacterCTRL character)
    {
        int amount = character.ActiveSkill.GetCharacterLevel()[character.star].Data4;
        character.AddPercentageBonus(StatsType.Null, StatsType.Resistence, amount, "HoshinoPassive");
    }
}
public class IzunaObserver : CharacterObserverBase
{
    public int count = 0;
    public override void OnAttacking(CharacterCTRL character)
    {
        character.UpdateSkillContext();
        CustomLogger.Log(this, $"IzunaObserver{count}");
        bool isEnhanced = GameController.Instance.CheckCharacterEnhance(16, character.IsAlly);
        if (!isEnhanced)
        {
            count++;
            if (count >= 5)
            {
                character.customAnimator.ChangeState(CharacterState.CastSkill);
                character.AudioManager.PlayCastExSkillSound();
                count -= 5;
            }
        }
        else
        {
            count++;
            if (count >= 5)
            {
                Effect effect = EffectFactory.IzunaAttackSpeedBuff();
                character.effectCTRL.AddEffect(effect, character);
                HexNode h = Utility.FindFarthestNode(character, (int)character.GetStat(StatsType.Range));
                HexNode currHex = character.CurrentHex;
                character.transform.position = h.Position + new Vector3(0, 0.14f, 0);
                h.HardReserve(character);
                currHex.HardRelease();
                count -= 5;
            }
        }
    }
}
public class KayokoObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource == DamageSourceType.NormalAttack.ToString())
        {
            if (Utility.GetRand(source) < 30 + PressureManager.Instance.GetPressure(source.IsAlly) * 0.01f)
            {
                List<CharacterCTRL> list = new List<CharacterCTRL>();
                list.Add(target);
                FearManager.Instance.ApplyFear(source, list, source.ActiveSkill.GetCharacterLevel()[source.star].Data5 + PressureManager.Instance.GetPressure(source.IsAlly) * 0.01f);
            }
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
public class MisakiObserver : CharacterObserverBase
{
    public List<GameObject> Fragments = new List<GameObject>();
    public Dictionary<GameObject, HexNode> FragmentNodes = new Dictionary<GameObject, HexNode>();
    public override void OnAttacking(CharacterCTRL character)
    {
        GameObject target = character.Target;
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.Missle, character.transform.position, Quaternion.identity);

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
    }

    void Detonate(CharacterCTRL character)
    {
        MisakiObserver misakiObserver = character.characterObserver as MisakiObserver;

        // 建立快照清單，避免修改 Dictionary 時拋出例外
        List<KeyValuePair<GameObject, HexNode>> fragments = new List<KeyValuePair<GameObject, HexNode>>(misakiObserver.FragmentNodes);

        int count = 0;
        int randkey = 0;

        foreach (var item in fragments)
        {
            count++;
            randkey += Utility.GetRand(character, count);
            Explode(item.Key, item.Value, character, randkey);
        }
    }


    void Explode(GameObject fragment, HexNode node, CharacterCTRL character, int randKey)
    {
        MisakiObserver misakiObserver = character.characterObserver as MisakiObserver;
        foreach (var item in Utility.GetCharacterInrange(node, 1, character, false))
        {
            int dmg = character.ActiveSkill.GetAttackCoefficient(character.GetSkillContext());
            (bool iscrit, int dmg1) = character.CalculateCrit(dmg);
            if (GameController.Instance.CheckCharacterEnhance(34, character.IsAlly))
            {
                Effect effect = EffectFactory.StatckableStatsEffct(5, "MisakiEnhancedSkill", -20, StatsType.Resistence, character, false);
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                    (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
                );
                item.effectCTRL.AddEffect(effect, item);
            }
            item.GetHit(dmg1, character, DamageSourceType.Skill.ToString(), iscrit);
        }
        bool active = true;
        if (GameController.Instance.CheckCharacterEnhance(34, true))
        {

            if (Utility.GetRand(character, randKey) >= 30)
            {
                active = false;
            }
        }
        fragment.SetActive(active);
        if (!active)
        {
            misakiObserver.FragmentNodes.Remove(fragment);
            misakiObserver.Fragments.Remove(fragment);
        }
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
public class MoeObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        if (GameController.Instance.CheckCharacterEnhance(37, character.IsAlly))
        {
            CharacterCTRL target = character.Target.GetComponent<CharacterCTRL>();
            int dmg = character.GetAttack();
            target.CurrentHex.ApplyBurningEffect(3, dmg, 0.5f, character);//TODO: 檢查是否可以和ex疊加，數值是否正確
        }
        else
        {
            CharacterCTRL target = character.GetTargetCTRL();
            Effect effect = EffectFactory.StatckableStatsEffct(1.5f, "Moe", -5, StatsType.Resistence, character, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, target);
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
public class TsubakiObserver : CharacterObserverBase
{
    private int getHitCount;
    private int triggerCount;
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        if (GameController.Instance.CheckCharacterEnhance(23, character.IsAlly))
        {
            getHitCount++;
            if (getHitCount >= 100)
            {
                getHitCount -= 100;
                triggerCount++;
            }
            if (character.GetHealthPercentage() <= 0.6f && triggerCount > 0)
            {
                triggerCount--;
                character.Heal((int)(character.GetStat(StatsType.Health) * 0.4f), character);
            }
        }

    }
    public override void OnBattleEnd(bool isVictory)
    {
        triggerCount = 1;
    }
}
public class TsurugiObserver : CharacterObserverBase
{
    private bool trigger;
    private TsurugiActiveSkill skill;
    private CharacterCTRL parent;
    public int DamageIncrease = 0;

    public override void OnBattleEnd(bool isVictory)
    {
        trigger = true;
    }
    public override void OncharacterEnabled(CharacterCTRL character)
    {
        character.AddExtraStat(StatsType.Lifesteal, 150, "TsurugiLifeSteal", false);
    }
    public override bool BeforeDying()
    {
        if (trigger)
        {
            trigger = false;
            int amount = (int)(parent.GetStat(StatsType.Health) * 0.5f);
            parent.Heal(amount, parent);
            Effect effect = EffectFactory.CreateInvincibleEffect(1, parent);
            parent.effectCTRL.AddEffect(effect, parent);
            return true;
        }
        return false;
    }
    public override int DamageModifier(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        return (int)(damage * (1+(DamageIncrease * 0.01f)));
    }
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        character.Heal((int)character.GetStat(StatsType.Health), character);
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }
    public override void CharacterStart(CharacterCTRL character)
    {
        skill = character.GetComponent<TsurugiActiveSkill>();
        skill.Parent = character;
        parent = character;
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {

    }
    public override void OnAttacking(CharacterCTRL character)
    {
        character.transform.LookAt(character.Target.transform);
        HexNode origin = character.CurrentHex;
        HexNode targetHex = character.Target.GetComponent<CharacterCTRL>().CurrentHex;
        int range = 1;
        if (skill.Enhancing)
        {
            skill.SpecialAttackCount--;
            range++;
            if (skill.SpecialAttackCount <= 0)
            {
                skill.ResetAttackType();
            }
        }

        List<HexNode> candidates = Utility.GetHexInRange(origin, range);
        Vector3 forward = (origin.Position - targetHex.Position).normalized;
        List<HexNode> affectedHex = new List<HexNode>();

        foreach (var hex in candidates)
        {
            if (hex == targetHex) continue;

            Vector3 dir = (origin.Position - hex.Position).normalized;
            float angle = Vector3.Angle(forward, dir);

            if (Mathf.Abs(angle) <= 61)
            {
                affectedHex.Add(hex);
                CustomLogger.Log(this, $"Get {hex.name} on {hex.Position} with angle {angle} ,{dir} compare to {forward}");
            }
        }

        foreach (var item in Utility.GetCharacterInSet(affectedHex, character, false))
        {
            (bool iscrit, int dmg1) = character.CalculateCrit((int)character.GetStat(StatsType.Attack));
            item.GetHit(dmg1, character, DamageSourceType.NormalAttack.ToString(), iscrit);
        }
    }

}
public class NeruObserver : CharacterObserverBase
{
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (detailedSource == DamageSourceType.NormalAttack.ToString())
        {
            Effect effect = EffectFactory.StatckableStatsEffct(5, "NeruPassive", 1, StatsType.Attack, source, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Attack, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Attack, -effect.Value, effect.Source)
            );
            source.effectCTRL.AddEffect(effect, source);
        }
    }
}
public class YuukaObserver : CharacterObserverBase
{
    public override void OnDodged(CharacterCTRL character)
    {
        if (character.effectCTRL.GetEffect("YuukaSkill") != null)
        {
            character.Addmana(1);
        }
    }
}
public class SaoriObserver : CharacterObserverBase
{

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
            Effect effect = EffectFactory.CreateInvincibleEffect(1f, character);
            character.effectCTRL.AddEffect(effect, character);
            hasTriggered70 = true;
            newHealth = maxHealth * healthThreshold70;
            int adjustedAmount = (int)(currHealth - newHealth);
            CustomLogger.Log(this, $" {character.characterStats.name} 血量降至 70%，觸發效果。");
            skillCTRL.forcepick++;
            amount = adjustedAmount;
        }
        if (!hasTriggered30 && newPercentage <= healthThreshold30)
        {
            Effect effect = EffectFactory.CreateInvincibleEffect(1f, character);
            character.effectCTRL.AddEffect(effect, character);
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
public class WakamoObserver : CharacterObserverBase
{
    public override void CharacterStart(CharacterCTRL character)
    {
        character.SetStat(StatsType.Range, 10);
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        character.FindTarget();
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {

    }
}
public class NullObserver : CharacterObserverBase
{

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
        if (GameController.Instance.CheckCharacterEnhance(9, character.IsAlly))
        {
            GameController.Instance.AddSerinaEnhancedSkill_CritCountStack(character.IsAlly);
        }
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
        if (iscrit)
        {
            source.OnCrit();
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }

    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion)
    {
        if (!recursion) return;
        if (character.effectCTRL.GetEffect("MiyuEnhancedSkillEffect") != null)
        {
            CharacterParent characterParent = source.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            CharacterCTRL miyu = Utility.GetSpecificCharacterByIndex(characterParent.GetBattleFieldCharacter(), 36);
            if (miyu != null && detailedSource != "MiyuSkill")
            {
                float dmg = miyu.ActiveSkill.GetAttackCoefficient(miyu.GetSkillContext()) * 0.15f;
                character.GetHit((int)dmg, miyu, "MiyuSkill", false);
            }
        }
        if (character.effectCTRL.GetEffect("HinaMark") != null && source.traitController.GetAcademy() == Traits.Gehenna && detailedSource != "HinaMarkExplode")
        {
            int stack = PressureManager.Instance.GetPressure(source.IsAlly);
            int ratio = source.ActiveSkill.GetCharacterLevel()[source.star].Data4;
            character.GetHit((int)(stack * ratio * 0.01f + amount * ratio), source, "HinaMarkExplode", false);
        }
    }

    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        if (character.traitController.GetAcademy() == Traits.Gehenna)
        {
            PressureManager.Instance.IncreasePressure(1);
        }
        base.OnKilledEnemy(character, detailedSource, characterDies);
    }

    public override void OnDying(CharacterCTRL character)
    {

        if (character.effectCTRL.HaveEffect("WakamoEnhancedMark"))
        {
            CharacterCTRL c = Utility.GetNearestAlly(character);
            Effect effect = EffectFactory.WakamoEnhancedMark(c, 20);
            c.effectCTRL.AddEffect(effect, c);
            c.dmgRecivedOnWakamoMarked += character.dmgRecivedOnWakamoMarked;
        }
        if (character.effectCTRL.HaveEffect("KazusaMark"))
        {
            CustomLogger.Log(this, $"{character.characterStats.name} KazusaMark");
            CharacterParent c = character.IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
            CharacterCTRL ch = Utility.GetSpecificCharacterByIndex(c.GetBattleFieldCharacter(), 18);
            GameController.Instance.AddExtraStat(StatsType.AttackSpeed, ch.ActiveSkill.GetCharacterLevel()[ch.star].Data4 * 0.01f);
        }
        if (character.effectCTRL.HaveEffect("NoaEnhancedSkill"))
        {
            CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
            CharacterCTRL noa = Utility.GetSpecificCharacterByIndex(characterParent.GetBattleFieldCharacter(), 7);
            if (noa.ActiveSkill is NoaEnhancedSkill noaSkill)
            {
                noaSkill.AddMark(noa.GetSkillContext());
            }
        }
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
    public override void OncharacterEnabled(CharacterCTRL character)
    {
        if (GameController.Instance.CheckSpecificCharacterEnhanced(character,11,character.IsAlly))
        {
            character.AddPercentageBonus(StatsType.Health, StatsType.Attack, 5, "SumireActiveSkill");
        }
        base.OncharacterEnabled(character);
    }
    public override void OnCharacterDisabled(CharacterCTRL character)
    {
        EffectCanvas.Instance.RemoveWakamoImage(character);
    }
    public override void ResetVaribles(CharacterCTRL characterCTRL)
    {
        CustomLogger.Log(this, $"ResetVaribles()");
    }
    public override int DamageModifier(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (source.traitController.GetAcademy() == Traits.Gehenna)
        {
            damage = (int)((1 + PressureManager.Instance.GetPressure(source.IsAlly) * 0.15f) * damage);
        }
        if (detailedSource == "HiyoriSkill" && target.effectCTRL.GetEffect("HiyoriMark") != null)
        {
            HiyoriEnhancedSkill_EffectCounter h = source.GetComponent<HiyoriEnhancedSkill_EffectCounter>();
            h.SetLastTarget(target);
            return (int)(damage * h.ratio);
        }
        return base.DamageModifier(source, target, damage, detailedSource, iscrit);
    }

}
