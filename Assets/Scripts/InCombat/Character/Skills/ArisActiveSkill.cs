using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArisActiveSkill : MonoBehaviour
{
    public SkillContext skillContext;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ExecuteSkill()
    {
        LayerMask layer = skillContext.Parent.GetTargetLayer();
        int maxHitCount = 0;
        float cannonWidth = 1.0f;
        RaycastHit[] results = new RaycastHit[10]; // 箇だ皌计舱
        List<RaycastHit> maxHits = new List<RaycastHit>(); // ノ纗程㏑い计寄
        Vector3 maxDirection = Vector3.zero; // 穝糤跑秖ㄓ纗よ
        float decayfactor = 90 / 100f;
        foreach (var enemy in skillContext.Enemies)
        {
            Vector3 direction = (enemy.GetCollidPos() - skillContext.Parent.GetCollidPos()).normalized;
            int hitCount = Physics.SphereCastNonAlloc(skillContext.Parent.GetCollidPos(), cannonWidth, direction, results, Mathf.Infinity, layer);
            if (hitCount > maxHitCount)
            {
                maxHitCount = hitCount;
                maxHits.Clear();
                maxHits.AddRange(results.Take(hitCount));
                maxDirection = direction; // 穝程㏑い计よ
            }
        }

        if (maxHitCount > 0)
        {
            GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.PenetrateTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
            bullet.transform.position = skillContext.Parent.FirePoint.position;
            List<CharacterCTRL> enemies = new();
            foreach (RaycastHit hit in maxHits)
            {
                CharacterCTRL hitEnemy = hit.collider.GetComponent<CharacterCTRL>();
                enemies.Add(hitEnemy);
            }
            bullet.GetComponent<TrailedBullet>().Initialized(skillContext.Parent.GetCollidPos() + maxDirection * 50, skillContext.DamageAmount, decayfactor, layer, skillContext.Parent,false);//TODO: эタ絋厨阑瞯
        }
        transform.LookAt(maxDirection);
    }
}
