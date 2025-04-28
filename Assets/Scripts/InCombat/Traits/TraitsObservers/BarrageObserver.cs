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
        base.OnCastedSkill(character);
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
    public float GetAngle()
    {
        return InitAngle + (CastTimes * 20);
    }

    public List<HexNode> FindBestSector()
    {
        Vector3 origin = Character.transform.position;
        CustomLogger.Log(this, $"Character position: {origin}");
        List<HexNode> bestSector = new List<HexNode>();
        int maxEnemiesCount = 0;
        Vector3 v = Character.transform.position - Character.GetTarget().transform.position;
        float startAngle = Character.transform.rotation.eulerAngles.y;
        CustomLogger.Log(this, $"Character start rotation angle: {startAngle}");
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < numSteps; i++)
        {
            float angleStart = (startAngle + i * (360f / numSteps)) % 360;
            float angleEnd = (angleStart + GetAngle()) % 360;
            sb.AppendLine($"Step {i}: angleStart = {angleStart}, angleEnd = {angleEnd}");

            List<HexNode> currentSectorNodes = GetNodesInSector(origin, angleStart, angleEnd);
            sb.AppendLine($"Step {i}: Nodes in sector = {currentSectorNodes.Count}");

            int enemiesCount = CountEnemiesInNodes(currentSectorNodes);
            sb.AppendLine($"Step {i}: Enemies count = {enemiesCount}");

            if (enemiesCount > maxEnemiesCount)
            {
                maxEnemiesCount = enemiesCount;
                bestSector = currentSectorNodes;
                BestAngle = angleStart + startAngle + (GetAngle() / 2);
                sb.AppendLine($"New best sector found at step {i} with {maxEnemiesCount} enemies. BestAngle = {BestAngle}");
            }
        }
        List<HexNode> currentSectorNode = GetNodesInSector(origin, startAngle - GetAngle() / 2, startAngle + GetAngle() / 2);
        int EnemiesCount = CountEnemiesInNodes(currentSectorNode);
        if (EnemiesCount >= maxEnemiesCount)
        {
            bestSector = currentSectorNode;
            BestAngle = startAngle;
            sb.AppendLine($"New best sector found at step ORIGIN with {maxEnemiesCount} enemies. BestAngle = {BestAngle}");
        }
        sb.AppendLine($"Final best sector found with {maxEnemiesCount} enemies.");
        CustomLogger.Log(this, sb.ToString());
        return bestSector;
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
