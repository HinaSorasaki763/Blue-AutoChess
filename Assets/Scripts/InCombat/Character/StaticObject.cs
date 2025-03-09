using GameEnum;
using System.Collections;
using UnityEngine;

public class StaticObject : CharacterCTRL
{
    private Quaternion originalRotation;
    private float lastShakeTime = 0f; // �W������ĪGĲ�o���ɶ�
    private float shakeCooldown = 0.7f; // ����ĪG���N�o�ɶ��]��^
    public CharacterCTRL parent;

    public override void OnEnable()
    {
        star = 1;
        isObj = true;
        isTargetable = true;
        IsAlly = true;
        gameObject.layer = IsAlly ? 8 : 9;
        stats = characterStats.Stats.Clone();
        effectCTRL = GetComponent<EffectCTRL>();
        modifierCTRL = GetComponent<ModifierCTRL>();
        traitController = GetComponent<TraitController>();
        equipmentManager = GetComponent<CharacterEquipmentManager>();
        equipmentManager.SetParent(this);
        AudioManager = GetComponent<CharacterAudioManager>();
        ActiveSkill = characterSkills[0]();

        originalRotation = transform.rotation;
        initStats();
    }
    public override void GetHit(int amount, CharacterCTRL sourceCharacter, string detailedSource, bool isCrit)
    {
        if (!isAlive) return;
        base.GetHit(amount, sourceCharacter, detailedSource, isCrit);
        if (isAlive && gameObject.activeInHierarchy && Time.time - lastShakeTime >= shakeCooldown)
        {
            lastShakeTime = Time.time;
            StartCoroutine(ShakeRotation());
        }

    }
    public void Start()
    {

    }
    public void initStats()
    {
        SetStat(StatsType.Health, GetStat(StatsType.Health));
        SetStat(StatsType.currHealth, GetStat(StatsType.Health));
    }
    public void RefreshDummy(CharacterCTRL c)
    {
        parent = c;
        IsAlly = c.IsAlly;
        gameObject.layer = IsAlly ? 8 : 9;
        SetStats();
        characterBars.InitBars();
    }
    public void SetStats()
    {
        var observer = parent.traitController.GetObserverForTrait(Traits.logistic) as LogisticObserver;
        float ratio = observer.GetCurrStat() * 0.01f;
        int health = (int)(parent.GetStat(StatsType.Health) * ratio);
        int def = (int)(parent.GetStat(StatsType.Resistence) * ratio);
        int percentageResistance = (int)(parent.GetStat(StatsType.PercentageResistence) * ratio);
        SetStat(StatsType.Health, health);
        SetStat(StatsType.currHealth, health);
        SetStat(StatsType.Resistence, def);
        SetStat(StatsType.PercentageResistence, percentageResistance);
    }
    public override void Die()
    {
        Debug.Log($"{gameObject.name} Die()");
        CurrentHex.OccupyingCharacter = null;
        CurrentHex.HardRelease();
        gameObject.SetActive(false);
        StopAllCoroutines();
    }
    private IEnumerator ShakeRotation()
    {
        float duration = 0.5f; // ����ĪG������ɶ�
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // �H���ܤƱ��ਤ��
            float xRotation = UnityEngine.Random.Range(-5f, 5f); // �]�w�p�T�ת��H�����ਤ��
            float yRotation = UnityEngine.Random.Range(-5f, 5f);
            float zRotation = UnityEngine.Random.Range(-5f, 5f);

            // ��s���骺����
            transform.rotation = originalRotation * Quaternion.Euler(xRotation, yRotation, zRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation;
    }
}
