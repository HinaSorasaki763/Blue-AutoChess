using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinityManager : MonoBehaviour
{
    public int TrinityStackCount = 0;
    public static TrinityManager Instance { get; set; }
    public GameObject cometPrefab;        // �k�P��Prefab
    public GameObject groundEffectPrefab; // �a���S�Ī�Prefab
    public float fallDuration = 2f;       // �k�P�Y�����ɶ�
    public float fallHeight = 20f;        // �k�P���_�l����
    readonly int StackUplimit = 2;
    public void OnEnable()
    {
        Instance = this;
    }
    public void AddStack(Vector3 v)
    {
        TrinityStackCount++;
        if (TrinityStackCount >=StackUplimit)
        {
            TrinityStackCount -= StackUplimit;
            TriggerComet(v);
        }
    }
    public void TriggerComet(Vector3 targetPosition)
    {
        // �ͦ��a���S��
        GameObject groundEffect = Instantiate(groundEffectPrefab, targetPosition, Quaternion.identity);
        Vector3 currentRotation = groundEffect.transform.eulerAngles;
        currentRotation.x += 90f;
        groundEffect.transform.eulerAngles = currentRotation;
        groundEffect.transform.position += new Vector3(0, 0.15f, 0);
        // �p��k�P����l��m�]�פW��^
        Vector3 offset = new Vector3(-20f, fallHeight, 0f); // X �b���� 5�AY �b����
        Vector3 cometStartPosition = targetPosition + offset;

        // �p��k�P���¦V
        Quaternion cometRotation = Quaternion.LookRotation(targetPosition - cometStartPosition);

        // �ͦ��k�P����
        GameObject comet = Instantiate(cometPrefab, cometStartPosition, cometRotation);

        // �}�l�k�P�Y����{
        StartCoroutine(FallComet(comet, targetPosition, groundEffect));
    }

    private IEnumerator FallComet(GameObject comet, Vector3 targetPosition, GameObject groundEffect)
    {
        Vector3 startPosition = comet.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fallDuration);

            // ���k�P�u�۪��u�Y���츨�I
            comet.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        // �k�P��F�a��
        comet.transform.position = targetPosition;

        // �P���a���S�ĩM�k�P
        Destroy(groundEffect, 2f); // �a���S��2������
        Destroy(comet);
    }

}
