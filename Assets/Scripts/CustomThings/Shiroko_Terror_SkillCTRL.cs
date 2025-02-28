using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class Shiroko_Terror_SkillCTRL : MonoBehaviour
{
    public int forcepick;
    private Dictionary<int, I_Shiroko_Terror_Skill> skillDictionary;
    public GameObject Grenade;
    public GameObject Drone;
    public Shiroko_Terror_DroneCTRL droneCTRL;
    private SkillContext skillContext;
    public GameObject droneRef;
    public void Start()
    {
        // 初始化技能字典
        skillDictionary = new Dictionary<int, I_Shiroko_Terror_Skill>
        {
            { 2, new GrenadeSkill() },
            { 3, new DroneSummonSkill() },
            { 4, new StrongerDroneSummonSkill() },
            { 5, new HoshinoMimicSkill() },
            { 6, new DropBelow70Skill() },
            { 7, new NonomiStrafingSkill() },
            { 8, new DropBelow30Skill() },
            { 9, new WakamoMimicSkill() },
            { 10, new UncertainSkill() }
        };
        Debug.Log("[Shiroko_Terror_SkillCTRL] 初始化技能字典完成");
    }
    public int ChooseSkill(SkillContext skillContext)
    {
        Debug.Log($"[Shiroko_Terror_SkillCTRL] 開始選擇技能, forcepick: {forcepick}");
        int chosenSkillId = GetForcedSkillId() ?? Choose(skillContext);
        if (skillDictionary.TryGetValue(chosenSkillId, out I_Shiroko_Terror_Skill skill))
        {
            Debug.Log($"[Shiroko_Terror_SkillCTRL] 執行技能 ID: {chosenSkillId}");
            skill.Execute(skillContext);
        }
        else
        {
            Debug.LogWarning($"[Shiroko_Terror_SkillCTRL] 未找到技能 ID: {chosenSkillId}");
        }

        return chosenSkillId;
    }

    private int? GetForcedSkillId()
    {
        Debug.Log($"[Shiroko_Terror_SkillCTRL] 檢查 forcepick: {forcepick}");

        switch (forcepick)
        {
            case 1:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 強制選擇技能 ID: 6");
                return 6;
            case 2:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 強制選擇技能 ID: 8");
                return 8;
            default:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 無強制選擇");
                return null;
        }
    }

    public int Choose(SkillContext skillContext)
    {
        HexNode centerNode = skillContext.currHex;
        bool isAlly = skillContext.Parent.IsAlly;
        CharacterCTRL parent = skillContext.Parent;
        HexNode bestNode = SpawnGrid.Instance.FindBestHexNode(parent, 2, true, false, centerNode);

        Debug.Log($"[Shiroko_Terror_SkillCTRL] 執行 Choose 方法, 父角色: {parent.name}, 中心節點: {centerNode.name}");
        if (SpawnGrid.Instance.GetCharactersWithinRadius(centerNode, isAlly, 2, true, parent).Count >= 2)
        {
            Debug.Log("[Shiroko_Terror_SkillCTRL] 條件達成: 中心節點附近角色數量 >= 2，選擇技能 ID: 5");
            return 5;
        }
        if (parent.GetEnemies().Count >= 5)
        {
            Debug.Log("[Shiroko_Terror_SkillCTRL] 條件達成: 敵人數量 >= 5，選擇技能 ID: 7");
            return 7;
        }
        if (SpawnGrid.Instance.GetCharactersWithinRadius(bestNode, isAlly, 2, true, parent).Count >= 3)
        {
            skillContext.TargetHex = bestNode;
            Debug.Log("[Shiroko_Terror_SkillCTRL] 條件達成: 最佳節點附近角色數量 >= 3，選擇技能 ID: 2");
            return 2;
        }
        if (parent.GetEnemies().Count == 1)
        {
            Debug.Log("[Shiroko_Terror_SkillCTRL] 條件達成: 敵人數量 == 1，選擇技能 ID: 9");
            return 9;
        }

        // 預設邏輯處理
        int defaultSkillId = GetDefaultSkillId();
        Debug.Log($"[Shiroko_Terror_SkillCTRL] 預設選擇技能 ID: {defaultSkillId}");
        return defaultSkillId;
    }

    private int GetDefaultSkillId()
    {
        Debug.Log($"[Shiroko_Terror_SkillCTRL] 檢查預設選擇 forcepick: {forcepick}");

        switch (forcepick)
        {
            case 0:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 預設選擇技能 ID: 3");
                return 3;
            case 1:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 預設選擇技能 ID: 4");
                return 4;
            default:
                Debug.Log("[Shiroko_Terror_SkillCTRL] 預設選擇技能 ID: 2");
                return 2;
        }
    }
    public GameObject GetGrenade(SkillContext skillContext)
    {
        return Instantiate(Grenade, skillContext.Parent.transform.position, Quaternion.identity);
    }
    public GameObject GetDrone(SkillContext skillContext)
    {
        return Instantiate(Drone, skillContext.Parent.transform.position, Quaternion.identity);
    }
    public void HoshinoMimicSingleshot()
    {
        foreach (var item in skillContext.SelectedHex)
        {
            if (item.OccupyingCharacter != null)
            {
                int dmg = (int)skillContext.Parent.GetStat(GameEnum.StatsType.Attack);
                (bool,int) tuple = skillContext.Parent.CalculateCrit(dmg);
                item.OccupyingCharacter.GetHit(tuple.Item2, skillContext.Parent, "HoshinoMimicSingleshot()",tuple.Item1);
            }
        }
    }
    public void UpdateSkillContext(SkillContext context)
    {
        skillContext = context;
    }
}

