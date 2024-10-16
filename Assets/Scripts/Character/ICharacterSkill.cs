
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;
public abstract class CharacterSkillBase
{
    public virtual void ExecuteSkill(SkillContext skillContext)
    {
        Debug.Log($"{skillContext.Parent.gameObject.name} castSkill");
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
        HealPack.GetComponent<HealPack>().InitStats(targetHex, skillContext.Range, 100);
    }
}
public class HarukaSkill : CharacterSkillBase//架起護盾，並向前攻擊數波，她前方的敵軍將會被減少攻擊
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
        Debug.Log($"hexNode = {hexNode.Position}, currentHex = {currentHex.Position},Count = {commonNeighbors.Count}");
        foreach (var neighbor in commonNeighbors)
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != isAlly)
            {
                neighbor.OccupyingCharacter.GetHit(10,skillContext.Parent);
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
        int lowestHp = int.MaxValue;
        CharacterCTRL lowestEnemy = null;
        Debug.Log($"skillContext.Enemies.count = {skillContext.Enemies.Count}");
        foreach (var item in skillContext.Enemies)
        {
            if (item.gameObject.activeInHierarchy&& item.GetStat(StatsType.currHealth) < lowestHp)
            {
                lowestEnemy = item;
            }
        }
        Debug.Log($"lowestEnemy = {lowestEnemy.transform.position}");
        skillContext.Parent.transform.LookAt(lowestEnemy.GetHitPoint);
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        bullet.GetComponent<NormalBullet>().Initialize(lowestEnemy.transform.position,dmg,layer,skillContext.Parent, 15f);
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
        bullet.GetComponent<NormalBullet>().Initialize(target.transform.position, BaseDmg, layer, skillContext.Parent, 20f);
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
        skillContext.Parent.AddStat(StatsType.currHealth,BaseHeal);
        //TODO:尚未實現控制，等到實現控制再回來補足免疫控制(natsu)
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
        //等到控制類一併實現(Noa)
    }
}
public class SerikaSkill : CharacterSkillBase//增加自己一些攻擊力、攻擊速度
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
            {1, new StarLevelStats(10,1,1)},
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

        List<CharacterCTRL> allies = new List<CharacterCTRL>(skillContext.Allies);
        allies.Sort((a, b) => a.GetStat(StatsType.currHealth).CompareTo(b.GetStat(StatsType.currHealth)));

        List<CharacterCTRL> lowestHpAllies = new List<CharacterCTRL>();
        int actualBoxAmount = Math.Min(boxAmount, allies.Count);

        for (int i = 0; i < actualBoxAmount; i++)
        {
            lowestHpAllies.Add(allies[i]);
        }

        int remainingBoxes = boxAmount - actualBoxAmount;
        for (int i = 0; i < remainingBoxes; i++)
        {
            Vector3 pos = lowestHpAllies[i % actualBoxAmount].transform.position + new Vector3(0, 3, 0);
            GameObject HealPack = ResourcePool.Instance.SpawnObject(SkillPrefab.HealPack, pos, Quaternion.identity);
            HexNode hex = Utility.GetHexOnPos(pos);
            HealPack.GetComponent<HealPack>().InitStats(hex, skillContext.Range, 100);
        }
    }
}

