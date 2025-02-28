using GameEnum;
using System.Collections;
using UnityEngine;

public class Miyako_DroneCTRL : MonoBehaviour
{
    public GameObject DronePrefab;
    private GameObject DroneRef;
    public CharacterCTRL parent;

    // 假設動畫為 30 FPS
    private const float frameRate = 30f;
    private Animator droneAnimator;

    private void OnEnable()
    {
        if (DroneRef == null)
        {
            DroneRef = Instantiate(DronePrefab);
            DroneRef.SetActive(false);
        }
    }

    /// <summary>
    /// test
    /// </summary>
    public void GetDrone()
    {
        // 顯示無人機並重置位置
        DroneRef.SetActive(true);
        DroneRef.transform.position = transform.position;

        if (droneAnimator == null)
        {
            droneAnimator = DroneRef.GetComponent<Animator>();
        }

        // 播放無人機動畫 (假設名稱為 "DroneAnimation")
        if (droneAnimator != null)
        {
            droneAnimator.Play("DroneAnimation", 0, 0f);
        }

        // 啟動協程，持續追蹤 target 的位置
        StartCoroutine(DroneSequence());
    }

    /// <summary>
    /// 無人機飛行的協程：透過不同時間區段進行不同速度/效果。
    /// </summary>
    private IEnumerator DroneSequence()
    {
        if (parent.Target == null)
        {
            yield break;
        }
        yield return new WaitForSeconds(9f / frameRate);
        float slowDuration = (22f - 9f) / frameRate;
        float timer = 0f;
        Vector3 startPos = DroneRef.transform.position;
        float slowFraction = 0.2f;
        while (timer < slowDuration)
        {
            timer += Time.deltaTime;
            float t = timer / slowDuration;
            if (parent.Target == null)
            {
                break;
            }
            Vector3 currentTargetPos = parent.Target.transform.position;
            Vector3 slowTargetPos = Vector3.Lerp(startPos, currentTargetPos, slowFraction);
            DroneRef.transform.position = Vector3.Lerp(startPos, slowTargetPos, t);
            yield return null;
        }
        float accelerateDuration = (48f - 22f) / frameRate;
        timer = 0f;
        Vector3 accelerateStartPos = DroneRef.transform.position;

        while (timer < accelerateDuration)
        {
            timer += Time.deltaTime;
            float t = timer / accelerateDuration;

            if (parent.Target == null)
            {
                break;
            }
            Vector3 currentTargetPos = parent.Target.transform.position;
            float distToTarget = Vector3.Distance(accelerateStartPos, currentTargetPos);
            Vector3 accelerateTargetPos;
            if (distToTarget > 0.5f)
            {
                float moveDist = distToTarget - 0.5f;
                accelerateTargetPos = Vector3.Lerp(accelerateStartPos, currentTargetPos, moveDist / distToTarget);
            }
            else
            {
                accelerateTargetPos = accelerateStartPos;
            }
            DroneRef.transform.position = Vector3.Lerp(accelerateStartPos, accelerateTargetPos, t * t);

            yield return null;
        }
        CustomLogger.Log(this, "Frame48：無人機準備起跳炸向敵人");
        float jumpDuration = (51f - 48f) / frameRate;
        timer = 0f;
        Vector3 jumpStartPos = DroneRef.transform.position;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;

            if (parent.Target == null)
            {
                CustomLogger.LogWarning(this, "Target lost during jump");
                break;
            }
            Vector3 currentTargetPos = parent.Target.transform.position;
            DroneRef.transform.position = Vector3.Lerp(jumpStartPos, currentTargetPos, t);

            yield return null;
        }
        CustomLogger.Log(this, "Frame51：抵達敵人高度，即將爆炸");

        if (parent.Target != null) // 再次檢查目標是否仍存在
        {
            int dmg = parent.ActiveSkill.GetAttackCoefficient(parent.GetSkillContext());
            (bool iscrit, int dmg1) = parent.CalculateCrit(dmg);
            parent.Target.GetComponent<CharacterCTRL>().GetHit(dmg1, parent, DamageSourceType.Skill.ToString(), iscrit);
            StarLevelStats stats = parent.ActiveSkill.GetCharacterLevel()[parent.star];
            Effect effect = EffectFactory.CreateStunEffect(stats.Data3, parent);
            Effect effect1 = EffectFactory.StatckableStatsEffct(5, "Miyako_Skill", -20, StatsType.Resistence, parent, false);
            effect1.SetActions(
                (character) => character.ModifyStats(StatsType.Resistence, effect1.Value, effect1.Source),
                (character) => character.ModifyStats(StatsType.Resistence, -effect1.Value, effect1.Source)
            );
            parent.Target.GetComponent<CharacterCTRL>().effectCTRL.AddEffect(effect);
            parent.Target.GetComponent<CharacterCTRL>().effectCTRL.AddEffect(effect1);
        }
        DroneRef.SetActive(false);
    }
}
