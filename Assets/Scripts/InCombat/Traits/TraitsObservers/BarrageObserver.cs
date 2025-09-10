using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BarrageObserver : CharacterObserverBase
{
    public CharacterCTRL Character;
    public int numSteps = 36;
    public float maxDistance = 20f;
    public float BestAngle;
    private int dmg = 1;
    private int Level;
    private int CastTimes;
    public int IntervalAngle;
    public int InitAngle;
    public BarrageObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        Level = level;
        Character = character;
        IntervalAngle = character.characterStats.BarrageIntervalAngle;
        InitAngle = character.characterStats.BarrageInitAngle;
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(3,1,30)},
            {2, new TraitLevelStats(5,1,40)},
            {3, new TraitLevelStats(10,3,50)}
        };
        return statsByStarLevel;
    }
    public override void ResetVaribles(CharacterCTRL character)
    {
        base.ResetVaribles(character);
        CastTimes = 0;
    }
    public override void OnCastedSkill(CharacterCTRL character)
    {
        IntervalAngle = character.characterStats.BarrageIntervalAngle;
        InitAngle = character.characterStats.BarrageInitAngle;
        CustomLogger.Log(this, $"BarrageObserver OnCastedSkill {character.name}");
        CoroutineController coroutineController = character.GetComponent<CoroutineController>();
        if (coroutineController == null)
        {
            coroutineController = character.gameObject.AddComponent<CoroutineController>();
        }
        List<HexNode> bestSectorNodes = FindBestSector();
        float bestAngle = BestAngle;

        if (character.isShirokoTerror)
        {
            int skillID = character.customAnimator.animator.GetInteger("SkillID");
            if (skillID == 7)
            {
                float animationTime = character.customAnimator.GetAnimationClipInfo(15).Item2;
                coroutineController.SetNextSkill(this, animationTime, 0.1f, bestAngle, GetAngle(),false);
                return;
            }
        }
        float time = character.customAnimator.GetAnimationClipInfo(7).Item2;
        coroutineController.SetNextSkill(this, time, 0.1f, bestAngle, GetAngle(), character.ActiveSkill.Barrage_penetrate());
        CastTimes++;
    }
    public int GetAngle()
    {
        return InitAngle + (CastTimes * 20);
    }

    public List<HexNode> FindBestSector()
    {
        Vector3 origin = Character.transform.position;
        float startYaw = Character.transform.rotation.eulerAngles.y;

        int maxEnemiesCount = -1;
        float bestStartRel = 0f; // 以 forward 為 0° 的相對起始角
        List<HexNode> bestSector = null;

        float step = 360f / numSteps;

        // 掃描：全部以「相對 forward 角度」運算
        for (int i = 0; i < numSteps; i++)
        {
            float angleStartRel = i * step;
            float angleEndRel = angleStartRel + GetAngle();

            List<HexNode> sector = GetNodesInSectorRelative(origin, angleStartRel, angleEndRel);
            int enemies = CountEnemiesInNodes(sector);

            if (enemies > maxEnemiesCount)
            {
                maxEnemiesCount = enemies;
                bestSector = sector;
                bestStartRel = angleStartRel;
            }
        }

        // 計算「當前朝向」這一扇形（-half ~ +half，相對角度）
        float half = GetAngle() * 0.5f;
        List<HexNode> currentSector = GetNodesInSectorRelative(origin, -half, +half);
        int currentCount = CountEnemiesInNodes(currentSector);

        // 決策：只有「相等」時才維持原角度
        if (currentCount == maxEnemiesCount)
        {
            BestAngle = startYaw; // 維持原先面向
            return currentSector;
        }
        else
        {
            // 轉向最佳：世界角 = 目前朝向 + (最佳起始相對角 + 扇形一半)
            float worldYaw = startYaw + bestStartRel + half;
            // 正規化到 0~360
            BestAngle = (worldYaw % 360f + 360f) % 360f;
            return bestSector ?? currentSector; // 理論上不會為空；保險
        }
    }

    private List<HexNode> GetNodesInSectorRelative(Vector3 origin, float angleStartRel, float angleEndRel)
    {
        List<HexNode> nodesInSector = new List<HexNode>();

        float start = (angleStartRel % 360f + 360f) % 360f;
        float end = (angleEndRel % 360f + 360f) % 360f;

        foreach (var node in SpawnGrid.Instance.hexNodes.Values)
        {
            Vector3 toNode = node.Position - origin;
            float dist = toNode.magnitude;
            if (dist > maxDistance) continue;

            // 0° 代表角色 forward，範圍 0~360
            float ang = Vector3.SignedAngle(Character.transform.forward, toNode, Vector3.up);
            ang = (ang % 360f + 360f) % 360f;

            bool inRange = start <= end ? (ang >= start && ang <= end)
                                        : (ang >= start || ang <= end);

            if (inRange)
                nodesInSector.Add(node);
        }
        return nodesInSector;
    }

    private int CountEnemiesInNodes(List<HexNode> nodes)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.OccupyingCharacter != null
                && node.OccupyingCharacter.IsAlly != Character.IsAlly
                && node.OccupyingCharacter.gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    private List<HexNode> GetNodesInSector(Vector3 origin, float angleStart, float angleEnd)
    {
        List<HexNode> nodesInSector = new List<HexNode>();

        foreach (var node in SpawnGrid.Instance.hexNodes.Values)
        {
            Vector3 directionToNode = node.Position - origin;
            float distanceToNode = directionToNode.magnitude;
            if (distanceToNode > maxDistance)
                continue;

            float angleToNode = Vector3.SignedAngle(Character.transform.forward, directionToNode, Vector3.up);
            angleToNode = (angleToNode + 360) % 360;
            float adjustedStart = (angleStart + 360) % 360;
            float adjustedEnd = (angleEnd + 360) % 360;

            bool inRange = adjustedStart <= adjustedEnd
                ? (angleToNode >= adjustedStart && angleToNode <= adjustedEnd)
                : (angleToNode >= adjustedStart || angleToNode <= adjustedEnd);

            if (inRange)
            {
                nodesInSector.Add(node);
            }
        }

        return nodesInSector;
    }

    public void ScatterBulletAtAngle(float bulletAngle)
    {
        Vector3 direction = Quaternion.Euler(0, bulletAngle, 0) * Vector3.forward;
        Vector3 targetPosition = Character.transform.position + direction * maxDistance;
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, Character.FirePoint.position, Quaternion.identity);
        dmg = Character.ActiveSkill.GetAttackCoefficient(Character.GetSkillContext());
        List<HitEffect> l = new List<HitEffect>();
        if (GameController.Instance.CheckCharacterEnhance(21, Character.IsAlly))
        {
            float ratio = Mathf.Min(GameController.Instance.GetGoldAmount(),50)*0.01f;
            dmg = (int)(dmg * (1 + ratio));
            l.Add(new NonomiSkillEffect());
        }
        if (GameController.Instance.CheckCharacterEnhance(25, Character.IsAlly))
        {
            l.Add(new HinaSkillEffect());
        }
        (bool iscrit, int dmg1) = Character.CalculateCrit(dmg);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, Character.GetTargetLayer(), Character, 15f, Character.GetTarget(), true, iscrit, l, 20, true, targetPosition);
    }

}
