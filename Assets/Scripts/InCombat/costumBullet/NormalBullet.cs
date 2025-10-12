using GameEnum;
using System.Collections.Generic;
using System.Linq;
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
    public bool isSkillBullet;
    private List<HitEffect> hitEffects = new List<HitEffect>();
    private bool isBarrage;
    private Vector3 targetPos;
    private bool IsNormalAttack;
    public bool penetrate;
    private string detailedSource;
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

    public void Initialize(int damage, LayerMask hitLayer, CharacterCTRL parent, float maxDistance, GameObject Target, bool isSkillBullet, bool iscrit, List<HitEffect> effects = null, int speed = 20, bool barrage = false, Vector3 v = default, bool penetrate = false, string detailedSource = null, bool neru = false, Vector3 neruInitPos = default)
    {
        if (!neru)
        {
            transform.position = parent.FirePoint.transform.position;
        }
        else
        {
            transform.position = neruInitPos;
        }

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
        this.penetrate = penetrate;
        this.detailedSource = detailedSource;
    }

    void OnTriggerEnter(Collider other)
    {
        if (HitWallLayer(other))
        {
            other.GetComponent<Wall>().GetHit(parent, damage, isCrit);
            CustomLogger.Log(this, "hitWall");
            if (!penetrate)
            {
                DisableBullet();
            }
            else
            {
                damage = (int)(damage*(1- other.GetComponent<Wall>().GetResist()));
            }
        }
        if (IsInHitLayer(other))
        {

            var enemy = other.GetComponent<CharacterCTRL>();
            if (enemy != null && !isSkillBullet)
            {
                HitTarget(enemy);
                DisableBullet();
            }
            else
            {
                HitTarget(enemy);
                if (!penetrate)
                {
                    DisableBullet();
                }
            }
        }
    }
    public void DisableBullet()
    {
        damage = 0;
        hitEffects.Clear();
        isBarrage = false;
        targetPos = default;
        gameObject.SetActive(false);

    }
    void Update()
    {
        MoveTowardsTarget();
        CheckMaxDistance();
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Preparing || !Target || (!isSkillBullet && !Target.activeInHierarchy))
        {
            DisableBullet();
        }
    }
    private bool HitWallLayer(Collider collider)
    {
        if (((1 << collider.gameObject.layer) & WallLayer) == 0)
            return false;
        Wall wall = collider.GetComponent<Wall>();
        if (wall == null)
            return false;

        bool isAlly = parent.IsAlly;
        return wall.IsAllyWall != isAlly;
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
                Vector3 targetPosWithFixedY = new Vector3(Target.GetComponent<CharacterCTRL>().GetHitPoint.position.x, transform.position.y, Target.GetComponent<CharacterCTRL>().GetHitPoint.position.z);
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
        if (parent == null || !parent.gameObject.activeInHierarchy) return;
        foreach (var effect in hitEffects)
        {
            effect.ApplyEffect(enemy, parent);
        }
        if (isSkillBullet)
        {
            if (detailedSource != null)
            {
                enemy.GetHit(damage, parent, detailedSource, isCrit);
            }
            else
            {
                enemy.GetHit(damage, parent, DamageSourceType.Skill.ToString(), isCrit);
            }
        }
        else
        {
            enemy.GetHit(damage, parent, DamageSourceType.NormalAttack.ToString(), isCrit);
        }
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
        foreach (var item in Utility.GetCharacterInrange(parent.CurrentHex, 2, parent, false))
        {
            Effect effect = EffectFactory.UnStatckableStatsEffct(5, "HiyoriExplosion", -20, StatsType.Resistence, parent, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect.Value, effect.Source)
            );
            item.effectCTRL.AddEffect(effect, item);
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
        int dmg = BaseDamage + DamageRatio * (int)(source.GetStat(StatsType.Attack) * 0.01f);
        (bool iscrit, int dmg1) = source.CalculateCrit(dmg);
        source.StartCoroutine(target.Explosion(dmg1, 2, target.CurrentHex, source, 100 / 30f, DamageSourceType.Skill.ToString(), iscrit));
    }
}
public class MikaEnhanceskillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Dictionary<HexNode, CharacterCTRL> matchNode = new();
        StarLevelStats stats = source.ActiveSkill.GetCharacterLevel()[target.star];
        int BaseDamage = stats.Data1;
        int DamageRatio = stats.Data2;
        int dmg = source.ActiveSkill.GetAttackCoefficient(source.GetSkillContext());
        (bool iscrit, int dmg1) = source.CalculateCrit(dmg);
        source.StartCoroutine(
            target.Explosion(dmg1, 1, target.CurrentHex, source, 100 / 30f, DamageSourceType.Skill.ToString(), iscrit)
        );
        HexNode centerNode = target.CurrentHex;
        List<HexNode> firstRingNodes = centerNode.Neighbors;
        List<HexNode> secondRingNodes = firstRingNodes
            .SelectMany(node => node.Neighbors)
            .Where(node => !firstRingNodes.Contains(node) && node != centerNode)
            .Distinct()
            .ToList();
        var charactersInSecondRing = Utility.GetCharacterInSet(secondRingNodes, source, false);
        var adjacency = new Dictionary<CharacterCTRL, List<HexNode>>();
        var firstRingSet = new HashSet<HexNode>(firstRingNodes);

        foreach (var c in charactersInSecondRing)
        {
            var possibleMoves = c.CurrentHex.Neighbors.Where(n => firstRingSet.Contains(n)).ToList();
            adjacency[c] = possibleMoves;
        }
        matchNode = new Dictionary<HexNode, CharacterCTRL>();
        foreach (var node in firstRingNodes)
        {
            matchNode[node] = node.OccupyingCharacter;
        }
        foreach (var c in charactersInSecondRing)
        {
            var visited = new HashSet<HexNode>();
            Utility.TryAssign(c, visited, adjacency, ref matchNode);
        }
        List<CharacterCTRL> cList = new List<CharacterCTRL>();
        foreach (var kvp in matchNode)
        {
            HexNode node = kvp.Key;
            CharacterCTRL c = kvp.Value;
            if (c == null) continue;
            c.CurrentHex.HardRelease();
            c.transform.position = node.Position + new Vector3(0, 0.14f, 0);
            node.HardReserve(c);
            cList.Add(c);
            CustomLogger.Log(this, $"Moving {c.name} from {c.CurrentHex.name} to {node.name}");
        }
        foreach (var item in cList)
        {
            TrinityManager.Instance.TriggerComet(centerNode.Position, "MikaEnhancedskill", centerNode, source);
        }

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

