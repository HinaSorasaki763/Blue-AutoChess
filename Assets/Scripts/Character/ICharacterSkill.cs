
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
public abstract class CharacterSkillBase
{
    public virtual Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,0,0)}, // 星級1的數據
            {2, new StarLevelStats(0,0,0)}, // 星級2的數據
            {3, new StarLevelStats(0,0,0)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public virtual int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = skillContext.Parent.ActiveSkill.GetCharacterLevel()[skillContext.Parent.star];
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int dmg = BaseDamage + (int)(DamageRatio / 100f * skillContext.Parent.GetStat(StatsType.Attack));
        return dmg;
    }
    public virtual void ExecuteSkill(SkillContext skillContext)
    {
        StarLevelStats stats = skillContext.Parent.ActiveSkill.GetCharacterLevel()[skillContext.Parent.star];
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} castSkill");
        CustomLogger.Log(this, $"{skillContext.Parent} using level {skillContext.CharacterLevel} skill ,data1 = {stats.Data1},data2 = {stats.Data2}");
    }
    public virtual CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return this;
    }

    public HexNode FindMaxOccupiedEntityGrid(int range, List<HexNode> hexNodes, SkillContext skillContext, bool FindingAlly)
    {
        float maxCount = 0;
        HexNode maxNode = null;
        bool IsAlly = skillContext.Parent.IsAlly;
        foreach (var startNode in hexNodes)
        {
            float count = CountOccupiedInRange(startNode, range, IsAlly, FindingAlly);

            if (count > maxCount)
            {
                maxCount = count;
                maxNode = startNode;
            }
        }
        Debug.Log($"max count = {maxCount},pos = {maxNode.Position}");
        return maxNode;
    }
    private float CountOccupiedInRange(HexNode startNode, int range, bool isAlly, bool GetAlly)
    {
        float count = 0;
        if (startNode.OccupyingCharacter != null)
        {
            if (startNode.OccupyingCharacter.IsAlly == isAlly == GetAlly)
            {
                count = 1.1f;
            }
        }
        List<HexNode> hexNodes = Utility.GetHexInRange(startNode, range);
        hexNodes.Remove(startNode);
        foreach (var item in hexNodes)
        {
            if (item.OccupyingCharacter != null)
            {
                if (item.OccupyingCharacter.IsAlly == isAlly == GetAlly)
                {
                    count++;
                }
            }
        }
        return count;
    }
}
public class NullSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {

    }
}
public class ArisSkill : CharacterSkillBase//愛麗絲找到一個可以貫穿最多敵人的方向，向該方向發射一個可以貫穿敵人的子彈
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int DamageRatio;
    public int BaseDamage;
    public int DecayFactor;
    public ArisActiveSkill ArisActiveSkill;
    public ArisSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,315,80)},
            {2, new StarLevelStats(0,430,90)},
            {3, new StarLevelStats(0,780,100)}
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = GetCharacterLevel()[level];
        base.ExecuteSkill(skillContext);
        BaseDamage = stats.Data1;
        DamageRatio = stats.Data2;
        DecayFactor = stats.Data3;
        skillContext.DamageAmount = (int)(skillContext.Parent.GetAttack() * DamageRatio * 0.01f) + BaseDamage;
        ArisActiveSkill = skillContext.Parent.GetComponent<ArisActiveSkill>();
        ArisActiveSkill.ArisSkill = this;
        ArisActiveSkill.SkillContext = skillContext;
        ArisActiveSkill.StartSkillCorutine();
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {

        return new ArisEnhancedSkill(this);
    }
}
public class ArisEnhancedSkill : CharacterSkillBase
{
    private ArisSkill originalSkill;
    public ArisActiveSkill ArisActiveSkill_Enhanced;
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int DamageRatio;
    public int BaseDamage;
    public ArisEnhancedSkill(ArisSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,475,100)}, // 星級1的數據
            {2, new StarLevelStats(0,600,100)}, // 星級2的數據
            {3, new StarLevelStats(0,900,100)}  // 星級3的數據
        };
        return statsByStarLevel;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDamage = stats.Data1;
        DamageRatio = stats.Data2;

        skillContext.DamageAmount = (int)(skillContext.Parent.GetAttack() * DamageRatio * 0.01f) + BaseDamage;
        ArisActiveSkill_Enhanced = skillContext.Parent.GetComponent<ArisActiveSkill>();
        ArisActiveSkill_Enhanced.SkillContext = skillContext;
        ArisActiveSkill_Enhanced.StartEnhanceSkillCorutine();
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Aris Skill");
    }
}
public class AyaneSkill : CharacterSkillBase//陵音(Ayane)找到一個範圍內最多友軍的空格，在該格投放治療，範圍內的友軍會被一次性的治癒
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseHeal;
    public int HealRatio;
    public AyaneSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,380)}, // 星級1的數據
            {2, new StarLevelStats(130,405)}, // 星級2的數據
            {3, new StarLevelStats(170,480)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = GetCharacterLevel()[level];
        BaseHeal = stats.Data1;
        HealRatio = stats.Data2;
        base.ExecuteSkill(skillContext);
        bool IsFindingAlly = true;
        HexNode targetHex = FindMaxOccupiedEntityGrid(skillContext.Range, skillContext.hexMap, skillContext, IsFindingAlly);
        GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, targetHex.Position + new Vector3(0, 3, 0), Quaternion.identity);
        HealPack.GetComponent<HealPack>().InitStats(targetHex, skillContext.Range, BaseHeal + (int)((HealRatio / 100f) * skillContext.Parent.GetStat(StatsType.healAbility)), skillContext.Parent, skillContext.Parent.IsAlly);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AyaneEnhancedSkill(this);
    }
}
public class AyaneEnhancedSkill : CharacterSkillBase //可能可以與阿拜多斯的羈絆互動?
{
    private AyaneSkill originalSkill;
    public AyaneEnhancedSkill(AyaneSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,380)}, // 星級1的數據
            {2, new StarLevelStats(130,405)}, // 星級2的數據
            {3, new StarLevelStats(170,480)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Ayane Skill");
    }
}
public class HarukaSkill : CharacterSkillBase//遙香(Haruka)架起護盾，並向前攻擊數波，她前方的敵軍將會被減少攻擊力
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public HarukaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,80,40)}, // 星級1的數據
            {2, new StarLevelStats(13,120,60)}, // 星級2的數據
            {3, new StarLevelStats(17,200,100)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        PressureRatio = stats.Data3;
        base.ExecuteSkill(skillContext);
        HexNode hexNode = skillContext.CurrentTarget.CurrentHex;
        HexNode currentHex = skillContext.Parent.CurrentHex;
        var commonNeighbors = hexNode.Neighbors.Intersect(currentHex.Neighbors)
                               .Where(h => h != currentHex)
                               .ToList();
        commonNeighbors.Add(hexNode);
        bool isAlly = skillContext.Parent.IsAlly;
        foreach (var neighbor in commonNeighbors)
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != isAlly)
            {
                int dmg = BaseDmg + (int)skillContext.Parent.GetStat(StatsType.Attack) * DmgRatio / 100 + PressureRatio * PressureManager.Instance.GetPressure();
                (bool, int) tuple = skillContext.Parent.CalculateCrit(dmg);
                neighbor.OccupyingCharacter.GetHit(tuple.Item2, skillContext.Parent, "HarukaSkill", tuple.Item1);
                Effect effect = EffectFactory.CreateHarukaMinusAtkEffect(5, 5, neighbor.OccupyingCharacter);
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(effect);
            }
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HarukaEnhancedSkill(this);
    }

}
public class HarukaEnhancedSkill : CharacterSkillBase
{
    private HarukaSkill originalSkill;
    public HarukaEnhancedSkill(HarukaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,80,40)}, // 星級1的數據
            {2, new StarLevelStats(13,120,60)}, // 星級2的數據
            {3, new StarLevelStats(17,200,100)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Haruka Skill");
    }
}
public class HarunaSkill : CharacterSkillBase//羽留奈(Haruna)朝著當前生命值最低的敵人進行狙擊
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public HarunaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,80,30)}, // 星級1的數據
            {2, new StarLevelStats(13,120,45)}, // 星級2的數據
            {3, new StarLevelStats(17,200,75)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        PressureRatio = stats.Data3;
        LayerMask layer = skillContext.Parent.GetTargetLayer();
        base.ExecuteSkill(skillContext);
        CharacterCTRL lowestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        skillContext.Parent.transform.LookAt(lowestEnemy.GetHitPoint);
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int initDmg = BaseDmg + DmgRatio * skillContext.Parent.GetAttack() + PressureManager.Instance.GetPressure() * PressureRatio;
        (bool, int) tuple = skillContext.Parent.CalculateCrit(initDmg);
        bullet.GetComponent<NormalBullet>().Initialize(tuple.Item2, layer, skillContext.Parent, 20, lowestEnemy.gameObject, true, tuple.Item1);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HarunaEnhancedSkill(this);
    }
}
public class HarunaEnhancedSkill : CharacterSkillBase
{
    private HarunaSkill originalSkill;
    private int Count;
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public HarunaEnhancedSkill(HarunaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,155,30)}, // 星級1的數據
            {2, new StarLevelStats(0,230,45)}, // 星級2的數據
            {3, new StarLevelStats(0,390,75)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        Count++;
        if (Count >= 3)
        {
            Count -= 3;
            Transform parentTransform = skillContext.Parent.transform;
            List<Transform> enemyTransforms = skillContext.Enemies.Select(e => e.transform).ToList();
            var sortedTargets = enemyTransforms
                .OrderBy(enemy => Vector3.Distance(parentTransform.position, enemy.position))
                .ToList();
            List<Transform> closestTargets = new List<Transform>();
            while (closestTargets.Count < 3)
            {
                foreach (var target in sortedTargets)
                {
                    if (closestTargets.Count < 3)
                    {
                        closestTargets.Add(target);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            foreach (var item in closestTargets)
            {
                Explosion(item.gameObject, skillContext);
            }
            CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Haruna Skill 3rd time");
            return;
        }
        Explosion(skillContext.Parent.Target, skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Haruna Skill,count = {Count}");
    }
    public void Explosion(GameObject target, SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        PressureRatio = stats.Data3;
        List<CharacterCTRL> characters = target.GetComponent<CharacterCTRL>().CurrentHex.GetCharacterOnNeighborHex(1, true);
        int dmg = BaseDmg + (int)skillContext.Parent.GetStat(StatsType.Attack) * DmgRatio / 100 + PressureRatio * PressureManager.Instance.GetPressure();
        (bool, int) tuple = skillContext.Parent.CalculateCrit(dmg);
        foreach (var item in characters)
        {
            item.GetHit(tuple.Item2, skillContext.Parent, "HarunaEnhancedSkill", tuple.Item1);
        }
    }
}
public class MichiruSkill : CharacterSkillBase//滿(Michiru)對當前目標發射煙火，使得其周圍一格的地板被灼燒，在十秒內造成dot
{
    public int BaseDmg;
    public int DmgRatio;
    public MichiruSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,155,30)}, // 星級1的數據
            {2, new StarLevelStats(0,230,45)}, // 星級2的數據
            {3, new StarLevelStats(0,390,75)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        LayerMask layer = skillContext.Parent.GetTargetLayer();
        CharacterCTRL target = skillContext.CurrentTarget;
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = GetAttackCoefficient(skillContext);
        (bool, int) tuple = skillContext.Parent.CalculateCrit(dmg);
        bullet.GetComponent<NormalBullet>().Initialize(tuple.Item2, layer, skillContext.Parent, 20f, target.gameObject, true, tuple.Item1);
        HexNode targetHex = target.CurrentHex;
        float burningDuration = 10f;
        int damagePerTick = dmg / 2;
        float tickInterval = 1f;
        targetHex.ApplyBurningEffect(burningDuration, damagePerTick, tickInterval, skillContext.Parent);
        foreach (HexNode neighbor in targetHex.Neighbors)
        {
            neighbor.ApplyBurningEffect(burningDuration, damagePerTick, tickInterval, skillContext.Parent);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MichiruEnhancedSkill(this);
    }
}
public class MichiruEnhancedSkill : CharacterSkillBase
{
    private MichiruSkill originalSkill;
    public MichiruEnhancedSkill(MichiruSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        //想不到
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Michiru Skill");
    }
}
public class NatsuSkill : CharacterSkillBase//夏(Natsu)治癒自己一定血量，架起護盾，一定時間內免疫負面狀態
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseHeal;
    public int healRatio;
    public int immuneduraion;
    public NatsuSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,1,1)},
            {2, new StarLevelStats(20,2,2)},
            {3, new StarLevelStats(30,3,3)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int heal = GetAttackCoefficient(skillContext);
        skillContext.Parent.Heal(heal, skillContext.Parent);
        Effect effect = EffectFactory.ClarityEffect(5, skillContext.Parent);
        skillContext.Parent.effectCTRL.AddEffect(effect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new NatsuEnhancedSkill(this);
    }
}
public class NatsuEnhancedSkill : CharacterSkillBase        //如果施放技能時解除了負面效果，永久獲得生命值。如果施放技能時沒有負面效果可以解除，永久獲得攻擊力。
{
    private NatsuSkill originalSkill;
    public NatsuEnhancedSkill(NatsuSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);

        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Natsu Skill");
    }
}
public class NoaSkill : CharacterSkillBase//乃愛(Noa)對生命值上限最低的敵軍施放"標記"，每一次施放時，使得周圍一格的友軍射程+1。所有友軍都會設法攻擊被標記的敵軍。如果該敵人已經被"標記"狀態，將會重複標記，使得她受到的傷害提升
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public NoaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10)},
            {2, new StarLevelStats(15)},
            {3, new StarLevelStats(20)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        return skillContext.Parent.GetAttack();
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        int amount = stats.Data1;
        base.ExecuteSkill(skillContext);
        CharacterCTRL weakestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.Health, false, 1, true)[0];
        if (weakestEnemy.isMarked)
        {
            Effect DmgEffect = EffectFactory.StatckableStatsEffct(0, "Noa Skill", amount, StatsType.PercentageResistence, skillContext.Parent, true);
            DmgEffect.SetActions(
                (character) => character.ModifyStats(StatsType.PercentageResistence, DmgEffect.Value, DmgEffect.Source),
                (character) => character.ModifyStats(StatsType.PercentageResistence, -DmgEffect.Value, DmgEffect.Source)
            );
            weakestEnemy.effectCTRL.AddEffect(DmgEffect);
        }
        else
        {
            Effect effect = EffectFactory.CreateMarkedEffect(weakestEnemy);
            weakestEnemy.effectCTRL.AddEffect(effect);
            CustomLogger.Log(this, $"Marking weakestEnemy {weakestEnemy}");
        }
        foreach (var item in skillContext.Parent.GetAllies())
        {
            if (item.CheckEnemyIsInrange(weakestEnemy))
            {
                item.ForceChangeTarget(weakestEnemy);
            }
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new NoaEnhancedSkill(this);
    }
}
public class NoaEnhancedSkill : CharacterSkillBase
{
    private NoaSkill originalSkill;
    public NoaEnhancedSkill(NoaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)//現在被標記就會賦予受到傷害提升的debuff，並且當被標記的敵人如果在擁有標記期間死去，一個新的敵人獲得"標記"，其完全等效於原技能
    {
        base.ExecuteSkill(skillContext);

        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Noa Skill");
    }
}
public class SerikaSkill : CharacterSkillBase//茜香(Serika)施放技能後獲得攻擊速度和物理攻擊。
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public SerikaSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,1,1)},
            {2, new StarLevelStats(20,2,2)},
            {3, new StarLevelStats(30,3,3)}
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        base.ExecuteSkill(skillContext);
        Effect effect = EffectFactory.CreateSerikaRageEffect(10, 5, skillContext.Parent);
        skillContext.Parent.effectCTRL.AddEffect(effect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SerikaEnhancedSkill(this);
    }
}
public class SerikaEnhancedSkill : CharacterSkillBase//施放技能後的該段時間，普通攻擊會有機率獲得金錢
{
    private SerikaSkill originalSkill;
    public SerikaEnhancedSkill(SerikaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);

        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Serika Skill");
    }
}
public class SerinaSkill : CharacterSkillBase//serina治癒生命值最低的友軍
{

