
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;
public abstract class CharacterSkillBase
{
    public virtual void ExecuteSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.gameObject.name} castSkill");
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
            // 检查起始节点上的角色是否满足条件
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
                Effect effect = EffectFactory.CreateHarukaMinusAtkEffect(5, 5);
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(effect);
            }
        }
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
        Effect effect = EffectFactory.ClarityEffect(5);
        skillContext.Parent.effectCTRL.AddEffect(effect);
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
        Effect effect = EffectFactory.CreateMarkedEffect(level);

        CharacterCTRL StrongestEnemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.Attack, false, 1)[0];
        StrongestEnemy.effectCTRL.AddEffect(effect);
        foreach (var item in skillContext.Parent.GetAllies())
        {
            if (item.CheckEnemyIsInrange(StrongestEnemy))
            {
                item.ForceChangeTarget(StrongestEnemy);
            }
        }
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
        Effect effect = EffectFactory.CreateSerikaRageEffect(10, 5);
        skillContext.Parent.effectCTRL.AddEffect(effect);
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
            Effect effect = EffectFactory.CreateShizukoEffect(30, 10);
            item.effectCTRL.AddEffect(effect);
        }
    }
}
public class SumireSkill : CharacterSkillBase//TODO:翻滾到最好的位置，且射擊一次(記得檢查可不可以通行)。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);
    }
}
public class AkoSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);
        
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
}
public class ChiseSkill : CharacterSkillBase//對範圍內的格子灑毒，對站在上面的敵人造成dot傷害
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);
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
}
public class IzunaSkill : CharacterSkillBase//???
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        //TODO: 尚未完成
        base.ExecuteSkill(skillContext);

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
            Effect kayokoFearEffect = EffectFactory.CreateKayokoFearEffct(1, 5);
            item.effectCTRL.AddEffect(kayokoFearEffect);
        }
    }
}
public class KazusaSkill : CharacterSkillBase//增加一定攻擊力之後，狙擊絕對生命值最低的敵人
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        Effect kazusaAttackEffect = EffectFactory.StatckableIncreaseStatsEffct(5,"Kazusa",50,StatsType.Attack);
        skillContext.Parent.effectCTRL.AddEffect(kazusaAttackEffect);
        CharacterCTRL lowestHpenemy = Utility.GetSpecificCharacters(skillContext.Parent.GetEnemies(), StatsType.currHealth, false, 1)[0];
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(lowestHpenemy.transform.position, skillContext.Parent.GetStat(StatsType.Attack),skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, lowestHpenemy.gameObject);
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
                Effect stunEffect = EffectFactory.CreateStunEffect(skillContext.duration);
                CustomLogger.Log(this, $"try Stun enemy: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(stunEffect);
                neighbor.OccupyingCharacter.AudioManager.PlayCrowdControlledSound();
                CustomLogger.Log(this,$"Stunned enemy: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
            }
            neighbor.SetColorState(ColorState.TemporaryYellow, .5f);
        }
    }

}
public class MomoiSkill : CharacterSkillBase//對範圍內敵人進行一次掃射
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class NonomiSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        //Finished in Barrage observer 
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
                Effect effect = EffectFactory.CreateTsubakiFearEffct(0,5);

                item.ForceChangeTarget(skillContext.Parent);
                item.effectCTRL.AddEffect(effect);
            }
        }
    }
}
public class YuukaSkill : CharacterSkillBase//跳到敵人最多的地方，同時找到某個相鄰友軍最多的空格子，插旗子。賦予旗子旁的友軍護盾。
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
        yield return new WaitForSeconds(31f / 30f);
        HexNode hex = character.CurrentHex;
        character.CurrentHex.HardRelease();

        Vector3 startPosition = character.transform.position;
        Vector3 targetPosition = targetHex.Position;

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

}
public class HinaSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        //在barrage之中完成了。
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
}
public class NeruSkill : CharacterSkillBase//以超多段傷害攻擊一名敵人
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        skillContext.Parent.ManaLock = true;
        //在動畫事件內完成了
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
}
public class Shiroko_TerrorSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        skillContext.Parent.transform.rotation = Quaternion.Euler(0, 180, 0);
        skillContext.Parent.customAnimator.animator.SetInteger("SkillID", skillContext.shirokoTerror_SkillID);
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