public class KazusaSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.KazusaMark();
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class MisakiSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        MisakiObserver misakiObserver = source.characterObserver as MisakiObserver;
        StarLevelStats stats = source.ActiveSkill.GetCharacterLevel()[source.star];
        int amount = stats.Data3;
        bool isally = source.IsAlly;
        CharacterParent characterParent = target.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        var enemies = characterParent.GetBattleFieldCharacter().Where(c => c.IsAlly != isally && c.isTargetable).ToList();
        List<(HexNode node, float nearestEnemyDist, int fragmentNearby)> nodeScores = new();

        foreach (var node in Utility.GetHexInRange(target.CurrentHex, 3))
        {
            if (node.OccupyingCharacter != null || misakiObserver.FragmentNodes.Values.Contains(node))
                continue;

            // 最近敵人距離
            float nearestDist = float.MaxValue;
            foreach (var enemy in enemies)
            {
                
                float dist = Vector3.Distance(node.Position, enemy.CurrentHex.Position);
                if (dist < nearestDist) nearestDist = dist;
            }

            // 附近碎片數
            int fragmentNearby = node.Neighbors.Count(n => misakiObserver.FragmentNodes.Values.Contains(n));
            nodeScores.Add((node, nearestDist, fragmentNearby));
        }

        nodeScores.Sort((a, b) =>
        {
            int d = a.nearestEnemyDist.CompareTo(b.nearestEnemyDist);
            return d != 0 ? d : b.fragmentNearby.CompareTo(a.fragmentNearby);
        });

        foreach (var n in nodeScores.Take(amount))
        {
            var frag = ResourcePool.Instance.SpawnObject(SkillPrefab.MissleFragmentsPrefab, n.node.transform.position, Quaternion.identity);
            misakiObserver.Fragments.Add(frag);
            misakiObserver.FragmentNodes.Add(frag, n.node);
        }
    }


}
public class MiyuEnhancedSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.MiyuEnhancedSkillEffect();
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class NonomiSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        int critChance = (int)source.GetStat(StatsType.CritChance);
        if (Utility.GetRandfloat(source) < (30 + critChance / 2) * 0.0001f)
        {
            ResourcePool.Instance.GetGoldPrefab(target.transform.position);
        }
    }
}
public class HinaSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.HinaEffect();
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class HiyoriSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.HiyoriEffect();
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class NeruSkillEffect : HitEffect
{
    public NeruSkillEffect(CharacterCTRL c)
    {
        Target = c;
    }
    private CharacterCTRL Target;
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        CharacterCTRL c = Utility.GetNearestAlly(Target);
        if (c != Target)
        {
            var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, target.transform.position, Quaternion.identity);
            var bulletComponent = bullet.GetComponent<NormalBullet>();
            int dmg = (int)(source.ActiveSkill.GetAttackCoefficient(source.GetSkillContext()) * 0.2f);
            (bool, int) tuple = source.CalculateCrit(dmg);
            bool iscrit = tuple.Item1;
            dmg = tuple.Item2;
            bulletComponent.Initialize(dmg, source.GetTargetLayer(), source, 20, c.gameObject, false, iscrit, null, 20, false, default, false, null, true, target.GetHitPoint.position);
        }
        else
        {
            int dmg = (int)(source.ActiveSkill.GetAttackCoefficient(source.GetSkillContext()) * 0.2f);
            (bool, int) tuple = source.CalculateCrit(dmg);
            bool iscrit = tuple.Item1;
            dmg = tuple.Item2;
            c.GetHit(dmg, source, DamageSourceType.NormalAttack.ToString(), iscrit);
        }
    }
}

public class WakamoSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.CreateWakamoEffect(30, 5, source);
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class WakamoEnhancedSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        Effect effect = EffectFactory.WakamoEnhancedMark(source, 20);
        target.effectCTRL.AddEffect(effect, target);
    }
}
public class SakurakoSkillEffect : HitEffect
{
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        CharacterParent characterParent = source.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        int amount = characterParent.SakurakoSkillDmg;
        if (GameController.Instance.CheckCharacterEnhance(49, source.IsAlly))
        {
            target.GetHitByTrueDamage(amount, source, "SakurakoSkillEffect", false);
        }
        else
        {
            target.GetHit(amount, source, "SakurakoSkillEffect", false, false);
        }
    }
}

