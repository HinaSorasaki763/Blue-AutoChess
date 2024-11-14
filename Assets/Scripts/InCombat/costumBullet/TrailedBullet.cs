using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    private bool IsCrit;
    void Awake()
    {
        attackId = nextId++;
    }
    public void OnEnable()
    {
        targetPosition = Vector3.zero;
    }
    public void Initialized(Vector3 targetPosition, float damage, float decayFactor, LayerMask hitLayer, CharacterCTRL parent,bool isCrit)
    {
        this.targetPosition = targetPosition;
        this.damage = damage;
        this.decayFactor = decayFactor;
        this.IsCrit = isCrit;
        HitLayer = hitLayer;
        Parent = parent;
        startPosition = transform.position;
    }
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        if (((1 << other.gameObject.layer) & HitLayer) != 0)
        {
            CharacterCTRL enemy = other.GetComponent<CharacterCTRL>();
            if (enemy != null)
            {
                HitTarget(enemy);
            }
        }
    }
    void Update()
    {
        Vector3 targetPosWithFixedY = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        transform.position += speed * Time.deltaTime * (targetPosWithFixedY - transform.position).normalized;
        transform.LookAt(targetPosWithFixedY);
        Quaternion additionalRotation = Quaternion.Euler(90, 0, 0);
        transform.rotation *= additionalRotation;
        CheckMaxDistance();
    }
    void HitTarget(CharacterCTRL enemy)
    {
        enemy.GetHit((int)damage, Parent,IsCrit);
        damage *= decayFactor;
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
            gameObject.SetActive(false); // ¸T¥Î¤l¼u
        }
    }
}
