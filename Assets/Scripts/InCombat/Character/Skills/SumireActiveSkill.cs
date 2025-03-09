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
        GameObject obj = skillContext.Parent.gameObject;
        CharacterCTRL enemy = parent.Target != null ? parent.Target.GetComponent<CharacterCTRL>() : null;
        (List<HexNode> bestnodes, HexNode oppositeNode, int count, int dir)
            = Utility.FindMaxOccupantArcNode(parent, false);
        if (oppositeNode == null)
        {
            CustomLogger.LogWarning(this, "OppositeNode not found. Skill aborted.");
            yield break;
        }
        HexNode nextOppositeNode = Utility.GetNeighbor(oppositeNode, (dir + 3) % 6);
        if (nextOppositeNode == null)
        {
            CustomLogger.LogWarning(this, "NextOppositeNode not found. Skill aborted.");
            yield break;
        }
        yield return MoveBetweenNodes(obj, obj.transform.position, nextOppositeNode.Position, 21);
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);
        yield return MoveBetweenNodes(obj, obj.transform.position, oppositeNode.Position, 20);
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);
        yield return MoveBetweenNodes(obj, obj.transform.position, nextOppositeNode.Position, 20);
        ShootAllEnemiesInBestNodes(bestnodes, skillContext);
    }

    /// <summary>
    /// 在指定 frame 數內，線性移動角色至目標點
    /// </summary>
    private IEnumerator MoveBetweenNodes(GameObject obj, Vector3 startPos, Vector3 endPos, int totalFrames)
    {
        HexNode h =  Utility.GetHexOnPos(startPos);
        h.Release();
        CustomLogger.Log(this, $"character {obj} from {startPos} moving to {endPos} in {totalFrames} frames");
        if (obj == null) yield break;
        for (int i = 0; i <= totalFrames; i++)
        {
            if (obj == null) yield break;
            float t = (float)i / totalFrames;
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return new WaitForSeconds(1f / 30f);
        }
        HexNode he = Utility.GetHexOnPos(endPos);
        he.Reserve(obj.GetComponent<CharacterCTRL>());
    }


    /// <summary>
    /// 對 bestnodes 所代表的所有節點上之敵人各射擊一次
    /// </summary>
    private void ShootAllEnemiesInBestNodes(List<HexNode> bestnodes, SkillContext skillContext)
    {
        if (bestnodes == null || bestnodes.Count == 0) return;
        HexNode nearestNode = null;
        float minDistance = float.MaxValue;
        Vector3 parentPos = skillContext.Parent.transform.position;
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
        if (nearestNode != null)
        {
            Vector3 dir = nearestNode.Position - parentPos;
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                skillContext.Parent.transform.rotation = Quaternion.LookRotation(dir);
            }

            CustomLogger.Log(this, $"Nearest node: {nearestNode.name}, distance = {minDistance}");
        }
        else
        {
            CustomLogger.LogWarning(this, "No valid nearest node found in bestnodes.");
            return;
        }
        foreach (HexNode node in bestnodes)
        {
            if (node == null) continue;
            var occupant = node.OccupyingCharacter;
            if (occupant == null) continue;

            CharacterCTRL occupantCtrl = occupant.GetComponent<CharacterCTRL>();
            if (occupantCtrl == null) continue;
            if (occupantCtrl.IsAlly == skillContext.Parent.IsAlly) continue;
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
}
