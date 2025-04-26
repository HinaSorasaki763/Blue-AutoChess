using System.Collections.Generic;
using UnityEngine;

public class TrailedBullet : MonoBehaviour
{
    private static int nextId = 0;
    private int attackId;

    public float speed = 20f;
    public Vector3 targetPosition;
    public float damage;
    public float decayFactor;
    public LayerMask HitLayer;
    public CharacterCTRL Parent;

    private Vector3 startPosition;
    private Vector3 moveDirection; // 用來記錄移動方向
    private bool IsCrit;

    void Awake()
    {
        attackId = nextId++;
    }

    public void OnEnable()
    {
        targetPosition = Vector3.zero;
    }

    public void Initialized(Vector3 targetPosition, float damage, float decayFactor, LayerMask hitLayer, CharacterCTRL parent, bool isCrit)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;
        this.decayFactor = decayFactor;
        this.IsCrit = isCrit;
        HitLayer = hitLayer;
        Parent = parent;
        startPosition = transform.position;
        Vector3 targetPosWithFixedY = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        moveDirection = (targetPosWithFixedY - transform.position).normalized;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & HitLayer) != 0)
        {
            CharacterCTRL enemy = other.GetComponent<CharacterCTRL>();
            if (enemy != null&& !enemy.characterStats.logistics)
            {
                HitTarget(enemy);
            }
        }
    }

    void Update()
    {
        // 沿著初始化時的方向持續前進
        transform.position += moveDirection * speed * Time.deltaTime;

        // 調整朝向
        transform.LookAt(transform.position + moveDirection);
        // 再加上額外的90度旋轉
        Quaternion additionalRotation = Quaternion.Euler(90, 0, 0);
        transform.rotation *= additionalRotation;

        CheckMaxDistance();
    }

    void HitTarget(CharacterCTRL enemy)
    {
        CustomLogger.Log(this, "enemy.GetHit((int)damage, Parent, \"TrailedBullet\", IsCrit);");
        enemy.GetHit((int)damage, Parent, "TrailedBullet", IsCrit);
        damage *= decayFactor * 0.01f;
    }

    public void OnDisable()
    {
        Parent = null;
        decayFactor = 0;
        damage = 0;
        attackId = 0;
    }

    private void CheckMaxDistance()
    {
        float distanceTravelled = Vector3.Distance(startPosition, transform.position);
        if (distanceTravelled >= 20)
        {
            gameObject.SetActive(false);
        }
    }
}
