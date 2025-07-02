using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokiActiveskill : MonoBehaviour
{
    public SkillContext SkillContext;
    public Toki_Skill TokiSkill;
    public Transform LaserFirePoint;
    public void Awake()
    {
        
    }
    public void StartSkillCorutine()
    {
        ExecuteSkill(SkillContext);
    }
    public void ExecuteSkill(SkillContext skillContext)
    {
        Transform parent = skillContext.Parent.transform;
        LayerMask layer = skillContext.Parent.GetTargetLayer();

        float searchRadius = 10f;
        float maxDistance = 50f;
        float beamRadius = 1.5f;

        var allEnemies = Physics.OverlapSphere(parent.position, searchRadius, layer)
            .Select(col => col.GetComponent<CharacterCTRL>())
            .Where(c => c != null && c.isAlive && c.isTargetable && !c.characterStats.logistics)
            .ToList();

        if (allEnemies.Count == 0) return;

        Vector3 bestDir = Vector3.zero;
        float bestScore = float.MinValue;

        foreach (var target in allEnemies)
        {
            Vector3 direction = target.transform.position - LaserFirePoint.position;
            direction.y = 0f;
            direction = direction.normalized;

            if (direction.sqrMagnitude < 0.0001f) continue;

            var hits = allEnemies.Where(e =>
            {
                Vector3 toEnemy = e.transform.position - LaserFirePoint.position;
                float proj = Vector3.Dot(toEnemy, direction);
                if (proj < 0f || proj > maxDistance) return false;

                float dist = Vector3.Cross(toEnemy, direction).magnitude;
                return dist <= beamRadius;
            }).ToList();

            if (hits.Count == 0) continue;
            float avgAngle = hits.Average(e =>
            {
                Vector3 toE = (e.transform.position - LaserFirePoint.position).normalized;
                return Vector3.Angle(direction, toE);
            });
            float score = hits.Count - avgAngle * 0.05f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = direction;
            }
        }

        if (bestDir == Vector3.zero) return;

        skillContext.Parent.LockDirection(bestDir);

        GameObject beamObj = ResourcePool.Instance.SpawnObject(
            SkillPrefab.Beam,
            skillContext.Parent.FirePoint.position,
            Quaternion.LookRotation(bestDir)
        );

        Beam beam = beamObj.GetComponentInChildren<Beam>();
        beam.Initialize(
            skillContext.Parent,
            skillContext.Parent.ActiveSkill.GetAttackCoefficient(skillContext),
            2f,
            bestDir,
            LaserFirePoint.position
        );

    }


}
