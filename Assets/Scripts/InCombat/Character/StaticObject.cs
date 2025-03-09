using GameEnum;
using System.Collections;
using UnityEngine;

public class StaticObject : CharacterCTRL
{
    private Quaternion originalRotation;
    private float lastShakeTime = 0f; // 上次旋轉效果觸發的時間
    private float shakeCooldown = 0.7f; // 旋轉效果的冷卻時間（秒）
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
        float duration = 0.5f; // 旋轉效果的持續時間
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 隨機變化旋轉角度
            float xRotation = UnityEngine.Random.Range(-5f, 5f); // 設定小幅度的隨機旋轉角度
            float yRotation = UnityEngine.Random.Range(-5f, 5f);
            float zRotation = UnityEngine.Random.Range(-5f, 5f);

            // 更新物體的旋轉
            transform.rotation = originalRotation * Quaternion.Euler(xRotation, yRotation, zRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation;
    }
}
