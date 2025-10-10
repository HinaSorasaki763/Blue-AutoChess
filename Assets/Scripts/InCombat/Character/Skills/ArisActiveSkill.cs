using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArisActiveSkill : MonoBehaviour
{
    public SkillContext SkillContext;
    public ArisSkill ArisSkill;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartEnhanceSkillCorutine()
    {
        ExecuteEnhancedSkill(SkillContext);
    }
    public void ExecuteEnhancedSkill(SkillContext skillContext)
    {
        CustomLogger.Log(this, $"Start ExecuteEnhancedSkill ");
        var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        var bulletComponent = bullet.GetComponent<NormalBullet>();
        int dmg = skillContext.DamageAmount;
        (bool crit1, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        (bool crit2, int dmg2) = skillContext.Parent.CalculateCrit(dmg1);
        bool crit = crit1 || crit2;
        bulletComponent.Initialize( dmg2, skillContext.Parent.GetTargetLayer(),skillContext.Parent, 50f,skillContext.Parent.GetTarget(), true, crit);
        bullet.transform.localScale = Vector3.one * 0.3f;
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
        float beamRadius = 0.5f;

        var allEnemies = Physics.OverlapSphere(parent.position, searchRadius, layer)
            .Select(col => col.GetComponent<CharacterCTRL>())
            .Where(c => c != null && c.isAlive && c.isTargetable && !c.characterStats.logistics)
            .ToList();
        if (allEnemies.Count == 0) return;

        // 將敵人轉為方向向量
        var dirs = allEnemies.Select(e => (e.transform.position - parent.position).normalized).ToList();

        // 計算所有兩兩角度
        float bestAngle = 0f;
        Vector3 bestDir = Vector3.zero;
        int maxHits = 0;

        for (int i = 0; i < dirs.Count; i++)
        {
            for (int j = i + 1; j < dirs.Count; j++)
            {
                float angle = Vector3.Angle(dirs[i], dirs[j]);
                if (angle <= 0f || angle > 180f) continue;

                // 計算夾角中心方向
                Vector3 midDir = (dirs[i] + dirs[j]).normalized;

                // 在夾角範圍內每度檢查一次
                int step = Mathf.CeilToInt(angle);
                for (int a = -step / 2; a <= step / 2; a++)
                {
                    Quaternion rot = Quaternion.AngleAxis(a, Vector3.up);
                    Vector3 checkDir = rot * midDir;

                    var hits = allEnemies.Where(e =>
                    {
                        Vector3 toEnemy = e.transform.position - parent.position;
                        float proj = Vector3.Dot(toEnemy, checkDir);
                        if (proj < 0f || proj > maxDistance) return false;
                        float dist = Vector3.Cross(toEnemy, checkDir).magnitude;
                        return dist <= beamRadius;
                    }).ToList();

                    if (hits.Count > maxHits)
                    {
                        maxHits = hits.Count;
                        bestAngle = a;
                        bestDir = checkDir;
                    }
                }
            }
        }

        if (maxHits == 0) return;

        skillContext.Parent.LockDirection(bestDir);
        GameObject bullet = ResourcePool.Instance.SpawnObject(
            SkillPrefab.PenetrateTrailedBullet,
            skillContext.Parent.FirePoint.position,
            Quaternion.identity
        );
        bullet.GetComponent<TrailedBullet>().Initialized(
            parent.position + bestDir * maxDistance,
            skillContext.DamageAmount,
            ArisSkill.DecayFactor,
            layer,
            skillContext.Parent,
            false
        );
    }

}
