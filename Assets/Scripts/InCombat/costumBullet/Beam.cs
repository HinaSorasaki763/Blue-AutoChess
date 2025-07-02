
using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public float duration = 2f; // 整體持續時間
    public float maxRadius = 5f; // 最終半徑
    public float tickInterval = 0.5f; // 傷害間隔
    public CharacterCTRL Parent;
    public int damage = 10;

    private float elapsedTime = 0f;
    private float tickTimer = 0f;

    private CapsuleCollider capsuleCollider;
    public Transform BeamVisual; // 指向 BeamVisual 子物件
    private HashSet<CharacterCTRL> damagedThisTick = new HashSet<CharacterCTRL>();

    private void Awake()
    {
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        capsuleCollider.isTrigger = true;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        tickTimer += Time.deltaTime;

        float t = Mathf.Clamp01(elapsedTime / duration);
        float currentRadius = Mathf.SmoothStep(0f, 2, t); // 或使用 AnimationCurve

        //capsuleCollider.radius = currentRadius;

        if (BeamVisual != null)
        {
            Vector3 scale = BeamVisual.localScale;
            scale.x = currentRadius * 2f;
            scale.z = currentRadius * 2f;
            BeamVisual.localScale = scale;
        }

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyDamageToEnemiesInCollider();
        }

        if (elapsedTime >= duration)
        {
            gameObject.SetActive(false);
        }
    }

    public void Initialize(CharacterCTRL parent, int damage, float duration, Vector3 direction, Vector3 firePoint)
    {
        this.Parent = parent;
        this.damage = damage;
        this.duration = duration;

        elapsedTime = 0f;
        tickTimer = 0f;

        transform.rotation = Quaternion.LookRotation(direction);
        float halfLength = capsuleCollider.height * 0.5f * transform.localScale.z;
        transform.position = firePoint + direction * halfLength;
        gameObject.SetActive(true);
    }



    private void ApplyDamageToEnemiesInCollider()
    {
        damagedThisTick.Clear();

        Vector3 localCenter = capsuleCollider.center;
        Vector3 worldCenter = capsuleCollider.transform.TransformPoint(localCenter);
        Vector3 forward = capsuleCollider.transform.forward;
        float height = capsuleCollider.height * 0.5f * transform.localScale.z;
        float radius = capsuleCollider.radius* capsuleCollider.transform.localScale.x;

        Vector3 diff = Parent.GetComponent<TokiActiveskill>().LaserFirePoint.position - worldCenter;
        Vector3 point1 = worldCenter + diff;
        Vector3 point2 = worldCenter - diff;

        // 打印詳細資料
        CustomLogger.Log(this, $"--- Capsule Debug Info ---");
        CustomLogger.Log(this, $"localCenter: {localCenter}");
        CustomLogger.Log(this, $"worldCenter: {worldCenter}");
        CustomLogger.Log(this, $"forward: {forward}");
        CustomLogger.Log(this, $"height: {height}, radius: {radius}");
        CustomLogger.Log(this, $"point1: {point1}, point2: {point2}");
        CustomLogger.Log(this, $"--------------------------");

        Collider[] hits = Physics.OverlapCapsule(point1, point2, radius);

        CustomLogger.Log(this, $"Hit Count: {hits.Length}");

        foreach (var hit in hits)
        {

            CharacterCTRL enemy = hit.GetComponent<CharacterCTRL>();
            if (enemy != null && !enemy.IsAlly && !damagedThisTick.Contains(enemy))
            {
                CustomLogger.Log(this, $"Beam Hit: {hit.name}");
                (bool iscrit, int dmg1) = Parent.CalculateCrit(damage);
                enemy.GetHit(damage, Parent, DamageSourceType.Skill.ToString(), iscrit);
                damagedThisTick.Add(enemy);
            }
        }
    }


}
