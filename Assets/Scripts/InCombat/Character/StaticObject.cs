using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : CharacterCTRL
{
    private Quaternion originalRotation;
    private float lastShakeTime = 0f; // 上次旋轉效果觸發的時間
    private float shakeCooldown = 0.7f; // 旋轉效果的冷卻時間（秒）
    public override void OnEnable()
    {
        isObj = true;
        isTargetable = true;
        IsAlly = true;
        stats = characterStats.Stats.Clone();
        effectCTRL = GetComponent<EffectCTRL>();
        modifierCTRL = GetComponent<ModifierCTRL>();
        traitController = GetComponent<TraitController>();
        ResetStats();
        originalRotation = transform.rotation;
    }
    public override void Update()
    {

    }
    public override void GetHit(int amount, CharacterCTRL sourceCharacter)
    {
        base.GetHit(amount, sourceCharacter);
        if (Time.time - lastShakeTime >= shakeCooldown)
        {
            lastShakeTime = Time.time; // 更新上次旋轉的時間
            StartCoroutine(ShakeRotation());
        }
    }
    public override IEnumerator Die()
    {
        Debug.Log($"{gameObject.name} Die()");
        SpawnGrid.Instance.RemoveCenterPoint(CurrentHex);
        CurrentHex.OccupyingCharacter = null;
        CurrentHex.HardRelease();
        gameObject.SetActive(false);
        return null;
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
