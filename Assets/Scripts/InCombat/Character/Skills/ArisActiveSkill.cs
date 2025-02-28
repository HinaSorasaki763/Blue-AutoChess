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

        LayerMask layer = skillContext.Parent.GetTargetLayer();
        float searchRadius = 10f;
        float maxDistance = 50f;
        float beamRadius = 0.5f; // 寬度=1 => 半徑=0.5

        Collider[] allEnemyColliders = Physics.OverlapSphere(
            skillContext.Parent.transform.position,
            searchRadius,
            layer
        );

        CustomLogger.Log(this, $"allEnemyColliders count = {allEnemyColliders.Length}");
        List<CharacterCTRL> allEnemies = new List<CharacterCTRL>();
        foreach (var col in allEnemyColliders)
        {
            CharacterCTRL enemy = col.GetComponent<CharacterCTRL>();
            if (enemy != null)
            {
                allEnemies.Add(enemy);
            }
        }
        allEnemies = allEnemies.Where(item => item.isTargetable).Where(item => item.isAlive).Where(item => !item.characterStats.logistics).ToList();
        CustomLogger.Log(this, $"allEnemies count = {allEnemies.Count}");
        if (allEnemies.Count == 0)
        {
            CustomLogger.Log(this, "No enemies found within range.");
            return;
        }
        int maxHitCount = 0;
        Vector3 bestDirection = Vector3.zero;
        List<CharacterCTRL> bestHitEnemies = new();

        Vector3 parentPos = skillContext.Parent.transform.position;

        // 2. 逐一「以 parent -> 某個敵人」方向為候選方向
        foreach (CharacterCTRL enemy in allEnemies)
        {
            Vector3 dirVec = enemy.transform.position - parentPos;
            // 如果剛好在同一個座標，避免 normalized 出現 (0,0,0)
            if (dirVec.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            Vector3 direction = dirVec.normalized; // 單位向量
            List<CharacterCTRL> hitEnemies = new();

            // 3. 檢查所有敵人是否被這條「光束」命中
            foreach (CharacterCTRL e2 in allEnemies)
            {
                Vector3 diff = e2.transform.position - parentPos;

                // dot = 代表「沿著 direction 的位移量」
                float t = Vector3.Dot(diff, direction);

                // 若 dot < 0 => 表示在背後，不算
                // 若 dot > maxDistance => 超出光束最大距離
                if (t < 0f || t > maxDistance)
                {
                    continue;
                }

                // 計算敵人到這條光束的垂直距離
                // dist = magnitude of cross((P - A), d)
                float dist = Vector3.Cross(diff, direction).magnitude;

                if (dist <= beamRadius)
                {
                    hitEnemies.Add(e2);
                }
            }

            // 4. 若命中數量超越當前最佳，更新
            if (hitEnemies.Count > maxHitCount)
            {
                maxHitCount = hitEnemies.Count;
                bestDirection = direction;
                bestHitEnemies = hitEnemies;
            }
        }

        CustomLogger.Log(this, $"Best direction found, max hits = {maxHitCount}");

        // 5. 若找到「最優光束方向」，就發射子彈 (或做其他處理)
        if (maxHitCount > 0)
        {
            // 鎖定朝向
            skillContext.Parent.LockDirection(bestDirection);

            GameObject bullet = ResourcePool.Instance.SpawnObject(
                SkillPrefab.PenetrateTrailedBullet,
                skillContext.Parent.FirePoint.position,
                Quaternion.identity
            );

            bullet.transform.position = skillContext.Parent.FirePoint.position;
            float decayFactor = ArisSkill.DecayFactor;

            bullet.GetComponent<TrailedBullet>().Initialized(
                skillContext.Parent.transform.position + bestDirection * maxDistance,
                skillContext.DamageAmount,
                decayFactor,
                layer,
                skillContext.Parent,
                false
            );
        }
    }

}
