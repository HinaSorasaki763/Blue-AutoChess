using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using GameEnum;

public class FearManager : MonoBehaviour
{
    private readonly Vector3 offset = new Vector3(0, 0.14f, 0);
    private static FearManager _instance;
    public static FearManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("FearManager");
                _instance = go.AddComponent<FearManager>();
            }
            return _instance;
        }
    }

    private Dictionary<CharacterCTRL, FearData> fearedCharacters = new Dictionary<CharacterCTRL, FearData>();

    public void ApplyFear(CharacterCTRL fearSource, List<CharacterCTRL> targets, float duration)
    {
        (float length, float effectiveness) = fearSource.BeforeApplyingNegetiveEffect(duration, 0);
        float endTime = Time.time + length;
        foreach (var t in targets)
        {
            if (!fearedCharacters.ContainsKey(t) && !t.isCCImmune)
            {
                fearedCharacters.Add(t, new FearData
                {
                    stop = false,
                    fearSource = fearSource,
                    fearEndTime = endTime,
                    fearMovementCoroutine = null
                }); 
                CustomLogger.Log(this, $"角色 {t.name} 被恐懼影響,結束時間: {endTime},恐懼來源: {fearSource.name}");
            }
            else
            {
                FearData fearData = fearedCharacters[t];
                fearData.fearEndTime = endTime;
                fearedCharacters[t] = fearData;
            }
            t.IsFeared = true;
        }
        StartCoroutine(FearLoop());
    }

    private IEnumerator FearLoop()
    {
        while (fearedCharacters.Count > 0)
        {
            RemoveExpiredFear();
            if (fearedCharacters.Count == 0)
                yield break;
            ProcessFearMovement();
            yield return new WaitForSeconds(2f);
        }
    }

    private void RemoveExpiredFear()
    {
        List<CharacterCTRL> expired = new List<CharacterCTRL>();
        foreach (var kv in fearedCharacters)
        {
            if (Time.time >= kv.Value.fearEndTime)
            {
                expired.Add(kv.Key);
            }
        }

        foreach (var c in expired)
        {
            EndFear(c);
        }
    }

    private void EndFear(CharacterCTRL c)
    {
        if (fearedCharacters.TryGetValue(c, out FearData data))
        {
            if (data.fearMovementCoroutine != null)
            {
                data.stop = true;
                StopCoroutine(data.fearMovementCoroutine);
                CustomLogger.Log(this, $"角色 {c.name} 恐懼結束,移除恐懼協程");
            }
        }
        fearedCharacters.Remove(c);
        c.IsFeared = false;
        c.RecenterOnHex();
        c.FindTarget();
        CustomLogger.Log(this, $"角色 {c.name} 恐懼結束");
    }


    private void ProcessFearMovement()
    {
        var list = fearedCharacters
            .Select(kv => new { character = kv.Key, fearData = kv.Value })
            .ToList();
        list.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.character.CurrentHex.Position, a.fearData.fearSource.CurrentHex.Position);
            float distB = Vector3.Distance(b.character.CurrentHex.Position, b.fearData.fearSource.CurrentHex.Position);
            return distB.CompareTo(distA);
        });

        foreach (var item in list)
        {
            var c = item.character;
            if (!fearedCharacters.ContainsKey(c))
                continue;
            if (c.customAnimator != null)
            {
                c.customAnimator.ChangeState(GameEnum.CharacterState.Moving);
                ExecuteFearMove(c, fearedCharacters[c]);
            }

        }
    }

    private void ExecuteFearMove(CharacterCTRL c, FearData data)
    {
        var neighbors = c.CurrentHex.Neighbors;
        HexNode bestNode = null;
        float bestDist = Vector3.Distance(c.CurrentHex.Position, data.fearSource.CurrentHex.Position);

        foreach (var n in neighbors)
        {
            if (n.OccupyingCharacter != null)
                continue;
            float dist = Vector3.Distance(n.Position, data.fearSource.CurrentHex.Position);
            if (dist > bestDist)
            {
                bestDist = dist;
                bestNode = n;
            }
        }

        if (bestNode == null)
        {
            Vector3 directionAway = c.transform.position - data.fearSource.transform.position;
            c.transform.rotation = Quaternion.LookRotation(directionAway);
            return;
        }

        if (c.CurrentHex != null)
        {
            c.CurrentHex.SetOccupyingCharacter(null);
            c.CurrentHex.Release();
        }
        bestNode.Reserve(c);
        bestNode.SetOccupyingCharacter(c);
        c.CurrentHex = bestNode;

        Vector3 targetPos = bestNode.Position + offset;
        Coroutine moveCoroutine = StartCoroutine(MoveTowardsPosition(targetPos, c));
        data.fearMovementCoroutine = moveCoroutine;
        fearedCharacters[c] = data;

        CustomLogger.Log(this, $"恐懼移動: {c.name} 移動到 {bestNode.name}");
    }
    public void Update()
    {

    }
    public IEnumerator MoveTowardsPosition(Vector3 targetPos, CharacterCTRL characterCTRL)
    {
        while (Vector3.Distance(characterCTRL.transform.position, targetPos) > 0.1f && !fearedCharacters[characterCTRL].stop)
        {
            characterCTRL.transform.position = Vector3.MoveTowards(characterCTRL.transform.position, targetPos, 0.2f * Time.deltaTime);
            characterCTRL.transform.rotation = Quaternion.Slerp(
                characterCTRL.transform.rotation,
                Quaternion.LookRotation(targetPos - characterCTRL.transform.position),
                Time.deltaTime * 5
            );
            CustomLogger.Log(this,$"chararcter { characterCTRL } is feared and moving to {targetPos} at {Time.time}");
            HexNode newGrid = SpawnGrid.Instance.GetHexNodeByPosition(targetPos - offset);
            if (newGrid != null)
            {
                characterCTRL.MoveToNewGrid(newGrid);
            }

            yield return null;
        }

        characterCTRL.transform.position = targetPos;
    }
    public void RemoveCharacterFromFear(CharacterCTRL c)
    {
        if (fearedCharacters.TryGetValue(c, out FearData data))
        {
            if (data.fearMovementCoroutine != null)
            {
                StopCoroutine(data.fearMovementCoroutine);
            }
            fearedCharacters.Remove(c);
            c.IsFeared = false;
            c.RecenterOnHex();
            c.FindTarget();
        }
    }
}

public struct FearData
{
    public CharacterCTRL fearSource;
    public float fearEndTime;
    public bool stop;
    // ★新增：用來記錄移動協程的 Coroutine 參考
    public Coroutine fearMovementCoroutine;
}
