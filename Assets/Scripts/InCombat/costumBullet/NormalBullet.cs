using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    private static int nextId = 0;
    private int attackId;
    public float speed = 20f;
    public Vector3 targetPosition;
    public float damage;
    public LayerMask hitLayer;
    public CharacterCTRL parent;
    private Vector3 startPosition; // 記錄子彈起始位置
    private float maxDistance; // 最大距離

    void Awake()
    {
        attackId = nextId++;
    }

    void OnEnable()
    {
       // targetPosition = Vector3.zero;
    }

    public void Initialize(Vector3 targetPosition, float damage, LayerMask hitLayer, CharacterCTRL parent, float maxDistance)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;
        this.parent = parent;
        this.hitLayer = hitLayer;
        this.maxDistance = maxDistance;
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
        if (true)
        {

        }
        Vector3 targetPosWithFixedY = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
        transform.LookAt(targetPosWithFixedY);
        RotateBullet();
    }

    private void RotateBullet()
    {
        transform.rotation *= Quaternion.Euler(90, 0, 0); // 增加 X 軸上 90 度的旋轉
    }

    private void HitTarget(CharacterCTRL enemy)
    {
        enemy.GetHit((int)damage, parent);
    }

    // 檢查子彈是否超過最大距離
    private void CheckMaxDistance()
    {
        float distanceTravelled = Vector3.Distance(startPosition, transform.position);
        if (distanceTravelled >= maxDistance)
        {
            gameObject.SetActive(false); // 禁用子彈
        }
    }
}