public class ShizukoSkill : CharacterSkillBase//在角色(無論敵我)最多的空格子放置一台餐車，該餐車"架起護盾"，且護盾範圍+2的範圍內的友軍增加命中率
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class SumireSkill : CharacterSkillBase//翻滾到最好的位置，且射擊一次(記得檢查可不可以通行)。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class AkoSkill : CharacterSkillBase//增加某些人的命中率、爆擊率、爆擊數值
{
    //TODO:等到這些數值實裝
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class AzusaSkill : CharacterSkillBase//對某個生命絕對值最低的人狙擊，若擊殺之則...?
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class ChiseSkill : CharacterSkillBase//對範圍內的格子灑毒，對站在上面的敵人造成dot傷害
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class FuukaSkill : CharacterSkillBase//找到一個範圍內最多友軍的空格，在該格投放治療，範圍內的友軍會被一次性的治癒
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class IzunaSkill : CharacterSkillBase//???
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class KayokoSkill : CharacterSkillBase//對大範圍敵人造成少量傷害及恐懼
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class KazusaSkill : CharacterSkillBase//增加一定攻擊力之後，狙擊絕對生命值最低的敵人
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class MineSkill : CharacterSkillBase//跳躍到敵人最多的位置，擊暈他們
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);

        // Step 1: 尋找範圍內敵人最多的格子
        HexNode targetHex = SpawnGrid.Instance.FindBestHexNode(skillContext.Parent,3,true,true,skillContext.currHex);

        if (targetHex != null)
        {
            // Step 2: 讓角色移動到目標格子
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex,3, skillContext));//TODO:修改此處，改為傳入skill context的range
        }
    }
    // Step 2: 執行跳躍
    private IEnumerator JumpToTarget(CharacterCTRL character, HexNode targetHex,int range, SkillContext skillContext)
    {
        // 等待28幀，假設每秒30幀
        yield return new WaitForSeconds(31f / 30f);
        character.CurrentHex.HardRelease();
        Vector3 startPosition = character.transform.position;
        Vector3 targetPosition = targetHex.Position;
        targetHex.HardReserve(character);
        float jumpDuration = 9f / 30f;
        float elapsedTime = 0f;

        // 跳躍動畫
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
                neighbor.OccupyingCharacter.effectCTRL.AddEffect(stunEffect);
                Debug.Log($"Stunned enemy: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
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
public class NonomiSkill : CharacterSkillBase//對範圍內敵人進行兩次掃射
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class ShirokoSkill : CharacterSkillBase//一個無人機攻擊若干次
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class TsubakiSkill : CharacterSkillBase//架起護盾。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
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
            // Step 2: 讓角色移動到目標格子
            skillContext.Parent.StartCoroutine(JumpToTarget(skillContext.Parent, targetHex, 2, skillContext));//TODO:修改此處，改為傳入skill context的range
        }
    }
    private IEnumerator JumpToTarget(CharacterCTRL character, HexNode targetHex, int range, SkillContext skillContext)
    {
        // 等待28幀，假設每秒30幀
        yield return new WaitForSeconds(31f / 30f);

        // 釋放當前格子
        Debug.Log($"[JumpToTarget] Releasing current hex: {character.CurrentHex.Position}");
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

        // 打印跳躍後的狀態
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[JumpToTarget] Finished jump to target hex: {targetHex.Position}, Reserving character: {character.name}");

        foreach (HexNode neighbor in SpawnGrid.Instance.GetHexNodesWithinRange(targetHex, range))
        {
            sb.AppendLine($"[JumpToTarget] Neighbor Hex: {neighbor.Position}, OccupyingCharacter: {neighbor.OccupyingCharacter}");
        }
        Debug.Log(sb.ToString());

        // 為周圍友軍提供護盾
        ShieldAllyAroundHex(targetHex, range, skillContext);
    }


    private void ShieldAllyAroundHex(HexNode targetHex, int range, SkillContext skillContext)
    {
        bool isAlly = skillContext.Parent.IsAlly;

        StringBuilder sb = new StringBuilder();
        // Log skill context info
        sb.AppendLine($"[ShieldAllyAroundHex]ShieldAllyAroundHex called. Parent: {skillContext.Parent.name}, IsAlly: {isAlly}");

        foreach (HexNode neighbor in SpawnGrid.Instance.GetHexNodesWithinRange(targetHex, range))
        {
            // Log each neighbor's information
            sb.AppendLine($"[ShieldAllyAroundHex]Checking neighbor: {neighbor.Position}, IsBattlefield: {neighbor.IsBattlefield}, OccupyingCharacter: {neighbor.OccupyingCharacter}");

            if (neighbor.OccupyingCharacter != null)
            {
                sb.AppendLine($"[ShieldAllyAroundHex]Neighbor OccupyingCharacter: {neighbor.OccupyingCharacter.name}, IsAlly: {neighbor.OccupyingCharacter.IsAlly}");

                if (neighbor.OccupyingCharacter.IsAlly == isAlly)
                {
                    // 如果符合條件，增加護盾
                    neighbor.OccupyingCharacter.AddShield(50, 5.0f);
                    sb.AppendLine($"[ShieldAllyAroundHex]Shielded Ally: {neighbor.OccupyingCharacter.name} at Hex: {neighbor.Position}");
                }
            }
        }
        Debug.Log( sb.ToString() );
    }

}
public class HinaSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
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
        if (bestDirection.OccupyingCharacter!= null)
        {
            skillContext.Parent.ForceChangeTarget(bestDirection.OccupyingCharacter);
        }

        var bestDirectionNeighbors = new HashSet<HexNode>(bestDirection.Neighbors);
        var currentHexNeighbors = new HashSet<HexNode>(skillContext.Parent.CurrentHex.Neighbors);
        bestDirectionNeighbors.Add(bestDirection);
        skillContext.SelectedHex = bestDirectionNeighbors.Intersect(currentHexNeighbors).ToList();
        CustomLogger.Log(this,$"Hoshino skillContext.SelectedHex = {skillContext.SelectedHex.Count}");
        skillContext.Parent.GetComponent<HoshinoShotgunSkill>().skillContext = skillContext;
    }
}
public class MikaSkill : CharacterSkillBase//對一個人狙擊。此次攻擊必定爆擊，且所有爆擊率轉為爆擊數值，若此技能擊殺了敵人，會以相同傷害在一格之內爆炸。
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class NeruSkill : CharacterSkillBase//以超多段傷害攻擊一名敵人
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class TsurugiSkill : CharacterSkillBase//彈藥數量變少，但一次以更多傷害攻擊扇形範圍內多個目標。後續施放會逐漸增加傷害，每n次施放多1距離
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);

    }
}
public class WakamoSkill : CharacterSkillBase//對同一目標開槍若干次，每一次會暈眩若干秒，最後一次暈眩最久
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
    }
}
public class Shiroko_TerrorSkill : CharacterSkillBase
{
    public override void ExecuteSkill(SkillContext skillContext)
    {
        base.ExecuteSkill(skillContext);
        skillContext.Parent.transform.rotation = Quaternion.Euler(0, 180, 0);
        skillContext.Parent.customAnimator.animator.SetInteger("SkillID",skillContext.shirokoTerror_SkillID);
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

    public List<HexNode> SelectedHex = new ();
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