public interface I_Shiroko_Terror_Skill
{
    void Execute(SkillContext skillContext);
}
public class GrenadeSkill : I_Shiroko_Terror_Skill//2
{
    public void Execute(SkillContext skillContext)
    {
        Debug.Log($"{skillContext.Parent.characterStats.name} 使用了手榴彈技能！");

        Shiroko_Terror_SkillCTRL skillController = skillContext.Parent.GetComponent<Shiroko_Terror_SkillCTRL>();
        GameObject grenadeInstance = skillController.GetGrenade(skillContext);

        // 投掷手榴弹到目标位置
        Vector3 targetPosition = skillContext.TargetHex.Position;
        float parabolaHeight = 2f; // 可调节的抛物线高度
        float flightDuration = 1f; // 飞行持续时间
        skillController.StartCoroutine(ThrowGrenade(grenadeInstance, targetPosition, parabolaHeight, flightDuration, skillContext));
    }
    private IEnumerator ThrowGrenade(GameObject grenadeInstance, Vector3 targetPosition, float parabolaHeight, float flightDuration, SkillContext skillContext)
    {
        yield return new WaitForSeconds(0.9f);
        Vector3 startPosition = grenadeInstance.transform.position;
        float timeElapsed = 0f;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float adjustedDuration = flightDuration + (distance * 0.1f);

        while (timeElapsed < adjustedDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / adjustedDuration;
            Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            float parabolicHeight = parabolaHeight * 4 * (progress - progress * progress);
            grenadeInstance.transform.position = new Vector3(linearPosition.x, linearPosition.y + parabolicHeight, linearPosition.z);

            yield return null;
        }
        grenadeInstance.transform.position = targetPosition;
        int dmg = (int)skillContext.Parent.GetStat(GameEnum.StatsType.Attack);
        (bool,int) tuple = skillContext.Parent.CalculateCrit(dmg);
        foreach (var item in skillContext.TargetHex.Neighbors)
        {
            if (item.OccupyingCharacter!= null && item.OccupyingCharacter.IsAlly!= skillContext.Parent.IsAlly)
            {
                item.OccupyingCharacter.GetHit(tuple.Item2, skillContext.Parent, "Shiroko_Terror_ThrowGrenade()", tuple.Item1);
            }
        }
    }
}

