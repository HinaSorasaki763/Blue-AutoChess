
using GameEnum;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class CharacterObserverBase
{
    public bool activated;
    public bool Augument125_Triggered = false;
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
        Augument125_Triggered = false;
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
        if (source.effectCTRL.HaveEffect("SakurakoBuff"))
        {
            CharacterParent characterParent = source.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            int amount = characterParent.SakurakoSkillDmg;
            if (GameController.Instance.CheckCharacterEnhance(49, source.IsAlly))
            {
                target.GetHitByTrueDamage(amount, source, "SakurakoSkillEffect", false);
                CustomLogger.Log(this, $"dealt {amount} as SakurakoSkillEffect's truedmg");
            }
            else
            {
                target.GetHit(amount, source, "SakurakoSkillEffect", false, false);
                CustomLogger.Log(this, $"dealt {amount} as SakurakoSkillEffect normal dmg");
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
            character.AddExtraStat(StatsType.CritChance, 1, "AkoCritChance", true);
            character.AddExtraStat(StatsType.CritRatio, 1, "AkoCritRatio", true);
            character.AkoAddedCrit++;
        }

        CustomLogger.Log(this, $"{character.characterStats.name} killed an enemy {characterDies} with source {detailedSource}.");
    }

    public virtual void OnDying(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"{character.characterStats.name} is dying.");
    }
    public virtual bool BeforeDying(CharacterCTRL parent)
    {
        CustomLogger.Log(this, $"Check {parent} undying");
        if (!Augument125_Triggered && parent.isAugment125Reinforced)
        {
            Augument125_Triggered = true;
            parent.Heal((int)parent.GetStat(StatsType.Health), parent);

            Effect effect = EffectFactory.CreateInvincibleEffect(1, parent);
            Effect stunEffect = EffectFactory.CreateStunEffect(1, parent);
            parent.effectCTRL.AddEffect(effect, parent);
            parent.effectCTRL.AddEffect(stunEffect, parent);
            return true;
        }
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
    public virtual void OnBattleStart()
    {

    }
    public virtual void OnBattleEnd(bool isVictory, CharacterCTRL parent)
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
    public HexNode FindMaxOccupiedEntityGrid(
    int range,
    List<HexNode> hexNodes,
    SkillContext skillContext,
    bool FindingAlly,
    bool isInjured = false)
    {
        float maxWeight = 0;  // 用來記錄最大權重
        HexNode maxNode = null;
        // 用於暫存目前擁有最高權重的角色清單（含權重）
        List<(CharacterCTRL character, float weight)> bestTuples = new List<(CharacterCTRL, float)>();

        foreach (var startNode in hexNodes)
        {
            // 使用新的方法取得 (角色, 權重) 清單
            var characterTuples = Utility.GetCharacterInrangeWithWeight(
                startNode,
                range,
                skillContext.Parent,
                FindingAlly,
                isInjured
            );

            // 計算該點位所有角色權重總和
            float totalWeight = characterTuples.Sum(t => t.weight);

            // 用權重總和來判斷是否更新最大值
            if (totalWeight >= maxWeight)
            {
                maxWeight = totalWeight;
                maxNode = startNode;
                bestTuples.Clear();
                bestTuples.AddRange(characterTuples);
            }
        }

        // 紀錄一下結果
        StringBuilder sb = new StringBuilder();
        foreach (var tupleItem in bestTuples)
        {
            sb.AppendLine($"{tupleItem.character.name}: {tupleItem.character.CurrentHex.name}, weight = {tupleItem.weight}");
        }
        sb.AppendLine($"max weight = {maxWeight}, pos = {maxNode.Position}, {maxNode.name}");
        CustomLogger.Log(this, sb.ToString());

        return maxNode;
    }
}

