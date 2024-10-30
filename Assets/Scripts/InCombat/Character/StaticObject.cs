using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : CharacterCTRL
{
    private Quaternion originalRotation;
    private float lastShakeTime = 0f; // �W������ĪGĲ�o���ɶ�
    private float shakeCooldown = 0.7f; // ����ĪG���N�o�ɶ��]��^
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
            lastShakeTime = Time.time; // ��s�W�����઺�ɶ�
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
