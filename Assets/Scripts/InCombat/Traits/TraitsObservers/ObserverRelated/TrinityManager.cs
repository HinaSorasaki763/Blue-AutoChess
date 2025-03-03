using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TrinityManager : MonoBehaviour
{
    public int TrinityStackCount = 0;
    public static TrinityManager Instance { get; set; }
    public GameObject cometPrefab;        // 彗星的Prefab
    public GameObject groundEffectPrefab; // 地面特效的Prefab
    public GameObject Effect;
    public float fallDuration = 2f;       // 彗星墜落的時間
    public float fallHeight = 20f;        // 彗星的起始高度
    public void OnEnable()
    {
        Instance = this;
    }
    public void AddStack(Vector3 v,string detailSource,HexNode h,CharacterCTRL c )
    {
        var observer = c.traitController.GetObserverForTrait(Traits.Trinity) as TrinityObserver;
        int StackUplimit = observer.GetTraitObserverLevel()[observer.traitLevel].Data2;
        TrinityStackCount++;
        if (TrinityStackCount >=StackUplimit)
        {
            TrinityStackCount -= StackUplimit;
            TriggerComet(v,detailSource,h,c);
        }
    }
    public void TriggerComet(Vector3 targetPosition, string detailSource,HexNode h,CharacterCTRL c )
    {
        // 生成地面特效
        GameObject groundEffect = Instantiate(groundEffectPrefab, targetPosition, Quaternion.identity);
        Vector3 currentRotation = groundEffect.transform.eulerAngles;
        currentRotation.x += 90f;
        groundEffect.transform.eulerAngles = currentRotation;
        groundEffect.transform.position += new Vector3(0, 0.15f, 0);
        // 計算彗星的初始位置（斜上方）
        Vector3 offset = new Vector3(-20f, fallHeight, 0f); // X 軸偏移 5，Y 軸高度
        Vector3 cometStartPosition = targetPosition + offset;

        // 計算彗星的朝向
        Quaternion cometRotation = Quaternion.LookRotation(targetPosition - cometStartPosition);

        // 生成彗星物件
        GameObject comet = Instantiate(cometPrefab, cometStartPosition, cometRotation);

        // 開始彗星墜落協程
        StartCoroutine(FallComet(comet, targetPosition, groundEffect, detailSource,h,c));
    }

    private IEnumerator FallComet(GameObject comet, Vector3 targetPosition, GameObject groundEffect,string detailSource,HexNode h,CharacterCTRL c)
    {
        Vector3 startPosition = comet.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fallDuration);
            comet.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        comet.transform.position = targetPosition;
        var observer = c.traitController.GetObserverForTrait(Traits.Trinity) as TrinityObserver;
        int dmg = observer.GetCuurDmg();
        (bool iscrit, int dmg1) = c.CalculateCrit(dmg);
        foreach (var item in Utility.GetCharacterInrange(h,1,c,false))
        {
            item.GetHit(dmg1,c,detailSource, iscrit);
        }
        CustomLogger.Log(this, $"item.GetHit({dmg1},{c},{detailSource}, {iscrit});");
        GameObject instantiatedEffect = Instantiate(Effect, targetPosition, Quaternion.identity);
        Destroy(instantiatedEffect, 2f);
        Destroy(groundEffect, 2f);
        Destroy(comet);
    }

}