public class Example : CharacterObserverBase
{

}
public class AtsukoObserver : CharacterObserverBase
{
    public override int OnDamageTaken(CharacterCTRL character, CharacterCTRL source, int amount)
    {
        float healthThreshold = 0.25f;
        float maxHealth = character.GetStat(StatsType.Health);
        float currHealth = character.GetStat(StatsType.currHealth);
        float newHealth = currHealth - amount;
        float newPercentage = newHealth / maxHealth;
        StarLevelStats stats = character.ActiveSkill.GetCharacterLevel()[character.star];
        if (newPercentage <= healthThreshold)
        {
            Effect effect = EffectFactory.UnStatckableStatsEffct(0, "AtsukoPassive", stats.Data3, StatsType.DodgeChance, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.DodgeChance, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.DodgeChance, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
            return amount;
        }
        else
        {
            character.effectCTRL.ClearEffectWithSource("AtsukoPassive");
        }
        return amount;
    }

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
            Effect effect = EffectFactory.UnStatckableStatsEffct(20, "AyaneResistanceBuff", 20f, StatsType.Resistence, character, false, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            highestAttackCharacter.effectCTRL.AddEffect(effect, highestAttackCharacter);
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
        SkillContext skillContext = character.GetSkillContext();
        HexNode targetHex = FindMaxOccupiedEntityGrid(2, skillContext.hexMap, skillContext, true, true);
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
            if (Utility.GetRand(character) <= 7)
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
public class HimariObserver : CharacterObserverBase
{
    public override void OnAttacking(CharacterCTRL character)
    {
        CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        CharacterCTRL ally = Utility.GetSpecificCharacters(characterParent.GetBattleFieldCharacter(), StatsType.Attack, false, 1, true)[0];
        ally.Addmana(5);
        base.OnAttacking(character);
    }
}
public class HiyoriObserver : CharacterObserverBase
{
    public override void OnLogistic(CharacterCTRL character)
    {
        CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
        CharacterCTRL target = Utility.GetSpecificCharacters(characterParent.GetBattleFieldCharacter(), StatsType.currHealth, false, 1, true)[0];
        if (target != null)
        {
            Effect effect = EffectFactory.StatckableStatsEffct(5f, "HiyoriLogistic", -5, StatsType.Resistence, character, false);
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
    public override int BeforeHealing(CharacterCTRL characterCTRL, int amount)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(100, characterCTRL.IsAlly))
        {
            amount = (int)(amount * 1.2f);
        }
        return base.BeforeHealing(characterCTRL, amount);
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
                FearManager.Instance.ApplyFear(source, list, source.ActiveSkill.GetCharacterLevel()[source.star].Data5 + PressureManager.Instance.GetPressure(source.IsAlly) * 0.01f, damage);
            }
        }
    }
}
public class KasumiObserver : CharacterObserverBase
{

}
public class KarinObserver : CharacterObserverBase
{
    public override int BeforeDealtDmg(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        float hp = target.GetHealthPercentage();
        float dmgbonus =
            hp > 0.7f ? 0f :
            hp > 0.5f ? Mathf.Lerp(0f, 0.1f, Mathf.InverseLerp(0.7f, 0.5f, hp)) :
            hp > 0.2f ? Mathf.Lerp(0.1f, 0.5f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.5f, 0.2f, hp))) :
            0.5f;

