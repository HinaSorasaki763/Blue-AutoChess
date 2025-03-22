using GameEnum;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    private static int nextId = 0;
    private int attackId;
    public float speed = 20f;
    public GameObject Target;
    public int damage;
    public LayerMask hitLayer;
    public LayerMask WallLayer;
    public CharacterCTRL parent;
    private Vector3 startPosition; // 記錄子彈起始位置
    private float maxDistance; // 最大距離
    private bool isCrit;
    private bool isSkillBullet;
    private List<HitEffect> hitEffects = new List<HitEffect>();
    private bool isBarrage;
    private Vector3 targetPos;
    void Awake()
    {
        attackId = nextId++;
    }
    void OnEnable()
    {
        // 每次啟用時清除 Trail Renderer
        var trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    public void Initialize(int damage, LayerMask hitLayer, CharacterCTRL parent, float maxDistance, GameObject Target, bool isSkillBullet, bool iscrit, List<HitEffect> effects = null, int speed = 20, bool barrage = false, Vector3 v = default)
    {
        transform.position = parent.FirePoint.transform.position;
        this.damage = damage;
        this.parent = parent;
        this.hitLayer = hitLayer;
        this.maxDistance = maxDistance;
        this.Target = Target;
        this.isSkillBullet = isSkillBullet;
        this.isCrit = iscrit;
        this.startPosition = transform.position;
        this.speed = speed;
        this.isBarrage = barrage;
        this.targetPos = v;
        // 初始化效果
        if (effects != null)
        {
            hitEffects = effects;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (HitWallLayer(other))
        {
            CustomLogger.Log(this, "hitWall");
            DisableBullet();
            
        }
        if (IsInHitLayer(other))
        {
            var enemy = other.GetComponent<CharacterCTRL>();
            if (enemy != null)
            {
                HitTarget(enemy);
                DisableBullet();
            }
        }
    }
    public void DisableBullet()
    {
        damage = 0;
        gameObject.SetActive(false);
        hitEffects.Clear();
        isBarrage = false;
        targetPos = default;
    }
    void Update()
    {
        MoveTowardsTarget();
        CheckMaxDistance();
    }
    private bool HitWallLayer(Collider collider)
    {
        return ((1 << collider.gameObject.layer) & WallLayer) != 0;
    }

    private bool IsInHitLayer(Collider other)
    {
        return ((1 << other.gameObject.layer) & hitLayer) != 0;
    }

    private void MoveTowardsTarget()
    {
        if (Target != null && Target.activeInHierarchy)
        {
            if (!isBarrage)
            {
                Vector3 targetPosWithFixedY = new Vector3(Target.transform.position.x, transform.position.y, Target.transform.position.z);
                transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
                transform.LookAt(targetPosWithFixedY);
            }
            else
            {

                Vector3 targetPosWithFixedY = new Vector3(targetPos.x, transform.position.y, targetPos.z);
                transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
                transform.LookAt(targetPosWithFixedY);
            }
        }
        else
        {
            if (isBarrage)
            {
                Vector3 targetPosWithFixedY = new Vector3(targetPos.x, transform.position.y, targetPos.z);
                transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
                transform.LookAt(targetPosWithFixedY);
            }
            else
            {
                DisableBullet();
            }    

        }
    }

    private void RotateBullet()
    {
        transform.rotation *= Quaternion.Euler(90, 0, 0); // 增加 X 軸上 90 度的旋轉
    }

    private void HitTarget(CharacterCTRL enemy)
    {
        if (isSkillBullet)
        {
            enemy.GetHit(damage, parent, DamageSourceType.Skill.ToString(), isCrit);
        }
        else
        {
            enemy.GetHit(damage, parent, DamageSourceType.NormalAttack.ToString(), isCrit);
        }


        // 觸發所有效果
        foreach (var effect in hitEffects)
        {
            effect.ApplyEffect(enemy, parent);
        }
        DisableBullet();
    }
    private void CheckMaxDistance()
    {
        float distanceTravelled = Vector3.Distance(startPosition, transform.position);
        if (distanceTravelled >= maxDistance)
        {
            DisableBullet();
        }
    }
    public void HiyoriExplosion()
    {
        foreach (var item in Utility.GetCharacterInrange(parent.CurrentHex,2,parent,false))
        {
            Effect effect = EffectFactory.UnStatckableStatsEffct(5, "HiyoriExplosion", -20, StatsType.Resistence, parent, false);
            item.effectCTRL.AddEffect(effect);
            item.GetHit(damage, parent, "default", isCrit);
        }
    }

}
public abstract class HitEffect
{
    public abstract void ApplyEffect(CharacterCTRL target, CharacterCTRL source);
}
public class MikaSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        StarLevelStats stats = source.ActiveSkill.GetCharacterLevel()[target.star];
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int dmg = BaseDamage + DamageRatio * (int)source.GetStat(StatsType.Attack);
        (bool iscrit, int dmg1) = source.CalculateCrit(dmg);
        source.StartCoroutine(target.Explosion(dmg1, 2, target.CurrentHex, source, 100 / 30f, DamageSourceType.Skill.ToString(), iscrit));
    }
}
public class AzusaSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        int dmg = source.ActiveSkill.GetAttackCoefficient(source.GetSkillContext());
        (bool iscrit, int dmg1) = source.CalculateCrit(dmg);
        source.StartCoroutine(target.Explosion(dmg1, 1, target.CurrentHex, source, 45 / 30f, DamageSourceType.Skill.ToString(), iscrit));
    }
}
public class HiyoriSkillEffecct : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        int dmg = source.ActiveSkill.GetAttackCoefficient(source.GetSkillContext());
        (bool iscrit, int dmg1) = source.CalculateCrit(dmg);
        source.StartCoroutine(target.Explosion(dmg1, 2, target.CurrentHex, source, 0, DamageSourceType.Skill.ToString(), iscrit));
        foreach (var item in Utility.GetCharacterInrange(target.CurrentHex,2,source,false))
        {
            Effect effect = EffectFactory.StatckableStatsEffct(5, "Hiyori_Skill", -20, StatsType.Resistence, source, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            item.effectCTRL.AddEffect(effect);
        }
    }
}