    public SerinaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,85,1)},
            {2, new StarLevelStats(75,115,1)},
            {3, new StarLevelStats(115,215,2)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int baseHeal = stats.Data1;
        int healRatio = stats.Data2;
        int boxAmount = stats.Data3;
        return baseHeal + (int)(healRatio * 0.01f * skillContext.Parent.GetAttack());
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int healAmount = GetAttackCoefficient(skillContext);
        int boxAmount = stats.Data3;
        base.ExecuteSkill(skillContext);
        int actualBoxAmount = Math.Min(boxAmount, skillContext.Parent.GetAllies().Count);
        List<CharacterCTRL> lowestHpAllies = Utility.GetSpecificCharacters(skillContext.Parent.GetAllies(), StatsType.currHealth, false, actualBoxAmount, false);
        for (int i = 0; i < boxAmount; i++)
        {
            Vector3 pos = lowestHpAllies[i % actualBoxAmount].transform.position + new Vector3(0, 10, 0);
            GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, pos, Quaternion.Euler(-90, 0, 0));
            HexNode hex = Utility.GetHexOnPos(pos);
            HealPack.GetComponent<HealPack>().InitStats(hex, skillContext.Range, healAmount, skillContext.Parent, skillContext.Parent.IsAlly);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SerinaEnhancedSkill(this);
    }
}
public class SerinaEnhancedSkill : CharacterSkillBase//我方總計普通攻擊(後勤也算)若干次時，本回合暫時增加爆擊傷害
{
    private SerinaSkill originalSkill;
    public SerinaEnhancedSkill(SerinaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Serina Skill");
    }
}
public class ShizukoSkill : CharacterSkillBase//靜子(shizuko)在角色(無論敵我)最多的空格子放置一台餐車，該餐車"架起護盾"，且護盾範圍+2的範圍內的友軍增加命中率
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350,3,5)},
            {2, new StarLevelStats(250,475,3,5)},
            {3, new StarLevelStats(400,585,4,6)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int range1 = GetCharacterLevel()[skillContext.Parent.star].Data3;
        int range2 = GetCharacterLevel()[skillContext.Parent.star].Data4;
        HexNode hex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, range1, false, true, skillContext.Parent.CurrentHex);
        skillContext.Parent.GetComponent<ShizukoActiveSkill>().SpawnTruck(hex, skillContext.Parent);
        foreach (var item in Utility.GetCharacterInrange(hex, range1, skillContext.Parent, true))
        {
            item.AddShield(100, 5f, skillContext.Parent);
        }
        foreach (var item in Utility.GetCharacterInrange(hex, range2, skillContext.Parent, true))
        {
            Effect effect = EffectFactory.CreateShizukoEffect(30, 10, skillContext.Parent);
            item.effectCTRL.AddEffect(effect);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ShizukoEnhancedSkill(this);
    }
}
public class ShizukoEnhancedSkill : CharacterSkillBase//餐車不會被破壞，但歸零時會失去特效。第一次施放技能後的技能改為增加餐車的最大生命(當前生命隨著最大生命的增加量而增加)，現在餐車還會賦予周圍友軍自身生命%數的治療
{
    private ShizukoSkill originalSkill;
    public ShizukoEnhancedSkill(ShizukoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shizuko Skill");
    }
}
public class SumireSkill : CharacterSkillBase//堇(sumire)翻滾到最好的位置，且對當前目標射擊一次。12、33、70禎時射擊。9-30禎時短距移動，30~50長距移動，
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350)},
            {2, new StarLevelStats(250,475)},
            {3, new StarLevelStats(400,585)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        var sumireActiveSkill = skillContext.Parent.GetComponent<SumireActiveSkill>();
        if (sumireActiveSkill == null)
        {
            CustomLogger.LogWarning(this, "SumireActiveSkill component not found on Parent.");
            return;
        }
        sumireActiveSkill.parent = skillContext.Parent;
        sumireActiveSkill.StartCoroutine(sumireActiveSkill.SkillRoutine(skillContext));
    }

    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SumireEnhancedSkill(this);
    }
}
public class SumireEnhancedSkill : CharacterSkillBase//每施放一次技能就會增加生命值，自身的攻擊力隨著最大生命值上升。
{
    private SumireSkill originalSkill;
    public SumireEnhancedSkill(SumireSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Sumire Skill");
    }
}
public class AkoSkill : CharacterSkillBase
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public AkoSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(0,475,100)}, // 星級1的數據
            {2, new StarLevelStats(0,600,100)}, // 星級2的數據
            {3, new StarLevelStats(0,900,100)}  // 星級3的數據
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)//使本局造成傷害最多的角色獲得"在下一次主動技能施放結束前，增加爆擊機率和爆擊數值"
    {

        Effect effect = EffectFactory.CreateAkoActiveSkillBuff(40, 0, skillContext.Parent);
        DamageStatisticsManager.Instance.GetTopDamageDealer(skillContext.Parent.IsAlly).effectCTRL.AddEffect(effect);
        CustomLogger.Log(this, $"{DamageStatisticsManager.Instance.GetTopDamageDealer(skillContext.Parent.IsAlly)} getting buff {effect.GetType()}");
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AkoEnhancedSkill(this);
    }
}
public class AkoEnhancedSkill : CharacterSkillBase
{
    private AkoSkill originalSkill;
    public AkoEnhancedSkill(AkoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Ako Skill");
    }
}
public class AzusaSkill : CharacterSkillBase//梓(Azusa)對當前目標狙擊，短暫延遲後，小範圍內爆炸
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,380)}, // 星級1的數據
            {2, new StarLevelStats(150,450)}, // 星級2的數據
            {3, new StarLevelStats(225,780)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = skillContext.Parent.ActiveSkill.GetCharacterLevel()[skillContext.Parent.star];
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int dmg = BaseDamage + (int)(DamageRatio / 100f * skillContext.Parent.GetStat(StatsType.Attack));
        return dmg;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL c = skillContext.Parent.Target.GetComponent<CharacterCTRL>();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)skillContext.Parent.GetStat(StatsType.Attack);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        List<HitEffect> hitEffect = new List<HitEffect> { new AzusaSkillEffect() };
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, c.gameObject, true, iscrit, hitEffect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AzusaEnhancedSkill(this);
    }
}
public class AzusaEnhancedSkill : CharacterSkillBase//若目標因為梓(任意來源)死亡，永久增加攻擊力。
{
    private AzusaSkill originalSkill;
    public AzusaEnhancedSkill(AzusaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Azusa Skill");
    }
}
public class ChiseSkill : CharacterSkillBase//知世(Chise)對範圍內的格子灑毒，對站在上面的敵人造成dot傷害
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(20,40)}, // 星級1的數據
            {2, new StarLevelStats(40,60)}, // 星級2的數據
            {3, new StarLevelStats(80,90)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 3, true, false, skillContext.currHex);
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = GetCharacterLevel()[level];
        base.ExecuteSkill(skillContext);
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int dmg = (int)(skillContext.Parent.GetAttack() * DamageRatio * 0.01f) + BaseDamage;
        foreach (var item in Utility.GetHexInRange(targetHex, 3))
        {
            item.ApplyBurningEffect(5, dmg, 0.5f, skillContext.Parent);
        }
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ChiseEnhancedSkill(this);
    }
}
public class ChiseEnhancedSkill : CharacterSkillBase//還會降低攻擊速度，惡寒期間，百鬼夜行所屬學生會盡可能地攻擊範圍內可攻擊的惡寒敵對學生
{
    private ChiseSkill originalSkill;
    public ChiseEnhancedSkill(ChiseSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Chise Skill");
    }
}
public class FuukaSkill : CharacterSkillBase//風華(Fukka)找到一個範圍內最多友軍的空格，在該格投放治療，使得友軍盡可能往該格移動，範圍內的友軍會被一次性的治癒
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public FuukaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(20,40)}, // 星級1的數據
            {2, new StarLevelStats(40,60)}, // 星級2的數據
            {3, new StarLevelStats(80,90)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = GetCharacterLevel()[level];
        base.ExecuteSkill(skillContext);
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int healAmount = (int)(skillContext.Parent.GetAttack() * DamageRatio * 0.01f) + BaseDamage;
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 3, false, false, skillContext.currHex, true);
        GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, targetHex.Position, Quaternion.Euler(-90, 0, 0));
        HealPack.GetComponent<HealPack>().InitStats(targetHex, 3, healAmount, skillContext.Parent, skillContext.Parent.IsAlly);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new FuukaEnhancedSkill(this);
    }
}
public class FuukaEnhancedSkill : CharacterSkillBase
{
    private FuukaSkill originalSkill;
    public FuukaEnhancedSkill(FuukaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Fukka Skill");
    }
}
public class IzunaSkill : CharacterSkillBase//伊樹菜(Izuna)沒有主動技能，將第六次普通攻擊替換為"對圓形範圍內敵人造成傷害"
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(20,40)}, // 星級1的數據
            {2, new StarLevelStats(40,60)}, // 星級2的數據
            {3, new StarLevelStats(80,90)}  // 星級3的數據
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f) * skillContext.Parent.GetAttack();
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int dmg = skillContext.Parent.ActiveSkill.GetAttackCoefficient(skillContext.Parent.GetSkillContext());
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        Utility.DealDamageInRange(skillContext.Parent.Target.GetComponent<CharacterCTRL>().CurrentHex, 1, skillContext.Parent, dmg1, DamageSourceType.Skill.ToString(), iscrit);
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new IzunaEnhancedSkill(this);
    }
}
public class IzunaEnhancedSkill : CharacterSkillBase//獲得技能"瞬間出現在最佳位置，且三秒內獲得大量攻擊速度，此技能將會有三秒的魔力鎖"
{
    private IzunaSkill originalSkill;
    public IzunaEnhancedSkill(IzunaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Izuna Skill");
    }
}
public class KayokoSkill : CharacterSkillBase//佳代子(Kayoko)對大範圍敵人造成少量傷害及恐懼
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public KayokoSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,50,1,0,1.5f)},
            {2, new StarLevelStats(15,60,1,0,2.0f)},
            {3, new StarLevelStats(23,72,2,0,3.0f)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        PressureRatio = stats.Data3;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack()) + PressureRatio * PressureManager.Instance.GetPressure();
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        List<CharacterCTRL> characters = SpawnGrid.Instance.GetCharactersWithinRadius(skillContext.currHex, true, 6, true, skillContext.Parent);
        FearManager.Instance.ApplyFear(skillContext.Parent, characters, GetCharacterLevel()[skillContext.CharacterLevel].Data5);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {

        return new KayokoEnhancedSkill(this);
    }
}
public class KayokoEnhancedSkill : CharacterSkillBase
{
    private KayokoSkill originalSkill;
    public KayokoEnhancedSkill(KayokoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        //恐懼時長將隨著威壓變化，現在普通攻擊也有機率使得敵人恐懼，機率和時間同樣受益於威壓
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Kayoko Skill");
    }
}
public class KazusaSkill : CharacterSkillBase//和紗(Kazusa)增加一定攻擊力之後，狙擊當前生命值最低的敵人(會被阻擋)
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350,1)},
            {2, new StarLevelStats(250,475,1)},
            {3, new StarLevelStats(400,585,2)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        Effect kazusaAttackEffect = EffectFactory.StatckableStatsEffct(5, "Kazusa", 50, StatsType.Attack, skillContext.Parent, false);
        kazusaAttackEffect.SetActions(
            (character) => character.ModifyStats(StatsType.Attack, kazusaAttackEffect.Value, kazusaAttackEffect.Source),
            (character) => character.ModifyStats(StatsType.Attack, -kazusaAttackEffect.Value, kazusaAttackEffect.Source)
        );
        skillContext.Parent.effectCTRL.AddEffect(kazusaAttackEffect);
        CharacterCTRL lowestHpenemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = GetAttackCoefficient(skillContext);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, lowestHpenemy.gameObject, true, iscrit);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new KazusaEnhancedSkill(this);
    }
}
public class KazusaEnhancedSkill : CharacterSkillBase//被此技能擊中的敵人將會被打上"sugar rush"標記，不會被驅散，持續到回合結束，若此技能立刻擊殺之，或者是有此標記的敵人死亡，我方隊伍永久增加攻擊速度(後來者也算)
{
    private KazusaSkill originalSkill;
    public KazusaEnhancedSkill(KazusaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Kazusa Skill");
    }
}
public class MineSkill : CharacterSkillBase//美彌(Mine)跳躍到敵人最多的位置，擊暈他們
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350,3,1)},
            {2, new StarLevelStats(250,475,3,2)},
            {3, new StarLevelStats(400,585,5,5)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int range = stats.Data3;
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, range, true, true, skillContext.currHex);
        if (targetHex != null)
        {
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex, range, skillContext));
        }
    }
    private IEnumerator JumpToTarget(CharacterCTRL character, HexNode targetHex, int range, SkillContext skillContext)
    {
        yield return new WaitForSeconds(31f / 30f);
        HexNode hex = character.CurrentHex;
        character.CurrentHex.HardRelease();
        Vector3 startPosition = character.transform.position;
        Vector3 targetPosition = targetHex.Position;
        targetHex.HardReserve(character);
        float jumpDuration = 9f / 30f;
        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;
            Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, t);
            character.transform.position = nextPosition;

            yield return null;
        }
        character.transform.position = targetPosition;
        targetHex.HardReserve(character);
        if (targetHex != hex)
        {
            hex.HardRelease();
        }
        StunEnemiesAroundHex(targetHex, range, skillContext);
    }

    private void StunEnemiesAroundHex(HexNode targetHex, int range, SkillContext skillContext)
    {
        bool isAlly = skillContext.Parent.IsAlly;
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int duration = stats.Data3;
        foreach (HexNode neighbor in SpawnGrid.Instance.GetHexNodesWithinRange(targetHex, range))
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != isAlly)
            {

                Effect stunEffect = EffectFactory.CreateStunEffect(duration, neighbor.OccupyingCharacter);
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(stunEffect);
                neighbor.OccupyingCharacter.AudioManager.PlayCrowdControlledSound();
            }
            neighbor.SetColorState(ColorState.TemporaryYellow, .5f);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MineEnhancedSkill(this);
    }
}
public class MineEnhancedSkill : CharacterSkillBase        //不再擊暈它們，改為在落地時，施放一次小技能，對周圍兩格內敵人造成一次震擊，減少它們的攻擊速度
{
    private MineSkill originalSkill;
    public MineEnhancedSkill(MineSkill originalSkill)
    {

        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);

        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Mine Skill");
    }
}
public class MomoiSkill : CharacterSkillBase//桃井(Momoi)尋找可包含最多敵人的角度，且進行一次掃射
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,80,0,0,5f)},
            {2, new StarLevelStats(15,95,0,0,5f)},
            {3, new StarLevelStats(22,115,0,0,5f)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        float interval = stats.Data5;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {

        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MomoiEnhancedSkill(this);
    }
}
public class MomoiEnhancedSkill : CharacterSkillBase//桃和綠將會額外獲得mythic羈絆
{
    private MomoiSkill originalSkill;
    public MomoiEnhancedSkill(MomoiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Momoi Skill");
    }
}
public class NonomiSkill : CharacterSkillBase//野乃美(Nonomi)以當前目標為中心，進行掃射。
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,8,1,0,0.5f)},
            {2, new StarLevelStats(15,8,1,0,0.5f)},
            {3, new StarLevelStats(22,10,10,0,0.5f)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {

        base.ExecuteSkill(skillContext);
        //Finished in Barrage observer 
    }

    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new NonomiEnhancedSkill(this);
    }
}
public class NonomiEnhancedSkill : CharacterSkillBase//此技能將會有機率在命中敵人時掉落金錢，和爆擊率成正比。
{
    private NonomiSkill originalSkill;
    public NonomiEnhancedSkill(NonomiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Nonomi Skill");
    }
}
public class ShirokoSkill : CharacterSkillBase//白子(shiroko)招喚一個無人機攻擊若干次
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350)},
            {2, new StarLevelStats(250,475)},
            {3, new StarLevelStats(400,585)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        ShirokoActiveSkill s = skillContext.Parent.GetComponent<ShirokoActiveSkill>();
        Shiroko_Terror_DroneCTRL d = s.droneCTRL;
        if (d == null)
        {
            s.droneRef = s.GetDrone(skillContext);

            s.droneRef.transform.SetParent(skillContext.Parent.transform, true);
        }
        else
        {
            s.droneRef.SetActive(true);
            d.stack++;
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ShirokoEnhancedSkill(this);
    }
}
public class ShirokoEnhancedSkill : CharacterSkillBase//無人機會代替白子受到傷害，傷害為白子目前生命值的20%，護盾被破壞時，將會朝兩格內最近的敵人墜毀
{
    private ShirokoSkill originalSkill;
    public ShirokoEnhancedSkill(ShirokoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shiroko Skill");
    }
}
public class TsubakiSkill : CharacterSkillBase//樁(tsubaki)嘲諷所有敵人，若樁在敵人的攻擊範圍內，則他們都會轉為攻擊他。
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350)},
            {2, new StarLevelStats(250,475)},
            {3, new StarLevelStats(400,585)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        base.ExecuteSkill(skillContext);

        foreach (var item in skillContext.Parent.GetEnemies())
        {
            if (item.CheckEnemyIsInrange(skillContext.Parent))
            {
                Effect effect = EffectFactory.CreateTsubakiTauntEffct(0, 5, item);

                item.ForceChangeTarget(skillContext.Parent);
                item.effectCTRL.AddEffect(effect);
            }
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new TsubakiEnhancedSkill(this);
    }
}
public class TsubakiEnhancedSkill : CharacterSkillBase//生命值掉落到40%以下時，恢復40%最大生命，每被傷害100次時，增加一次可觸發的機會
{
    private TsubakiSkill originalSkill;
    public TsubakiEnhancedSkill(TsubakiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Tsubaki Skill");
    }
}
public class YuukaSkill : CharacterSkillBase//優香(yuuka)跳到三格內友軍最多的格子，給予他們基於自己最大生命值的護盾。
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,80,2)},
            {2, new StarLevelStats(75,125,2)},
            {3, new StarLevelStats(125,250,3)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseShield = stats.Data1;
        int ShieldRatio = stats.Data2;
        return BaseShield + (int)(ShieldRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int range = GetCharacterLevel()[skillContext.Parent.star].Data3;
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, range + 1, false, true, skillContext.currHex);

        if (targetHex != null)
        {
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex, range, skillContext));
        }
    }
    private IEnumerator JumpToTarget(CharacterCTRL character, HexNode targetHex, int range, SkillContext skillContext)
    {
        HexNode hex = character.CurrentHex;
        character.CurrentHex.HardRelease();

        Vector3 startPosition = character.transform.position;
        Vector3 targetPosition = targetHex.Position;

        float jumpDuration = 25f / 30f;
        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;
            Vector3 nextPosition = Vector3.Lerp(startPosition, targetPosition, t);
            character.transform.position = nextPosition;
            yield return null;
        }

        character.transform.position = targetPosition;
        targetHex.HardReserve(character);
        if (targetHex != hex)
        {
            hex.HardRelease();
        }
        int shield = GetAttackCoefficient(skillContext);
        ShieldAllyAroundHex(targetHex, range, shield, skillContext);
        skillContext.Parent.AddShield(shield, 1.0f, skillContext.Parent);
    }
    private void ShieldAllyAroundHex(HexNode targetHex, int range, int amount, SkillContext skillContext)
    {

        foreach (var character in Utility.GetCharacterInrange(targetHex, range, skillContext.Parent, true))
        {
            character.AddShield(amount, 1.0f, skillContext.Parent);
        }

    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new YuukaEnhancedSkill(this);
    }
}
public class YuukaEnhancedSkill : CharacterSkillBase//改為跳到敵人中央。閃避值上升，並且每一次的閃避都會填充魔力條
{
    private YuukaSkill originalSkill;
    public YuukaEnhancedSkill(YuukaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Yuuka Skill");
    }
}
public class HinaSkill : CharacterSkillBase//陽奈(Hina)朝著能夠覆蓋最多敵人的地方發射可以穿透敵人的彈幕
{
    public int BaseDmg;
    public int DmgRatio;
    public int PressureRatio;
    public HinaSkill()
    {

    }
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,8,1,0,0.25f)},
            {2, new StarLevelStats(15,8,1,0,0.25f)},
            {3, new StarLevelStats(999,219,10,0,0.25f)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        PressureRatio = stats.Data3;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack()) + PressureRatio * PressureManager.Instance.GetPressure();
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {

        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HinaEnhancedSkill(this);
    }
}
public class HinaEnhancedSkill : CharacterSkillBase//子彈可以穿過單位，傷害比原技能低，但是施加標記，格黑娜學院的學生(非陽奈)造成傷害時將會造成額外傷害(根據威壓)，帶有標記的單位死去時，將會根據觸發標記的次數的固定比例額外獲得"威壓"
{
    private HinaSkill originalSkill;
    public HinaEnhancedSkill(HinaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hina Skill");
    }
}
public class HoshinoSkill : CharacterSkillBase//星野(Hoshino)架起護盾，往前方一格以及左右兩格造成數波傷害
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HashSet<CharacterCTRL> enemies = new HashSet<CharacterCTRL>();
        Dictionary<HexNode, int> directionEnemyCount = new Dictionary<HexNode, int>();
        foreach (var neighbor in skillContext.Parent.CurrentHex.Neighbors)
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != skillContext.Parent.IsAlly)
            {
                enemies.Add(neighbor.OccupyingCharacter);
            }
        }
        foreach (HexNode neighbor in skillContext.Parent.CurrentHex.Neighbors)
        {
            int enemyCount = 0;
            if (neighbor.OccupyingCharacter != null && enemies.Contains(neighbor.OccupyingCharacter))
            {
                enemyCount++;
            }
            foreach (HexNode subNeighbor in neighbor.Neighbors)
            {
                if (subNeighbor.OccupyingCharacter != null && enemies.Contains(subNeighbor.OccupyingCharacter))
                {
                    enemyCount++;
                }
            }

            directionEnemyCount[neighbor] = enemyCount;
        }
        var bestDirection = directionEnemyCount.OrderByDescending(kv => kv.Value).First().Key;
        if (bestDirection.OccupyingCharacter != null)
        {
            skillContext.Parent.ForceChangeTarget(bestDirection.OccupyingCharacter);
        }

        var bestDirectionNeighbors = new HashSet<HexNode>(bestDirection.Neighbors);
        var currentHexNeighbors = new HashSet<HexNode>(skillContext.Parent.CurrentHex.Neighbors);
        bestDirectionNeighbors.Add(bestDirection);
        skillContext.SelectedHex = bestDirectionNeighbors.Intersect(currentHexNeighbors).ToList();
        CustomLogger.Log(this, $"Hoshino skillContext.SelectedHex = {skillContext.SelectedHex.Count}");
        skillContext.Parent.GetComponent<HoshinoShotgunSkill>().skillContext = skillContext;
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HoshinoEnhancedSkill(this);
    }
}
public class HoshinoEnhancedSkill : CharacterSkillBase//常駐防禦力上升，技能造成傷害時，附帶暈眩且強迫敵人轉火。
{
    private HoshinoSkill originalSkill;
    public HoshinoEnhancedSkill(HoshinoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hoshino Skill");
    }
}
public class MikaSkill : CharacterSkillBase//被動:彌香(mika)的傷害總是會爆擊，且所有爆擊率轉為爆擊數值。主動:對一個人狙擊。此次攻擊必定爆擊，若此技能擊殺了敵人，會以相同傷害在一格之內爆炸(可無限循環)。
{
    public int DamageRatio;
    public int BaseDamage;
    public int DecayFactor;
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,5000)},
            {2, new StarLevelStats(130,405)},
            {3, new StarLevelStats(170,480)}
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL C = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.Health, false, 1, true)[0];
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        List<HitEffect> hitEffect = new List<HitEffect> { new MikaSkillEffect() };
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(skillContext.Parent.GetAttack());
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, C.gameObject, true, iscrit, hitEffect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MikaEnhancedSkill(this);
    }
}
public class MikaEnhancedSkill : CharacterSkillBase//施放技能時，盡可能將半徑三格內的敵人往兩格之內位移，且根據托拽數量而定，立刻在脫拽的中心投入若干顆"聖三一"羈絆的砲擊。
{
    private MikaSkill originalSkill;
    public MikaEnhancedSkill(MikaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Mika Skill");
    }
}
public class NeruSkill : CharacterSkillBase//寧瑠(Neru)以超多段傷害攻擊當前目標，會觸發多次的命中(on-hit)效果
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,70,1)},
            {2, new StarLevelStats(75,85,2)},
            {3, new StarLevelStats(999,500,3)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        skillContext.Parent.ManaLock = true;
        //在動畫事件內完成了
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new NeruEnhancedSkill(this);
    }
}
public class NeruEnhancedSkill : CharacterSkillBase//每當命中時，在本回合增加攻擊力。每一段都會彈射到鄰近敵人(盡可能彈射到不同敵人)，觸發全額命中效果，若無其餘敵人則對於同一人結算。
{
    private NeruSkill originalSkill;
    public NeruEnhancedSkill(NeruSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Neru Skill");
    }
}
public class TsurugiSkill : CharacterSkillBase//鶴城(tsurugi)將接下來若干次的普通攻擊替換為範圍傷害，並且獲取高額的吸血
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350)},
            {2, new StarLevelStats(250,475)},
            {3, new StarLevelStats(400,585)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        TsurugiActiveSkill T = skillContext.Parent.GetComponent<TsurugiActiveSkill>();
        T.SpecialAttackCount = 5;
        T.ChangeToSpecialAttack();

        //在專屬代碼內完成了。
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new TsurugiEnhancedSkill(this);
    }
}
public class TsurugiEnhancedSkill : CharacterSkillBase//恢復造成傷害100%的生命值，每一次將要死亡時，生命值不會低於1。且若在五秒內將生命值恢復到最大值，將不會死亡，否則死亡。
{
    private TsurugiSkill originalSkill;
    public TsurugiEnhancedSkill(TsurugiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Tsurugi Skill");
    }
}
public class WakamoSkill : CharacterSkillBase//若藻(wakamo)對當前目標狙擊，且打上標記。標記會儲存部分受到傷害，且在五秒之後清除標記且引爆所儲存的傷害。
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,315,0,0,5.0f)},
            {2, new StarLevelStats(150,475,0,0,5.0f)},
            {3, new StarLevelStats(500,9999,0,0,5.0f)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL c = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        if (c != null)
        {
            Effect effect = EffectFactory.CreateWakamoEffect(0, 5, skillContext.Parent);
            c.effectCTRL.AddEffect(effect);
            List<HitEffect> hitEffect = new List<HitEffect> { new WakamoSkillEffecct() };
            GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
            int dmg = (int)skillContext.Parent.GetStat(StatsType.Attack);
            (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
            bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, c.gameObject, true, iscrit, hitEffect);

        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new WakamoEnhancedSkill(this);
    }
}
public class WakamoEnhancedSkill : CharacterSkillBase//對尚未被此角色傷害過的敵人造成傷害時，就會附加印記。現在若藻的攻擊距離沒有上限，每一次攻擊會優先尋找尚未被加上印記的，印記不會消退，而是每五秒結算一次。當帶有印記的敵人死掉時，將他身上本回合印記儲存過的所有傷害轉移到另一人身上，並且立刻結算一次。
{
    private WakamoSkill originalSkill;
    public WakamoEnhancedSkill(WakamoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Aris Skill");
    }
}
public class Shiroko_TerrorSkill : CharacterSkillBase//黑子(Shiroko_Terror)將會從很多技能之中挑選合適的施放。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        skillContext.Parent.transform.rotation = Quaternion.Euler(0, 180, 0);
        skillContext.Parent.customAnimator.animator.SetInteger("SkillID", skillContext.shirokoTerror_SkillID);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new Shiroko_TerrorEnhancedSkill(this);
    }
}
public class Shiroko_TerrorEnhancedSkill : CharacterSkillBase
{
    private Shiroko_TerrorSkill originalSkill;
    public Shiroko_TerrorEnhancedSkill(Shiroko_TerrorSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shiroko_Terror Skill");
    }
}
public class Atsuko_Skill : CharacterSkillBase//亞津子(Atsuko)召喚給大範圍內友軍回血的無人機
{
    public GameObject Drone;
    public Atsuko_Drone Atsuko_DroneRef;

    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(20,160)}, // 星級1的數據
            {2, new StarLevelStats(30,180)}, // 星級2的數據
            {3, new StarLevelStats(45,220)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseHeal = stats.Data1;
        int HealRatio = stats.Data2;
        return BaseHeal + (int)(HealRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        Atsuko_DroneRef = skillContext.Parent.GetComponent<Atsuko_Drone>();
        Drone = Atsuko_DroneRef.DroneRef;
        Drone.SetActive(true);
        Atsuko_DroneRef.GetDrone(5);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AtsukoEnhancedSkill(this);
    }
}
public class AtsukoEnhancedSkill : CharacterSkillBase//現在無人機還會在回血時，給友軍共享一部分的迴避數值。我方隊伍每一次的成功迴避都會給迴避的單位增加攻擊速度。
{
    private Atsuko_Skill originalSkill;
    public AtsukoEnhancedSkill(Atsuko_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Atsuko Skill");
    }
}
public class Hiyori_Skill : CharacterSkillBase//日和(Hiyori)對當前目標發射子彈，命中第一個敵人之後在2格範圍內爆炸並降低防禦
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,115)},
            {2, new StarLevelStats(150,130)},
            {3, new StarLevelStats(225,185)}
        };
        return statsByStarLevel;
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        var bulletComponent = bullet.GetComponent<NormalBullet>();
        CharacterCTRL lowestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        int dmg = BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetStat(StatsType.Attack));
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        List<HitEffect> hitEffect = new List<HitEffect> { new HiyoriSkillEffecct() };
        bulletComponent.Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 20, lowestEnemy.gameObject, true, iscrit, hitEffect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HiyoriEnhancedSkill(this);
    }
}
public class HiyoriEnhancedSkill : CharacterSkillBase //第二段的範圍傷害結算時，每一次都有機率再造成小型的爆炸，機率依爆擊率而成長。
{
    private Hiyori_Skill originalSkill;
    public HiyoriEnhancedSkill(Hiyori_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hiyori Skill");
    }
}
public class Misaki_Skill : CharacterSkillBase//被動:美咲(Misaki)普通攻擊為很慢的大砲，無法受益於攻擊速度增益，將會被等比例轉換為攻擊力，且普通攻擊會在地上留下碎片。主動:射出一大波彈藥碎片，然後引爆所有彈藥碎片。(碎片應該要盡可能的"智能")
{
    public List<GameObject> Fragments = new List<GameObject>();
    public Dictionary<GameObject, HexNode> FragmentNodes = new Dictionary<GameObject, HexNode>();

    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,75,2,4)},
            {2, new StarLevelStats(130,85,2,4)},
            {3, new StarLevelStats(170,115,3,6)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HexNode centerNode = skillContext.Parent.Target.GetComponent<CharacterCTRL>().CurrentHex;
        int dmg = GetAttackCoefficient(skillContext);
        (bool iscrit, int dmgFinal) = skillContext.Parent.CalculateCrit(dmg);
        Utility.DealDamageInRange(centerNode, 2, skillContext.Parent, dmgFinal, DamageSourceType.Skill.ToString(), iscrit);
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int extraFragmentsCount = stats.Data4;
        List<HexNode> candidateNodes = Utility.GetHexInRange(centerNode, 5)
            .Where(node =>
                !FragmentNodes.Values.Contains(node) &&
                node.OccupyingCharacter == null
            )
            .ToList();
        var selectedForFragments = candidateNodes.Take(extraFragmentsCount);
        foreach (var node in selectedForFragments)
        {
            GameObject fragment = GameObject.Instantiate(
                ResourcePool.Instance.MissleFragmentsPrefab,
                node.transform.position,
                Quaternion.identity
            );
            Fragments.Add(fragment);
            FragmentNodes.Add(fragment, node);

            CustomLogger.Log(this, $"ExecuteSkill: Spawned extra fragment at node {node.name}");
        }
    }

    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MisakiEnhancedSkill(this);
    }
}
public class MisakiEnhancedSkill : CharacterSkillBase//爆炸後的破片在回合結束前不會消失，傷害和破甲效果上升。
{
    private Misaki_Skill originalSkill;
    public MisakiEnhancedSkill(Misaki_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Misaki Skill");
    }
}
public class Miyako_Skill : CharacterSkillBase//宮子(Miyako)招喚無人機，朝著當前目標移動並且在碰到第一個敵軍時自爆。可以進行單體暈眩，且削減護甲
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,115,2)},
            {2, new StarLevelStats(70,150,2)},
            {3, new StarLevelStats(125,230,3)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        Miyako_DroneCTRL miyako_DroneCTRL = skillContext.Parent.GetComponent<Miyako_DroneCTRL>();
        miyako_DroneCTRL.parent = skillContext.Parent;
        miyako_DroneCTRL.GetDrone();
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MiyakoEnhancedSkill(this);
    }
}
public class MiyakoEnhancedSkill : CharacterSkillBase//無人機現在不會在施放技能後消失，每一次施放技能，不同的無人機會瞄準不同的目標。
{
    private Miyako_Skill originalSkill;
    public MiyakoEnhancedSkill(Miyako_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Miyako Skill");
    }
}
public class Miyu_Skill : CharacterSkillBase//被動:美遊(Miyu)不會被任何目標鎖定，但是會被範圍技能傷害，其餘友軍已經死亡時，美遊將恢復被可以鎖定的狀態。主動:對本局造成最多傷害的敵人進行狙擊，無法被任何形式阻擋。
{
    public bool UnTargetable;
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(150,350)},
            {2, new StarLevelStats(250,475)},
            {3, new StarLevelStats(400,585)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        skillContext.Parent.Target = DamageStatisticsManager.Instance.GetTopDamageDealer(skillContext.Parent.IsAlly).gameObject;
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = GetAttackCoefficient(skillContext);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, skillContext.Parent.Target, true, iscrit);
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MiyuEnhancedSkill(this);
    }
}
public class MiyuEnhancedSkill : CharacterSkillBase//當場上只剩下美遊時，在隨機位置出現三個垃圾桶，美遊將出現在其中之一，其餘三個為會爆炸的垃圾桶。
{
    private Miyu_Skill originalSkill;
    public MiyuEnhancedSkill(Miyu_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Miyu Skill");
    }
}
public class Moe_Skill : CharacterSkillBase //萌(Moe)對矩形範圍內造成dot傷害
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,115)},
            {2, new StarLevelStats(70,150)},
            {3, new StarLevelStats(125,230)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int dmg = GetAttackCoefficient(skillContext);
        foreach (var item in SpawnGrid.Instance.FindBestEnemyRectangle(4, skillContext.Parent.IsAlly))
        {
            item.ApplyBurningEffect(5, dmg / 10, 0.5f, skillContext.Parent);
            if (item.OccupyingCharacter != null
                && item.OccupyingCharacter.IsAlly != skillContext.Parent.IsAlly
                && item.OccupyingCharacter.isTargetable)
            {
                (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
                item.OccupyingCharacter.GetHit(dmg1, skillContext.Parent, DamageSourceType.Skill.ToString(), iscrit);
            }
        }
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MoeEnhancedSkill(this);
    }
}
public class MoeEnhancedSkill : CharacterSkillBase//普通攻擊替換為攻擊並灼燒該格，如果所有目標的格子都已經被點燃，則重複點燃(傷害和ex會累加計算)
{
    private Moe_Skill originalSkill;
    public MoeEnhancedSkill(Moe_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Moe Skill");
    }
}
public class Saki_Skill : CharacterSkillBase//咲(Saki)對當前目標的鄰格一格範圍內造成傷害&暈眩
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,8,1)},
            {2, new StarLevelStats(15,8,1)},
            {3, new StarLevelStats(999,219,10)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        foreach (var item in Utility.GetCharacterInrange(skillContext.Parent.Target.GetComponent<CharacterCTRL>().CurrentHex, 1, skillContext.Parent, false))
        {
            Effect effect = EffectFactory.CreateStunEffect(2, skillContext.Parent);
            item.effectCTRL.AddEffect(effect);
            (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(GetAttackCoefficient(skillContext));
            item.GetHit(dmg1, skillContext.Parent, DamageSourceType.Skill.ToString(), iscrit);
        }
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SakiEnhancedSkill(this);
    }
}
public class SakiEnhancedSkill : CharacterSkillBase//戰鬥開始時，不再回到備戰格，直接施放技能，但是不造成眩暈，無法被選中/造成傷害
{
    private Saki_Skill originalSkill;
    public SakiEnhancedSkill(Saki_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Saki Skill");
    }
}
public class Saori_Skill : CharacterSkillBase//紗織(Saori)開一槍，不會被任何物體抵擋，中間貫穿會損失一部分傷害，最低降至原傷害60%
{
    public override Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(50,185,20)},
            {2, new StarLevelStats(75,270,10)},
            {3, new StarLevelStats(125,410,0)}
        };
        return statsByStarLevel;
    }
    public override int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {

        CharacterCTRL target = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1, true)[0];
        skillContext.Parent.transform.LookAt(target.transform);

        GameObject bullet = ResourcePool.Instance.SpawnObject(
            SkillPrefab.PenetrateTrailedBullet,
            skillContext.Parent.FirePoint.position,
            Quaternion.identity
        );

        bullet.transform.position = skillContext.Parent.FirePoint.position;
        float decayFactor = GetCharacterLevel()[target.star].Data3;

        bullet.GetComponent<TrailedBullet>().Initialized(
            target.transform.position,
            skillContext.Parent.ActiveSkill.GetAttackCoefficient(skillContext),
            decayFactor,
            skillContext.Parent.GetTargetLayer(),
            skillContext.Parent,
            false
        );
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SaoriEnhancedSkill(this);
    }
}
public class SaoriEnhancedSkill : CharacterSkillBase//來源自紗織的傷害總是無視一部份防禦，%數的傷害減免對於沙織無效，且處決15%生命值以下的敵人。
{
    private Saori_Skill originalSkill;
    public SaoriEnhancedSkill(Saori_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Saori Skill");
    }
}
public class StarLevelStats
{
    public int Data1;
    public int Data2;
    public int Data3;
    public int Data4;
    public float Data5;