        return base.BeforeDealtDmg(source, target, (int)(damage * dmgbonus), detailedSource, iscrit);
    }
    public override void OnLogistic(CharacterCTRL character)
    {
        character.Addmana(10);
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        CharacterCTRL lowestHpenemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(character.GetAttack());
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, lowestHpenemy.gameObject, true, iscrit);
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
        List<KeyValuePair<GameObject, HexNode>> fragments =
            new List<KeyValuePair<GameObject, HexNode>>(misakiObserver.FragmentNodes);

        int baseRand = Utility.GetRand(character); // 單次基準亂數
        for (int i = 0; i < fragments.Count; i++)
        {
            int randKey = baseRand + i * 37; // 確保不同碎片有可重播偏移
            Explode(fragments[i].Key, fragments[i].Value, character, randKey);
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
                Effect effect = EffectFactory.StatckableStatsEffct(
                    5, "MisakiEnhancedSkill", -20, StatsType.Resistence, character, false);

                effect.SetActions(
                    (c) => c.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                    (c) => c.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source));

                item.effectCTRL.AddEffect(effect, item);
            }

            item.GetHit(dmg1, character, DamageSourceType.Skill.ToString(), iscrit);
        }

        // 改為依個別碎片判定是否保留
        bool active = true;
        if (GameController.Instance.CheckCharacterEnhance(34, character.IsAlly))
        {
            System.Random rng = new System.Random(randKey);
            active = rng.Next(0, 100) >= 70; // 每顆獨立判定
        }

        fragment.SetActive(active);

        if (!active)
        {
            misakiObserver.FragmentNodes.Remove(fragment);
            misakiObserver.Fragments.Remove(fragment);
        }
    }

    public override void OnDying(CharacterCTRL character)
    {
        MisakiObserver misakiObserver = character.characterObserver as MisakiObserver;
        foreach (var item in misakiObserver.Fragments)
        {
            GameObject.Destroy(item.gameObject);
        }
        misakiObserver.FragmentNodes.Clear();
        misakiObserver.Fragments.Clear();
    }
    public override void OnBattleEnd(bool isVictory, CharacterCTRL c)
    {

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
            int dmg = (int)(character.GetAttack() * 0.3f);
            target.CurrentHex.ApplyBurningEffect(3, dmg, 0.5f, character);//TODO: 檢查是否可以和ex疊加，數值是否正確
        }
        else
        {
            CharacterCTRL target = character.GetTargetCTRL();
            Effect effect = EffectFactory.StatckableStatsEffct(0, "Moe", -5, StatsType.Resistence, character, true);
            effect.SetActions(
                (target) => target.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (target) => target.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            target.effectCTRL.AddEffect(effect, target);
        }
    }
}
public class ShirokoObserver : CharacterObserverBase
{
    private ShirokoActiveSkill skillCTRL;
    private CharacterCTRL ctrl;
    public override void CharacterStart(CharacterCTRL character)
    {
        ctrl = character;
        skillCTRL = character.GetComponent<ShirokoActiveSkill>();
    }
    public override void CharacterUpdate()
    {

        if (GameController.Instance.CheckCharacterEnhance(31, true) && !ctrl.CurrentHex.IsBattlefield)
        {
            Shiroko_Terror_AugmentCheck shiroko_Terror_AugmentCheck = ctrl.GetComponent<Shiroko_Terror_AugmentCheck>();
            shiroko_Terror_AugmentCheck.parent = ctrl;
            shiroko_Terror_AugmentCheck.TriggerDetecting();
        }
        else
        {
            Shiroko_Terror_AugmentCheck shiroko_Terror_AugmentCheck = ctrl.GetComponent<Shiroko_Terror_AugmentCheck>();
            shiroko_Terror_AugmentCheck.parent = ctrl;
            shiroko_Terror_AugmentCheck.StopDetecting();
        }
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        ctrl = character;
        if (skillCTRL.droneCTRL != null)
        {
            skillCTRL.droneCTRL.AssistAttack(character.Target.GetComponent<CharacterCTRL>(), character);
        }
        CustomLogger.Log(this, $"Character : {character.characterStats.name} OnAttacking.");
    }
}
public class TokiObserver : CharacterObserverBase
{
    public override void OncharacterEnabled(CharacterCTRL character)
    {
        int amount = 100;
        character.AddExtraStat(StatsType.DodgeChance, amount, "TokiPassive", false);
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
            StarLevelStats stats = character.ActiveSkill.GetCharacterLevel()[character.star];
            getHitCount++;
            if (getHitCount >= stats.Data2)
            {
                getHitCount -= stats.Data2;
                triggerCount++;
            }
            if (character.GetHealthPercentage() <= 0.6f && triggerCount > 0)
            {
                triggerCount--;
                int percent = stats.Data1;
                character.Heal((int)(character.GetStat(StatsType.Health) * percent * 0.01f), character);
            }
        }

    }
    public override void OnBattleEnd(bool isVictory, CharacterCTRL c)
    {
        triggerCount = 1;
    }
}
public class TsurugiObserver : CharacterObserverBase
{
    private bool trigger;
    private TsurugiActiveSkill skill;
    private CharacterCTRL parent;