public class KazusaSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.KazusaMark();
        target.effectCTRL.AddEffect(effect);
    }
}
public class MisakiSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        StarLevelStats stats = source.ActiveSkill.GetCharacterLevel()[source.star];
        int amount = stats.Data3;
        bool isally = source.IsAlly;
        List<HexNode> nodesInRange = Utility.GetHexInRange(target.CurrentHex, 3);
        List<(HexNode node, int count)> nodeCounts = new List<(HexNode node, int count)>();
        foreach (var node in nodesInRange)
        {
            int count = 0;
            foreach (var neighbor in node.Neighbors)
            {
                if (neighbor.OccupyingCharacter != null && neighbor.OccupyingCharacter.IsAlly != isally)
                {
                    count++;
                }
            }
            nodeCounts.Add((node, count));
        }
        if (source.ActiveSkill is Misaki_Skill misakiSkill)
        {
            nodeCounts = nodeCounts
                .Where(x => !misakiSkill.FragmentNodes.Values.Contains(x.node))
                .Where(x => x.node.OccupyingCharacter == null)
                .ToList();
            nodeCounts.Sort((a, b) => b.count.CompareTo(a.count));
            var selectedNodes = nodeCounts.Take(amount).ToList();
            foreach (var selected in selectedNodes)
            {
                GameObject fragment = GameObject.Instantiate(
                    ResourcePool.Instance.MissleFragmentsPrefab,
                    selected.node.transform.position,
                    Quaternion.identity
                );
                misakiSkill.Fragments.Add(fragment);
                misakiSkill.FragmentNodes.Add(fragment, selected.node);

                CustomLogger.Log(this,
                    $"Spawned fragment at node {selected.node.name} with neighborCount={selected.count}");
            }
        }
        else
        {
            CustomLogger.LogWarning(this, "Source.ActiveSkill is not Misaki_Skill");
        }
    }
}
public class NonomiSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        if (Utility.GetRand(source) < 30)
        {
            GameController.Instance.AddGold(1);//TODO: 改為dropGold
        }
    }
}
public class WakamoSkillEffecct : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.CreateWakamoEffect(30, 5, source);
        target.effectCTRL.AddEffect(effect);
    }
}
