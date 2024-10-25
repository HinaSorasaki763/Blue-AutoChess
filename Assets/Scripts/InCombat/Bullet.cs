using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    private float Dmg = 0;
    private Vector3 targetPosition;
    private CharacterCTRL Target;
    private CharacterCTRL Parent;
    public void OnEnable()
    {
        targetPosition = Vector3.zero;
        Target = null;
    }
    public void SetTarget(Vector3 target,CharacterCTRL characterCTRL)
    {
        targetPosition = target;
        Target = characterCTRL;
    }
    public void SetParent(CharacterCTRL parent)
    {
        Parent = parent;
    }
    public void SetDmg(float dmg)
    {
        Dmg = dmg;
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * (targetPosition - transform.position).normalized;
        //transform.LookAt(targetPosition);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            HitTarget();
        }
    }
    void HitTarget()
    {
        if (!Target.gameObject.activeInHierarchy) return;
        Debug.Log($"Target {Target.characterStats.name} get hit {Dmg}");
        Target.GetHit((int)Dmg,Parent);
        gameObject.SetActive(false);
    }
    public void OnDisable()
    {
        Parent = null;
        Target = null;
    }
}