    public override void OnBattleEnd(bool isVictory, CharacterCTRL c)
    {
        trigger = true;
    }
    public override void OncharacterEnabled(CharacterCTRL character)
    {
        character.AddExtraStat(StatsType.Lifesteal, 150, "TsurugiLifeSteal", false);
    }
    public override bool BeforeDying(CharacterCTRL parent)
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
        if (character.Target == null) return;
        character.transform.LookAt(character.Target.transform);
        HexNode origin = character.CurrentHex;
        HexNode target = character.Target.GetComponent<CharacterCTRL>().CurrentHex;
        if (skill.Enhancing)
        {
            skill.SpecialAttackCount--;
            if (skill.SpecialAttackCount <= 0)
            {
                skill.ResetAttackType();
            }
        }
        HexNode nearest = null;
        float minDist = float.MaxValue;
        foreach (var n in character.CurrentHex.Neighbors)
        {
            float d = Vector3.Distance(target.Position, n.Position);
            if (d < minDist) { minDist = d; nearest = n; }
        }
        if (nearest == null) return;
        Vector3 offset = character.CurrentHex.Position - nearest.Position;
        HexNode mirror = SpawnGrid.Instance.GetHexNodeByPosition(nearest.Position - offset);
        var observer = character.traitController.GetObserverForTrait(Traits.Barrage) as BarrageObserver;
        List<HexNode> targetHex = GetHexSet(nearest, mirror, observer.CastTimes + 1);
        targetHex.Add(nearest);
        targetHex.ForEach(n => n.CreateFloatingPiece(Color.yellow, 1f));
        foreach (var item in Utility.GetCharacterInSet(targetHex, character, false))
        {
            (bool iscrit, int dmg1) = character.CalculateCrit((int)character.GetStat(StatsType.Attack));
            item.GetHit(dmg1, character, DamageSourceType.NormalAttack.ToString(), iscrit);
        }

    }
    public List<HexNode> GetHexSet(HexNode center, HexNode target, int range)
    {
        Vector3Int[] dirs =
        {
        new Vector3Int(+1, -1, 0),
        new Vector3Int(+1, 0, -1),
        new Vector3Int(0, +1, -1),
        new Vector3Int(-1, +1, 0),
        new Vector3Int(-1, 0, +1),
        new Vector3Int(0, -1, +1)
    };

        // 找出最接近 target 的方向
        int dx = target.X - center.X, dy = target.Y - center.Y, dz = target.Z - center.Z;
        int bestDir = 0, minDist = int.MaxValue;
        for (int i = 0; i < 6; i++)
        {
            int diff = Mathf.Abs(dx - dirs[i].x) + Mathf.Abs(dy - dirs[i].y) + Mathf.Abs(dz - dirs[i].z);
            if (diff < minDist) { minDist = diff; bestDir = i; }
        }

        int[] dirIndex = { bestDir, (bestDir + 1) % 6, (bestDir + 5) % 6 };

        HashSet<HexNode> result = new HashSet<HexNode>();
        Queue<HexNode> frontier = new Queue<HexNode>();
        frontier.Enqueue(center);

        for (int step = 0; step < range; step++)
        {
            int count = frontier.Count;
            for (int i = 0; i < count; i++)
            {
                HexNode node = frontier.Dequeue();
                foreach (int d in dirIndex)
                {
                    Vector3Int offset = dirs[d];
                    string key = SpawnGrid.Instance.CubeCoordinatesToKey(node.X + offset.x, node.Y + offset.y, node.Z + offset.z);
                    if (SpawnGrid.Instance.hexNodes.TryGetValue(key, out HexNode next) && result.Add(next))
                        frontier.Enqueue(next);
                }
            }
        }
        return result.ToList();
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
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (target.GetHealthPercentage() <= 0.15f && GameController.Instance.CheckCharacterEnhance(39, source.IsAlly))
        {
            target.Executed(source, "SaoriPassive");
        }
    }
}
public class SumireObserver : CharacterObserverBase
{
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        if (GameController.Instance.CheckSpecificCharacterEnhanced(character, 11, character.IsAlly))
        {
            character.AddPercentageBonus(StatsType.Health, StatsType.Attack, 5, "SumireActiveSkill");
        }
        CustomLogger.Log(this, $"{character} OnEnterBattleField.");
    }
    public override void OnLeaveBattleField(CharacterCTRL c)
    {
        if (GameController.Instance.CheckSpecificCharacterEnhanced(c, 11, c.IsAlly))
        {
            c.RemovePercentageBonus("SumireActiveSkill");
        }
        CustomLogger.Log(this, $"{c} OnLeaveBattleField.");
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
        if (skillCTRL.droneCTRL != null && character.Target != null)
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
    public bool Augment1023Triggered;
    public override void CharacterUpdate()
    {

    }

    public override void CharacterStart(CharacterCTRL character)
    {
        Augment1023Triggered = false;
        if (character.CurrentHex.Augment1006HexSelected)
        {
            Effect effect = EffectFactory.UnStatckableStatsEffct(0, "Augment1006-1", 10, StatsType.PercentageResistence, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.PercentageResistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.PercentageResistence, -effect.Value, effect.Source)
            );
            Effect effect1 = EffectFactory.UnStatckableStatsEffct(0, "Augment1006-2", 10, StatsType.DamageIncrease, character, true);
            effect1.SetActions(
                (character) => character.ModifyStats(StatsType.DamageIncrease, effect1.Value, effect1.Source),
                (character) => character.ModifyStats(StatsType.DamageIncrease, -effect1.Value, effect1.Source)
            );
            Effect effect2 = EffectFactory.UnStatckableStatsEffct(0, "Augment1006-3", 10, StatsType.AttackSpeed, character, true);
            effect2.SetActions(
                (character) => character.ModifyStats(StatsType.AttackSpeed, effect2.Value, effect2.Source),
                (character) => character.ModifyStats(StatsType.AttackSpeed, -effect2.Value, effect2.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
            character.effectCTRL.AddEffect(effect1, character);
            character.effectCTRL.AddEffect(effect2, character);
        }
        else
        {
            character.effectCTRL.ClearEffectWithSource("Augment1006-1");
            character.effectCTRL.ClearEffectWithSource("Augment1006-2");
            character.effectCTRL.ClearEffectWithSource("Augment1006-3");
        }
        if (SelectedAugments.Instance.CheckAugmetExist(1037, character.IsAlly))
        {
            CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            int amount = characterParent.GetAlliesEquipmentCount();
            Effect effect = EffectFactory.UnStatckableStatsEffct(0, "Augment1037Health", amount * 15, StatsType.Health, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Health, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Health, -effect.Value, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
            Effect effect2 = EffectFactory.UnStatckableStatsEffct(0, "Augment1037AttackSpeed", amount * 2, StatsType.AttackSpeed, character, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.AttackSpeed, effect2.Value, effect2.Source),
                (character) => character.ModifyStats(StatsType.AttackSpeed, -effect2.Value, effect2.Source)
            );
            character.effectCTRL.AddEffect(effect2, character);
        }
        if (SelectedAugments.Instance.CheckAugmetExist(1020, character.IsAlly))
        {
            int amount = (int)character.GetStat(StatsType.Health);
            character.GetHitByTrueDamage(amount, null, "Augment1020", false);
            int percentage = 12 + ResourcePool.Instance.ally.Augment1020DamagePercentage;
            Effect effect = EffectFactory.Augment1020Effect();
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.DamageIncrease, percentage, effect.Source),
                (character) => character.ModifyStats(StatsType.DamageIncrease, percentage, effect.Source)
            );
            character.effectCTRL.AddEffect(effect, character);
        }
    }

    public override void OnAttacking(CharacterCTRL character)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(1036, character.IsAlly))
        {
            CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;

            if (characterParent.Augment1036AdditionalDmg <= 100)
            {
                characterParent.Augment1036AdditionalDmg++;
            }
            foreach (var item in Utility.GetAllBattlingCharacter(characterParent))
            {
                Effect effect = EffectFactory.Augment1036Effect();
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.DamageIncrease, characterParent.Augment1036AdditionalDmg / 5, effect.Source),
                    (character) => character.ModifyStats(StatsType.DamageIncrease, -characterParent.Augment1036AdditionalDmg / 5, effect.Source)
                );
                item.effectCTRL.AddEffect(effect, item);
            }
        }
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
        if (SelectedAugments.Instance.CheckAugmetExist(1023, character.IsAlly))
        {
            float maxHealth = character.GetStat(StatsType.Health);
            float currHealth = character.GetStat(StatsType.currHealth);
            float newHealth = currHealth - amount;
            float newPercentage = newHealth / maxHealth;
            if (!Augment1023Triggered && newPercentage <= 0.5f)
            {
                Augment1023Triggered = true;
                character.AddShield((int)(maxHealth * 0.3f), 20, character);
            }
        }

        return base.OnDamageTaken(character, source, amount);
    }

    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(1036, source.IsAlly) && detailedSource != "Augment1036")
        {
            CharacterParent characterParent = source.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            if (characterParent.Augment1036AdditionalDmg >= 100)
            {
                target.GetHitByTrueDamage(damage, source, "Augment1036", false);
            }
        }
        if (iscrit)
        {
            source.OnCrit();
        }
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
            PressureManager.Instance.AddPressure(1, character.IsAlly);
        }
        (int augmentId, int chance)[] configs =
        {
            (1003, 25),
            (1018, 35),
            (1035, 50)
        };
        foreach (var (augmentId, chance) in configs)
        {
            if (SelectedAugments.Instance.CheckAugmetExist(augmentId, character.IsAlly) &&
                Utility.GetRand(character) <= chance)
            {
                ResourcePool.Instance.GetRandRewardPrefab(characterDies.transform.position);
            }
        }
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
    }

    public override void OnCastedSkill(CharacterCTRL character)
    {
        int mana = (int)character.GetStat(StatsType.MaxMana);
        CharacterParent characterParent = character.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        characterParent.YukariManacount += mana;
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
    public override void OnBattleEnd(bool isVictory, CharacterCTRL c)
    {
        string result = isVictory ? "victory" : "defeat";
        CustomLogger.Log(this, $"Battle ended with {result}.");
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        if (GameController.Instance.CheckSpecificCharacterEnhanced(character, 11, character.IsAlly))
        {
            character.AddPercentageBonus(StatsType.Health, StatsType.Attack, 5, "SumireActiveSkill");
        }
        if (GameController.Instance.CheckSpecificCharacterEnhanced(character, 26, character.IsAlly))
        {
            character.AddPercentageBonus(StatsType.Null, StatsType.Resistence, 25, "HoshinoPAssive");
        }
        CustomLogger.Log(this, $"{character} OnEnterBattleField.");
    }
    public override void OnLeaveBattleField(CharacterCTRL c)
    {
        if (GameController.Instance.CheckSpecificCharacterEnhanced(c, 11, c.IsAlly))
        {
            c.RemovePercentageBonus("SumireActiveSkill");
        }
        CustomLogger.Log(this, $"{c} OnLeaveBattleField.");
    }
    public override void OncharacterEnabled(CharacterCTRL character)
    {

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
        int be4 = damage;
        damage = (int)(damage * (1 + source.GetStat(StatsType.DamageIncrease) * 0.01f));
        CustomLogger.Log(this, $"before = {be4} ,after = {damage}");
        if (source.traitController.GetAcademy() == Traits.Gehenna)
        {
            damage = (int)((1 + PressureManager.Instance.GetPressure(source.IsAlly) * 0.05f) * damage);
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