public class DroneSummonSkill : I_Shiroko_Terror_Skill//3
{
    private const int normDroneDmg = 10;
    public void Execute(SkillContext skillContext)
    {

        Shiroko_Terror_SkillCTRL skillController = skillContext.Parent.GetComponent<Shiroko_Terror_SkillCTRL>();
        Shiroko_Terror_DroneCTRL d = skillController.droneCTRL;
        if (skillController.droneCTRL == null)
        {
            skillController.droneRef = skillController.GetDrone(skillContext);

            skillController.droneRef.transform.SetParent(skillContext.Parent.transform,true);
            skillController.droneCTRL = skillController.droneRef.GetComponent<Shiroko_Terror_DroneCTRL>();
            skillController.droneCTRL.stack = 1;
        }
        else
        {
            skillController.droneRef.SetActive(true);
            skillController.droneCTRL.stack = 1;
        }
        Debug.Log($"{skillContext.Parent.characterStats.name} 使用了召喚無人機技能！");
    }
}
public class StrongerDroneSummonSkill : I_Shiroko_Terror_Skill//4
{
    private const int extraDroneDmg = 10;
    public void Execute(SkillContext skillContext)
    {

        Shiroko_Terror_SkillCTRL skillController = skillContext.Parent.GetComponent<Shiroko_Terror_SkillCTRL>();
        Shiroko_Terror_DroneCTRL d = skillController.droneCTRL;
        if (skillController.droneCTRL == null)
        {
            GameObject drone = skillController.GetDrone(skillContext);
            skillController.droneCTRL = drone.GetComponent<Shiroko_Terror_DroneCTRL>();
            skillController.droneCTRL.stack = 2;
        }
        else
        {
            skillController.droneCTRL.stack += 2; ;
        }
        Debug.Log($"{skillContext.Parent.characterStats.name} 使用了召喚無人機技能！");
    }
}
public class HoshinoMimicSkill : I_Shiroko_Terror_Skill //5
{
    public void Execute(SkillContext skillContext)
    {
        HashSet<CharacterCTRL> enemies = new HashSet<CharacterCTRL>();
        Dictionary<HexNode, int> directionEnemyCount = new Dictionary<HexNode, int>();

        // 找到所有相鄰敵人
        foreach (var neighbor in skillContext.Parent.CurrentHex.Neighbors)
        {
            if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != skillContext.Parent.IsAlly)
            {
                enemies.Add(neighbor.OccupyingCharacter);
            }
        }

        // 計算每個方向的敵人數量
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

        // 找到最佳攻擊方向
        var bestDirection = directionEnemyCount.OrderByDescending(kv => kv.Value).First().Key;
        skillContext.Parent.ForceChangeTarget(bestDirection.OccupyingCharacter);

        // 計算 SelectedHex 的交集
        var bestDirectionNeighbors = new HashSet<HexNode>(bestDirection.Neighbors);
        var currentHexNeighbors = new HashSet<HexNode>(skillContext.Parent.CurrentHex.Neighbors);
        bestDirectionNeighbors.Add(bestDirection);
        // 取交集
        skillContext.SelectedHex = bestDirectionNeighbors.Intersect(currentHexNeighbors).ToList();

        // 更新技能上下文
        Shiroko_Terror_SkillCTRL skillController = skillContext.Parent.GetComponent<Shiroko_Terror_SkillCTRL>();
        skillController.UpdateSkillContext(skillContext);

        CustomLogger.Log(this, $"Best attack direction determined towards {bestDirection.Position} with {directionEnemyCount[bestDirection]} enemies.");
        CustomLogger.Log(this, $"SelectedHex updated with {skillContext.SelectedHex.Count} hexes.");
    }
}


public class DropBelow70Skill : I_Shiroko_Terror_Skill//6
{
    public void Execute(SkillContext skillContext)
    {
        skillContext.Parent.effectCTRL.ClearEffects(GameEnum.SpecialEffectType.Invincible);
        CustomLogger.Log(this, $"{skillContext.Parent.characterStats.name} 的血條第一次降至70%以下！");
    }
}
public class NonomiStrafingSkill : I_Shiroko_Terror_Skill//7
{
    public void Execute(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.characterStats.name} 模仿了nonomi的技能！");
    }
}
public class DropBelow30Skill : I_Shiroko_Terror_Skill//8
{
    public void Execute(SkillContext skillContext)
    {
        skillContext.Parent.effectCTRL.ClearEffects(GameEnum.SpecialEffectType.Invincible);
        CustomLogger.Log(this, $"{skillContext.Parent.characterStats.name} 的血條第一次降至30%以下！");
    }
}
public class WakamoMimicSkill : I_Shiroko_Terror_Skill//9
{
    public void Execute(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"{skillContext.Parent.characterStats.name} 模仿了若藻的大招！");
    }
}
public class UncertainSkill : I_Shiroko_Terror_Skill//10
{
    public void Execute(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"尚未確定");
    }
}