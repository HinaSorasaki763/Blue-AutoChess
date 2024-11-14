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
        RaycastHit[] results = new RaycastHit[10]; // �w�����t���Ʋ�
        List<RaycastHit> maxHits = new List<RaycastHit>(); // �Ω��x�s�̤j�R���ƪ��ĤH
        Vector3 maxDirection = Vector3.zero; // �s�W�@���ܶq���x�s��V
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
                maxDirection = direction; // ��s�̤j�R���ƪ���V
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
            bullet.GetComponent<TrailedBullet>().Initialized(skillContext.Parent.GetCollidPos() + maxDirection * 50, skillContext.DamageAmount, decayfactor, layer, skillContext.Parent,false);//TODO: �קאּ���T�������v
        }
        transform.LookAt(maxDirection);
    }
}
