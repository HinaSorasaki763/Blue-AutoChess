using GameEnum;
using UnityEngine;
using System.Linq;

public class NormalBullet : MonoBehaviour
{
    private static int nextId = 0;
    private int attackId;
    public float speed = 20f;
    public Vector3 targetPosition;
    public GameObject Target;
    public float damage;
    public LayerMask hitLayer;
    public CharacterCTRL parent;
    private Vector3 startPosition; // �O���l�u�_�l��m
    private float maxDistance; // �̤j�Z��
    private bool iscrit = false;

    void Awake()
    {
        attackId = nextId++;
    }
    void OnEnable()
    {
        // �C���ҥήɲM�� Trail Renderer
        var trailRenderer = GetComponentInChildren<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    public void Initialize(Vector3 targetPosition, float damage, LayerMask hitLayer, CharacterCTRL parent, float maxDistance,GameObject Target,bool iscrit = false)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;
        this.parent = parent;
        this.hitLayer = hitLayer;
        this.maxDistance = maxDistance;
        this.iscrit = iscrit;
        this.Target = Target;
        startPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsInHitLayer(other))
        {
            var enemy = other.GetComponent<CharacterCTRL>();
            if (enemy != null)
            {
                HitTarget(enemy);
                gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        MoveTowardsTarget();
        CheckMaxDistance();
    }

    private bool IsInHitLayer(Collider other)
    {
        return ((1 << other.gameObject.layer) & hitLayer) != 0;
    }

    private void MoveTowardsTarget()
    {
        Vector3 targetPosWithFixedY = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
        transform.LookAt(targetPosWithFixedY);
        RotateBullet();
    }

    private void RotateBullet()
    {
        transform.rotation *= Quaternion.Euler(90, 0, 0); // �W�[ X �b�W 90 �ת�����
    }

    private void HitTarget(CharacterCTRL enemy)
    {
        enemy.GetHit((int)damage, parent,iscrit);
    }

    // �ˬd�l�u�O�_�W�L�̤j�Z��
    private void CheckMaxDistance()
    {
        float distanceTravelled = Vector3.Distance(startPosition, transform.position);
        if (distanceTravelled >= maxDistance)
        {
            gameObject.SetActive(false); // �T�Τl�u
        }
    }
    public void HiyoriExplosion()
    {
        var nodes = Utility.GetHexInRange(parent.CurrentHex, 2);
        var targets = nodes
            .Where(node => node.OccupyingCharacter != null && node.OccupyingCharacter.IsAlly != parent.IsAlly)
            .Select(node => node.OccupyingCharacter)
            .ToList();
        foreach (var item in targets)
        {
            Effect effect = EffectFactory.UnStatckableStatsEffct(5,20,StatsType.Resistence,parent);
            item.effectCTRL.AddEffect(effect);
            item.GetHit((int)damage, parent,iscrit);
        }
    }
}
