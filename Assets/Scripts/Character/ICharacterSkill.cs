
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public abstract class CharacterSkillBase
{
    public virtual void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} castSkill");
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
    public ArisSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(100,1,90)}, // 星級1的數據
            {2, new StarLevelStats(200,2,95)}, // 星級2的數據
            {3, new StarLevelStats(300,3,100)}  // 星級3的數據
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        BaseDamage = stats.Data1;
        DamageRatio = stats.Data2;
        DecayFactor = stats.Data3;
        base.ExecuteSkill(skillContext);
        skillContext.Parent.GetComponent<ArisActiveSkill>().skillContext = skillContext;
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        // 回傳Aris的強化版技能實例
        return new ArisEnhancedSkill(this);
    }
}
public class ArisEnhancedSkill : CharacterSkillBase
{
    private ArisSkill originalSkill;
    public ArisEnhancedSkill(ArisSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        // 強化後的Aris技能邏輯
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Aris Skill");
    }
}
public class AyaneSkill : CharacterSkillBase//找到一個範圍內最多友軍的空格，在該格投放治療，範圍內的友軍會被一次性的治癒
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseHeal;
    public int HealRatio;
    public AyaneSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(1,1)}, // 星級1的數據
            {2, new StarLevelStats(2,2)}, // 星級2的數據
            {3, new StarLevelStats(3,3)}  // 星級3的數據
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        BaseHeal = stats.Data1;
        HealRatio = stats.Data2;
        base.ExecuteSkill(skillContext);
        bool IsFindingAlly = true;
        HexNode targetHex = FindMaxOccupiedEntityGrid(skillContext.Range, skillContext.hexMap, skillContext, IsFindingAlly);
        GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, targetHex.Position + new Vector3(0, 3, 0), Quaternion.identity);
        HealPack.GetComponent<HealPack>().InitStats(targetHex, skillContext.Range, 100, skillContext.Parent, skillContext.Parent.IsAlly);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AyaneEnhancedSkill(this);
    }
}
public class AyaneEnhancedSkill : CharacterSkillBase
{
    private AyaneSkill originalSkill;
    public AyaneEnhancedSkill(AyaneSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Ayane Skill");
    }
}
public class HarukaSkill : CharacterSkillBase//架起護盾，並向前攻擊數波，她前方的敵軍將會被減少攻擊力
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public HarukaSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(1,1)},
            {2, new StarLevelStats(2,2)},
            {3, new StarLevelStats(3,3)}
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
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
                neighbor.OccupyingCharacter.GetHit(10, skillContext.Parent);
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

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Haruka Skill");
    }
}
public class HarunaSkill : CharacterSkillBase
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public HarunaSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(1,1)},
            {2, new StarLevelStats(2,2)},
            {3, new StarLevelStats(3,3)}
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int dmg = 10;
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        LayerMask layer = skillContext.Parent.GetTargetLayer();
        base.ExecuteSkill(skillContext);
        CharacterCTRL lowestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1)[0];
        skillContext.Parent.transform.LookAt(lowestEnemy.GetHitPoint);
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(lowestEnemy.transform.position, dmg, layer, skillContext.Parent, 15f, lowestEnemy.gameObject);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HarunaEnhancedSkill(this);
    }
}
public class HarunaEnhancedSkill : CharacterSkillBase
{
    private HarunaSkill originalSkill;
    public HarunaEnhancedSkill(HarunaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Haruna Skill");
    }
}
public class MichiruSkill : CharacterSkillBase//TODO: 若目標已經被灼燒，轉向一名尚未被灼燒的敵軍施放。
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseDmg;
    public int DmgRatio;
    public MichiruSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(10,1)},
            {2, new StarLevelStats(20,2)},
            {3, new StarLevelStats(30,3)}
        };
    }
    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        BaseDmg = stats.Data1;
        DmgRatio = stats.Data2;
        LayerMask layer = skillContext.Parent.GetTargetLayer();
        base.ExecuteSkill(skillContext);
        CharacterCTRL target = skillContext.CurrentTarget;
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(target.transform.position, BaseDmg, layer, skillContext.Parent, 20f,target.gameObject);
        HexNode targetHex = target.CurrentHex;
        float burningDuration = 5f;
        int damagePerTick = 10;
        float tickInterval = 1f;
        bool appliedByAlly = skillContext.Parent.IsAlly;
        targetHex.ApplyBurningEffect(burningDuration, damagePerTick, tickInterval, appliedByAlly);
        foreach (HexNode neighbor in targetHex.Neighbors)
        {
            neighbor.ApplyBurningEffect(burningDuration, damagePerTick, tickInterval, appliedByAlly);
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Michiru Skill");
    }
}
public class NatsuSkill : CharacterSkillBase//治癒自己一定血量，架起護盾，一定時間內免疫負面狀態
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int BaseHeal;
    public int healRatio;
    public int immuneduraion;
    public NatsuSkill()
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
        BaseHeal = stats.Data1;
        healRatio = stats.Data2;
        immuneduraion = stats.Data3;
        base.ExecuteSkill(skillContext);
        skillContext.Parent.AddStat(StatsType.currHealth, BaseHeal);
        Effect effect = EffectFactory.ClarityEffect(5,skillContext.Parent);
        skillContext.Parent.effectCTRL.AddEffect(effect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new NatsuEnhancedSkill(this);
    }
}
public class NatsuEnhancedSkill : CharacterSkillBase
{
    private NatsuSkill originalSkill;
    public NatsuEnhancedSkill(NatsuSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Natsu Skill");
    }
}
public class NoaSkill : CharacterSkillBase//對生命值上限最低的敵軍施放"標記"，範圍內的友軍會設法攻擊他
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public NoaSkill()
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


        CharacterCTRL StrongestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.Attack, false, 1)[0];
        Effect effect = EffectFactory.CreateMarkedEffect(level,StrongestEnemy);
        StrongestEnemy.effectCTRL.AddEffect(effect);
        foreach (var item in skillContext.Parent.GetAllies())
        {
            if (item.CheckEnemyIsInrange(StrongestEnemy))
            {
                item.ForceChangeTarget(StrongestEnemy);
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

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Noa Skill");
    }
}
public class SerikaSkill : CharacterSkillBase
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
        Effect effect = EffectFactory.CreateSerikaRageEffect(10, 5,skillContext.Parent);
        skillContext.Parent.effectCTRL.AddEffect(effect);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SerikaEnhancedSkill(this);
    }
}
public class SerikaEnhancedSkill : CharacterSkillBase
{
    private SerikaSkill originalSkill;
    public SerikaEnhancedSkill(SerikaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Serika Skill");
    }
}
public class SerinaSkill : CharacterSkillBase
{
    private Dictionary<int, StarLevelStats> statsByStarLevel;
    public int baseHeal;
    public int healRatio;
    public int boxAmount;

