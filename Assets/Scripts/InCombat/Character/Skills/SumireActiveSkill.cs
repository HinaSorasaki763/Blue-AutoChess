using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumireActiveSkill : MonoBehaviour
{
    public CharacterCTRL parent;

    /// <summary>
    /// 整個技能的執行流程，按照特定 frame 或動畫 Sample 進行移動和射擊
    /// </summary>
    public IEnumerator SkillRoutine(SkillContext skillContext)
    {
        // 檢查必要參考
        if (parent == null)
        {
            CustomLogger.LogWarning(this, "Parent is null. Cannot execute skill.");
            yield break;
        }

        // 施法者物件
        GameObject obj = skillContext.Parent.gameObject;

        // 取得目標集合（如果後續要用 bestnodes 的 occupant 進行攻擊，這裡的 enemy 就視需求是否要保留）
        CharacterCTRL enemy = parent.Target != null ? parent.Target.GetComponent<CharacterCTRL>() : null;

        // 找出最佳攻擊弧線上的節點
        (List<HexNode> bestnodes, HexNode oppositeNode, int count, int dir)
            = Utility.FindMaxOccupantArcNode(parent, false);
        if (oppositeNode == null)
        {
            CustomLogger.LogWarning(this, "OppositeNode not found. Skill aborted.");
            yield break;
        }

        // nextOppositeNode = 以 oppositeNode 為中心，(dir + 3) % 6 方向的相鄰節點
        HexNode nextOppositeNode = Utility.GetNeighbor(oppositeNode, (dir + 3) % 6);
        if (nextOppositeNode == null)
        {
            CustomLogger.LogWarning(this, "NextOppositeNode not found. Skill aborted.");
            yield break;
        }
        yield return MoveBetweenNodes(obj, obj.transform.position, nextOppositeNode.Position, 21);

        // 移動完成後，對 bestnodes 上所有敵人進行射擊
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);

        // 第 30～50 帧：移動到 oppositeNode
        yield return MoveBetweenNodes(obj, obj.transform.position, oppositeNode.Position, 20);

        // 移動完成後，對 bestnodes 上所有敵人進行射擊
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);

        // 第 50～70 帧：移動回 nextOppositeNode
        yield return MoveBetweenNodes(obj, obj.transform.position, nextOppositeNode.Position, 20);

        // 移動完成後，對 bestnodes 上所有敵人進行射擊
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);
    }

    /// <summary>
    /// 在指定 frame 數內，線性移動角色至目標點
    /// </summary>
    private IEnumerator MoveBetweenNodes(GameObject obj, Vector3 startPos, Vector3 endPos, int totalFrames)
    {
        CustomLogger.Log(this, $"character {obj} from {startPos} moving to {endPos} in {totalFrames} frames");

        if (obj == null) yield break;

        // 這種寫法：最後一圈 i = totalFrames 時， t = 1.0f，能移動到 endPos
        for (int i = 0; i <= totalFrames; i++)
        {
            if (obj == null) yield break;

            float t = (float)i / totalFrames;
            // ↑ 由 0 緩慢變到 1，移動完畢時就會抵達 endPos

            obj.transform.position = Vector3.Lerp(startPos, endPos, t);

            // 每 1/30 秒跑一次，模擬 30 FPS（如果真實 FPS 非 30，則與畫面更新會錯開）
            yield return new WaitForSeconds(1f / 30f);
        }
    }


    /// <summary>
    /// 對 bestnodes 所代表的所有節點上之敵人各射擊一次
    /// </summary>
    private void ShootAllEnemiesInBestNodes(List<HexNode> bestnodes, SkillContext skillContext)
    {
        // 如果 bestnodes 為空或 null 就無需執行
        if (bestnodes == null || bestnodes.Count == 0) return;

        // 1) 找出距離角色最近的節點（不管該節點上是否有敵人）
        HexNode nearestNode = null;
        float minDistance = float.MaxValue;

        // 角色當前位置
        Vector3 parentPos = skillContext.Parent.transform.position;

        // 遍歷所有 bestnodes，找出最近的
        foreach (HexNode node in bestnodes)
        {
            if (node == null) continue;

            float dist = Vector3.Distance(parentPos, node.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestNode = node;
            }
        }

        // 2) 讓角色面向最近的節點
        if (nearestNode != null)
        {
            Vector3 dir = nearestNode.Position - parentPos;
            dir.y = 0f; // 僅在水平面旋轉
            if (dir != Vector3.zero)
            {
                skillContext.Parent.transform.rotation = Quaternion.LookRotation(dir);
            }

            CustomLogger.Log(this, $"Nearest node: {nearestNode.name}, distance = {minDistance}");
        }
        else
        {
            // 如果所有節點都是 null，那就無需繼續
            CustomLogger.LogWarning(this, "No valid nearest node found in bestnodes.");
            return;
        }

        // 3) 接著再針對 bestnodes 的所有敵人進行一次射擊 (散彈槍邏輯)
        foreach (HexNode node in bestnodes)
        {
            if (node == null) continue;

            // 取得此節點上的角色
            var occupant = node.OccupyingCharacter;
            if (occupant == null) continue;

            CharacterCTRL occupantCtrl = occupant.GetComponent<CharacterCTRL>();
            if (occupantCtrl == null) continue;

            // 排除己方
            if (occupantCtrl.IsAlly == skillContext.Parent.IsAlly) continue;

            // 對該角色造成傷害
            DoShoot(skillContext, occupantCtrl);
        }
    }


    /// <summary>
    /// 單次射擊傷害
    /// </summary>
    private void DoShoot(SkillContext skillContext, CharacterCTRL target)
    {
        if (target == null) return;

        int dmg = parent.ActiveSkill.GetAttackCoefficient(skillContext);
        (bool iscrit, int dmg1) = parent.CalculateCrit(dmg);
        target.GetHit(dmg1, parent, DamageSourceType.Skill.ToString(), iscrit);

        CustomLogger.Log(this, $"DoShoot() -> Target:{target.name}, Damage:{dmg1}, Crit:{iscrit}");
    }

    /// <summary>
    /// 等待指定的 frame 數
    /// </summary>
    private IEnumerator WaitFrames(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForSeconds(1/30f);
        }
    }
}