    public StarLevelStats(int data1 = 0, int data2 = 0, int data3 = 0, int data4 = 0, float data5 = 0)
    {
        Data1 = data1;
        Data2 = data2;
        Data3 = data3;
        Data4 = data4;
        Data5 = data5;
    }
}
public class SkillContext
{
    public List<CharacterCTRL> Allies;
    public List<CharacterCTRL> Enemies;
    public int CharacterLevel;
    public List<HexNode> hexMap;
    public CharacterCTRL Parent;
    public CharacterCTRL CurrentTarget;
    public HexNode TargetHex;
    public HexNode currHex;
    public int DamageAmount;
    public int ShieldOrHealAmount;
    public float duration;
    public int shirokoTerror_SkillID;
    public Dictionary<int, StarLevelStats> statsByStarLevel = new();

    public List<HexNode> SelectedHex = new();
    //散彈
    public int Angle;
    public int CastTimes;
    //賦予異常狀態
    public int Range;
    //
    public GameObject bullet;
    public GameObject HealPack;
    public GameObject objToSpawn;
}
public class TraitLevelStats
{
    public int Data1;
    public int Data2;
    public int Data3;
    public int Data4;
    public float Data5;

    public TraitLevelStats(int data1 = 0, int data2 = 0, int data3 = 0, int data4 = 0, float data5 = 0)
    {
        Data1 = data1;
        Data2 = data2;
        Data3 = data3;
        Data4 = data4;
        Data5 = data5;
    }
}