    public SerinaSkill()
    {
        statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(1000,1,1)},
            {2, new StarLevelStats(20,2,1)},
            {3, new StarLevelStats(30,3,2)}
        };
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        int level = skillContext.CharacterLevel;
        StarLevelStats stats = statsByStarLevel[level];
        baseHeal = stats.Data1;
        healRatio = stats.Data2;
        boxAmount = stats.Data3;
        base.ExecuteSkill(skillContext);
        int actualBoxAmount = Math.Min(boxAmount, skillContext.Parent.GetAllies().Count);
        List<CharacterCTRL> lowestHpAllies = Utility.GetSpecificCharacters(skillContext.Parent.GetAllies(), StatsType.currHealth, false, actualBoxAmount);
        for (int i = 0; i < boxAmount; i++)
        {
            Vector3 pos = lowestHpAllies[i % actualBoxAmount].transform.position + new Vector3(0, 10, 0);
            GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, pos, Quaternion.Euler(-90, 0, 0));
            HexNode hex = Utility.GetHexOnPos(pos);
            HealPack.GetComponent<HealPack>().InitStats(hex, skillContext.Range, baseHeal, skillContext.Parent, skillContext.Parent.IsAlly);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SerinaEnhancedSkill(this);
    }
}
public class SerinaEnhancedSkill : CharacterSkillBase
{
    private SerinaSkill originalSkill;
    public SerinaEnhancedSkill(SerinaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Serina Skill");
    }
}
public class ShizukoSkill : CharacterSkillBase//在角色(無論敵我)最多的空格子放置一台餐車，該餐車"架起護盾"，且護盾範圍+2的範圍內的友軍增加命中率
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HexNode hex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 3, false, true, skillContext.Parent.CurrentHex);
        skillContext.Parent.GetComponent<ShizukoActiveSkill>().SpawnTruck(hex, skillContext.Parent);
        foreach (var item in skillContext.Parent.CurrentHex.GetCharacterOnNeighborHex(3, true))
        {
            item.AddShield(100, 5f, skillContext.Parent);
            Effect effect = EffectFactory.CreateShizukoEffect(30, 10,skillContext.Parent);
            item.effectCTRL.AddEffect(effect);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ShizukoEnhancedSkill(this);
    }
}
public class ShizukoEnhancedSkill : CharacterSkillBase
{
    private ShizukoSkill originalSkill;
    public ShizukoEnhancedSkill(ShizukoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shizuko Skill");
    }
}
public class SumireSkill : CharacterSkillBase//TODO:翻滾到最好的位置，且射擊一次(記得檢查可不可以通行)。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SumireEnhancedSkill(this);
    }
}
public class SumireEnhancedSkill : CharacterSkillBase
{
    private SumireSkill originalSkill;
    public SumireEnhancedSkill(SumireSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Sumire Skill");
    }
}
public class AkoSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Ako Skill");
    }
}
public class AzusaSkill : CharacterSkillBase//對當前目標狙擊，若擊殺之則...?
{
    //TODO: 尚未完成
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL c = skillContext.Parent.Target.GetComponent<CharacterCTRL>();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(c.transform.position, skillContext.Parent.GetStat(StatsType.Attack), skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, c.gameObject);

    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new AzusaEnhancedSkill(this);
    }
}
public class AzusaEnhancedSkill : CharacterSkillBase
{
    private AzusaSkill originalSkill;
    public AzusaEnhancedSkill(AzusaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Azusa Skill");
    }
}
public class ChiseSkill : CharacterSkillBase//對範圍內的格子灑毒，對站在上面的敵人造成dot傷害
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ChiseEnhancedSkill(this);
    }
}
public class ChiseEnhancedSkill : CharacterSkillBase
{
    private ChiseSkill originalSkill;
    public ChiseEnhancedSkill(ChiseSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Chise Skill");
    }
}
public class FuukaSkill : CharacterSkillBase//找到一個範圍內最多友軍的空格，在該格投放治療，範圍內的友軍會被一次性的治癒
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 3, false, false, skillContext.currHex,true);
        GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, targetHex.Position, Quaternion.Euler(-90, 0, 0));
        HealPack.GetComponent<HealPack>().InitStats(targetHex, 3, 100, skillContext.Parent, skillContext.Parent.IsAlly);
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
        // 強化後的Aris技能邏輯
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Fukka Skill");
    }
}
public class IzunaSkill : CharacterSkillBase//???
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);

    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new IzunaEnhancedSkill(this);
    }
}
public class IzunaEnhancedSkill : CharacterSkillBase
{
    private IzunaSkill originalSkill;
    public IzunaEnhancedSkill(IzunaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Izuna Skill");
    }
}
public class KayokoSkill : CharacterSkillBase//對大範圍敵人造成少量傷害及恐懼
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        List<CharacterCTRL> characters = SpawnGrid.Instance.GetCharactersWithinRadius(skillContext.currHex, true, 6, true, skillContext.Parent);
        foreach (var item in characters)
        {
            Effect kayokoFearEffect = EffectFactory.CreateKayokoFearEffct(1, 5,item);
            item.effectCTRL.AddEffect(kayokoFearEffect);
        }
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Kayoko Skill");
    }
}
public class KazusaSkill : CharacterSkillBase//增加一定攻擊力之後，狙擊絕對生命值最低的敵人
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        Effect kazusaAttackEffect = EffectFactory.StatckableStatsEffct(5,"Kazusa",50,StatsType.Attack,skillContext.Parent,false);
        skillContext.Parent.effectCTRL.AddEffect(kazusaAttackEffect);
        CharacterCTRL lowestHpenemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1)[0];
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(lowestHpenemy.transform.position, skillContext.Parent.GetStat(StatsType.Attack),skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, lowestHpenemy.gameObject);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new KazusaEnhancedSkill(this);
    }
}
public class KazusaEnhancedSkill : CharacterSkillBase
{
    private KazusaSkill originalSkill;
    public KazusaEnhancedSkill(KazusaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Kazusa Skill");
    }
}
public class MineSkill : CharacterSkillBase//跳躍到敵人最多的位置，擊暈他們
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 3, true, true, skillContext.currHex);

        if (targetHex != null)
        {
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex, 3, skillContext));//TODO:修改此處，改為傳入skill context的range
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

        foreach (HexNode neighbor in SpawnGrid.Instance.GetHexNodesWithinRange(targetHex, range))
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != isAlly)
            {
                Effect stunEffect = EffectFactory.CreateStunEffect(skillContext.duration, neighbor.OccupyingCharacter);
                CustomLogger.Log(this, $"try Stun enemy: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(stunEffect);
                neighbor.OccupyingCharacter.AudioManager.PlayCrowdControlledSound();
                CustomLogger.Log(this,$"Stunned enemy: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
            }
            neighbor.SetColorState(ColorState.TemporaryYellow, .5f);
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MineEnhancedSkill(this);
    }
}
public class MineEnhancedSkill : CharacterSkillBase
{
    private MineSkill originalSkill;
    public MineEnhancedSkill(MineSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Mine Skill");
    }
}
public class MomoiSkill : CharacterSkillBase//對範圍內敵人進行一次掃射
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MomoiEnhancedSkill(this);
    }
}
public class MomoiEnhancedSkill : CharacterSkillBase
{
    private MomoiSkill originalSkill;
    public MomoiEnhancedSkill(MomoiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Momoi Skill");
    }
}
public class NonomiSkill : CharacterSkillBase
{
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
public class NonomiEnhancedSkill : CharacterSkillBase
{
    private NonomiSkill originalSkill;
    public NonomiEnhancedSkill(NonomiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Nonomi Skill");
    }
}
public class ShirokoSkill : CharacterSkillBase//一個無人機攻擊若干次
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        ShirokoActiveSkill s = skillContext.Parent.GetComponent<ShirokoActiveSkill>();
        Shiroko_Terror_DroneCTRL d = s.droneCTRL;
        if (d == null)
        {
            s.droneRef = s.GetDrone(skillContext);

            s.droneRef.transform.SetParent(skillContext.Parent.transform, true);
            d = s.droneCTRL = s.droneRef.GetComponent<Shiroko_Terror_DroneCTRL>();
            d.Dmg = skillContext.DamageAmount;
        }
        else
        {
            s.droneRef.SetActive(true);
            d.Dmg += skillContext.DamageAmount;
        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new ShirokoEnhancedSkill(this);
    }
}
public class ShirokoEnhancedSkill : CharacterSkillBase
{
    private ShirokoSkill originalSkill;
    public ShirokoEnhancedSkill(ShirokoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shiroko Skill");
    }
}
public class TsubakiSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        int level = skillContext.CharacterLevel;
        base.ExecuteSkill(skillContext);

        foreach (var item in skillContext.Parent.GetEnemies())
        {
            if (item.CheckEnemyIsInrange(skillContext.Parent))
            {
                Effect effect = EffectFactory.CreateTsubakiFearEffct(0,5,item);

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
public class TsubakiEnhancedSkill : CharacterSkillBase
{
    private TsubakiSkill originalSkill;
    public TsubakiEnhancedSkill(TsubakiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Tsubaki Skill");
    }
}
public class YuukaSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent, 2, false, true, skillContext.currHex);

        if (targetHex != null)
        {
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex, 2, skillContext));//TODO:修改此處，改為傳入skill context的range
        }
    }
    private IEnumerator JumpToTarget(CharacterCTRL character, HexNode targetHex, int range, SkillContext skillContext)
    {
        yield return new WaitForSeconds(12f / 30f);
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
        ShieldAllyAroundHex(targetHex, range, skillContext);
    }
    private void ShieldAllyAroundHex(HexNode targetHex, int range, SkillContext skillContext)
    {
        foreach (HexNode neighbor in SpawnGrid.Instance.GetHexNodesWithinRange(targetHex, range))
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly == skillContext.Parent.IsAlly)
            {
                neighbor.OccupyingCharacter.AddShield(50, 5.0f, skillContext.Parent);
            }
        }
        skillContext.Parent.AddShield(50, 5.0f, skillContext.Parent);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new YuukaEnhancedSkill(this);
    }
}
public class YuukaEnhancedSkill : CharacterSkillBase
{
    private YuukaSkill originalSkill;
    public YuukaEnhancedSkill(YuukaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Yuuka Skill");
    }
}
public class HinaSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        //在barrage之中完成了。
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HinaEnhancedSkill(this);
    }
}
public class HinaEnhancedSkill : CharacterSkillBase
{
    private HinaSkill originalSkill;
    public HinaEnhancedSkill(HinaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hina Skill");
    }
}
public class HoshinoSkill : CharacterSkillBase
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
public class HoshinoEnhancedSkill : CharacterSkillBase
{
    private HoshinoSkill originalSkill;
    public HoshinoEnhancedSkill(HoshinoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hoshino Skill");
    }
}
public class MikaSkill : CharacterSkillBase//對一個人狙擊。此次攻擊必定爆擊，且所有爆擊率轉為爆擊數值，若此技能擊殺了敵人，會以相同傷害在一格之內爆炸。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL C = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.Health, false, 1)[0];
        //TODO: 尚未完成。
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(C.transform.position, skillContext.Parent.GetStat(StatsType.Attack), skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, C.gameObject);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MikaEnhancedSkill(this);
    }
}
public class MikaEnhancedSkill : CharacterSkillBase
{
    private MikaSkill originalSkill;
    public MikaEnhancedSkill(MikaSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Mika Skill");
    }
}
public class NeruSkill : CharacterSkillBase//以超多段傷害攻擊一名敵人
{
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
public class NeruEnhancedSkill : CharacterSkillBase
{
    private NeruSkill originalSkill;
    public NeruEnhancedSkill(NeruSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        // 強化後的Aris技能邏輯
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Neru Skill");
    }
}
public class TsurugiSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        TsurugiActiveSkill T = skillContext.Parent.GetComponent<TsurugiActiveSkill>();
        T.ChangeToSpecialAttack();
        T.SpecialAttackCount = 5;
        //在專屬代碼內完成了。
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new TsurugiEnhancedSkill(this);
    }
}
public class TsurugiEnhancedSkill : CharacterSkillBase
{
    private TsurugiSkill originalSkill;
    public TsurugiEnhancedSkill(TsurugiSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        // 強化後的Aris技能邏輯
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Tsurugi Skill");
    }
}
public class WakamoSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        CharacterCTRL c = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, true, 1)[0];
        if (c!= null)
        {
            Effect effect = EffectFactory.CreateWakamoEffect(0, 5, skillContext.Parent);
            c.effectCTRL.AddEffect(effect);
            GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
            bullet.GetComponent<NormalBullet>().Initialize(c.transform.position, skillContext.Parent.GetStat(StatsType.Attack), skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, c.gameObject);

        }
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new WakamoEnhancedSkill(this);
    }
}
public class WakamoEnhancedSkill : CharacterSkillBase
{
    private WakamoSkill originalSkill;
    public WakamoEnhancedSkill(WakamoSkill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        // 強化後的Aris技能邏輯
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Aris Skill");
    }
}
public class Shiroko_TerrorSkill : CharacterSkillBase
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Shiroko_Terror Skill");
    }
}
public class Atsuko_Skill:CharacterSkillBase//召喚給友軍回血的無人機
{
    public GameObject Drone;
    public Atsuko_Drone Atsuko_DroneRef;

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
public class AtsukoEnhancedSkill : CharacterSkillBase
{
    private Atsuko_Skill originalSkill;
    public AtsukoEnhancedSkill(Atsuko_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Atsuko Skill");
    }
}
public class Hiyori_Skill : CharacterSkillBase//對當前目標發射子彈，命中第一個敵人之後在2格範圍內爆炸並降低防禦
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        var bulletComponent = bullet.GetComponent<NormalBullet>();
        var targetCtrl = skillContext.Parent.Target.GetComponent<CharacterCTRL>();
        bulletComponent.Initialize(targetCtrl.GetHitPoint.position, 100, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 20, targetCtrl.gameObject, false);

    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new HiyoriEnhancedSkill(this);
    }
}
public class HiyoriEnhancedSkill : CharacterSkillBase
{
    private Hiyori_Skill originalSkill;
    public HiyoriEnhancedSkill(Hiyori_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Hiyori Skill");
    }
}
public class Misaki_Skill : CharacterSkillBase//普通攻擊為很慢的大砲，會在地上留下碎片，施放技能時，在地上留下更多碎片，然後引爆。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MisakiEnhancedSkill(this);
    }
}
public class MisakiEnhancedSkill : CharacterSkillBase
{
    private Misaki_Skill originalSkill;
    public MisakiEnhancedSkill(Misaki_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Misaki Skill");
    }
}
public class Miyako_Skill : CharacterSkillBase//招喚無人機，可以進行單點控制，破甲
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Miyako Skill");
    }
}
public class Miyu_Skill : CharacterSkillBase//美遊不會被任何目標鎖定，但是會被範圍技能傷害，施放技能時，對本局造成最多傷害的敵人進行狙擊，其餘友軍已經死亡時，美遊將恢復被可以鎖定的狀態
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new MiyuEnhancedSkill(this);
    }
}
public class MiyuEnhancedSkill : CharacterSkillBase
{
    private Miyu_Skill originalSkill;
    public MiyuEnhancedSkill(Miyu_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Miyu Skill");
    }
}
public class Moe_Skill : CharacterSkillBase //對矩形範圍內造成dot傷害
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Moe Skill");
    }
}
public class Saki_Skill : CharacterSkillBase//對範圍內敵人造成傷害&暈眩
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
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
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Saki Skill");
    }
}
public class Saori_Skill : CharacterSkillBase//開一槍，不會被任何物體抵擋，中間貫穿會損失一部分傷害，最低降至原傷害60%
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
    public override CharacterSkillBase GetHeroicEnhancedSkill()
    {
        return new SaoriEnhancedSkill(this);
    }
}
public class SaoriEnhancedSkill : CharacterSkillBase
{
    private Saori_Skill originalSkill;
    public SaoriEnhancedSkill(Saori_Skill originalSkill)
    {
        this.originalSkill = originalSkill;
    }

    public override void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} cast ENHANCED Saori Skill");
    }
}
public class StarLevelStats
{
    public int Data1;
    public int Data2;
    public int Data3;
    public int Data4;
    public int Data5;

    public StarLevelStats(int data1 = 0, int data2 = 0, int data3 = 0, int data4 = 0, int data5 = 0)
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
