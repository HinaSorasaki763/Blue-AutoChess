using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class NeruActiveSkill : MonoBehaviour
{
    public CharacterCTRL parent;
    public void OnEnable()
    {
        parent = gameObject.GetComponent<CharacterCTRL>();
    }
    public void SkillAttack()
    {

        var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, parent.FirePoint.position, Quaternion.identity);
        var bulletComponent = bullet.GetComponent<NormalBullet>();
        int dmg = parent.ActiveSkill.GetAttackCoefficient(parent.GetSkillContext());
        CustomLogger.Log(this, $"Neru Skill Attack: {dmg}");
        (bool, int) tuple = parent.CalculateCrit(dmg);
        bool iscrit = tuple.Item1;
        dmg = tuple.Item2;
        GameObject go = null;
        if (Utility.GetNearestEnemy(parent) != null)
        {
            go = Utility.GetNearestEnemy(parent).gameObject;
            parent.transform.LookAt(go.transform.position);
        }
        List<HitEffect> hitEffect = new List<HitEffect>();

        if (GameController.Instance.CheckCharacterEnhance(28, parent.IsAlly))
        {
            if (go != null)
            {
                hitEffect.Add(new NeruSkillEffect(go.GetComponent<CharacterCTRL>()));
            }

        }
        bulletComponent.Initialize(dmg, parent.GetTargetLayer(), parent, 20, go, false, iscrit, hitEffect);
        parent.HandleAttacking(false);
    }
}
