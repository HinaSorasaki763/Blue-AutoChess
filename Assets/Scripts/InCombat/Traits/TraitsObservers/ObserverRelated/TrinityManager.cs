using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrinityManager : MonoBehaviour
{
    public int TrinityStackCount = 0;
    public static TrinityManager Instance { get; set; }
    public GameObject cometPrefab;        // 彗星的Prefab
    public GameObject groundEffectPrefab; // 地面特效的Prefab
    public GameObject Effect;
    public float fallDuration = 2f;       // 彗星墜落的時間
    public float fallHeight = 20f;        // 彗星的起始高度
    public List<Sprite> cakes;
    public Sprite plate;
    public GameObject mikaEmote;
    [SerializeField] private Canvas mainCanvas; // 指向你的主 UI Canvas
    [SerializeField] private GameObject imagePrefab; // 一個有 Image 的預製物 (Image component 必須掛好)

    public void OnEnable()
    {
        Instance = this;
    }
    public void AddStack(Vector3 v, string detailSource, HexNode h, CharacterCTRL c)
    {
        var observer = c.traitController.GetObserverForTrait(Traits.Trinity) as TrinityObserver;
        int StackUplimit = observer.GetTraitObserverLevel()[observer.traitLevel].Data2;
        TrinityStackCount++;
        if (TrinityStackCount >= StackUplimit)
        {
            TrinityStackCount -= StackUplimit;
            TriggerComet(v, detailSource, h, c);
        }
    }
    public void TriggerComet(Vector3 targetPosition, string detailSource, HexNode h, CharacterCTRL c)
    {
        
        if (SelectedAugments.Instance.CheckAugmetExist(122,c.IsAlly))
        {
            HexNode he = SpawnGrid.Instance.GetHexNodeByPosition(targetPosition);
            targetPosition = SpawnGrid.Instance.FindBestHexNode(c, 2, true, false, he).Position;
        }
        if (SelectedAugments.Instance.CheckAugmetExist(118,c.IsAlly))
        {
            HexNode he = SpawnGrid.Instance.GetHexNodeByPosition(targetPosition);
            targetPosition = SpawnGrid.Instance.FindBestHexNode(c, 2, false, false, he).Position;
        }
        if (SelectedAugments.Instance.CheckAugmetExist(127, c.IsAlly))
        {
            GiveDessert(c);
        }
        GameObject groundEffect = Instantiate(groundEffectPrefab, targetPosition, Quaternion.identity);
        Vector3 currentRotation = groundEffect.transform.eulerAngles;
        currentRotation.x += 90f;
        groundEffect.transform.eulerAngles = currentRotation;
        groundEffect.transform.position += new Vector3(0, 0.15f, 0);
        Vector3 offset = new Vector3(-20f, fallHeight, 0f);
        Vector3 cometStartPosition = targetPosition + offset;
        Quaternion cometRotation = Quaternion.LookRotation(targetPosition - cometStartPosition);
        GameObject comet = Instantiate(cometPrefab, cometStartPosition, cometRotation);
        StartCoroutine(FallComet(comet, targetPosition, groundEffect, detailSource, h, c));
    }
    public void GiveDessert(CharacterCTRL c)
    {
        CharacterParent characterParent = c.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        CharacterCTRL character = characterParent.GetRandomCharacter(false);
        var observer = c.traitController.GetObserverForTrait(Traits.Trinity) as TrinityObserver;
        int data4 = observer.GetData4();
        if (character != null)
        {
            if (character.GetStat(StatsType.Range) <= 2)
            {
                if (character.GetHealthPercentage() >= 0.8f)
                {

                    int amount = (int)(character.GetStat(StatsType.Health) * 0.1f + data4 * 10);
                    character.AddShield(amount, 5, c);
                }
                else
                {
                    int amount = (int)(character.GetStat(StatsType.Health) * 0.1f + data4 * 10);
                    character.Heal(amount, c);
                }
            }
            else
            {
                if (character.GetStat(StatsType.AttackSpeed) >= 1.5f)
                {
                    int amount = data4;
                    Effect effect = EffectFactory.UnStatckableStatsEffct(5f, "TrinityEffect", amount, StatsType.Attack, c, false);
                    effect.SetActions(
                        (character) => character.ModifyStats(StatsType.Attack, effect.Value, effect.Source),
                        (character) => character.ModifyStats(StatsType.Attack, -effect.Value, effect.Source)
                    );
                    character.effectCTRL.AddEffect(effect, c);
                }
                else
                {
                    int amount = data4;
                    Effect effect = EffectFactory.UnStatckableStatsEffct(5f, "TrinityEffect", amount * 0.01f, StatsType.AttackSpeed, c, false);
                    effect.SetActions(
                        (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
                        (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
                    );
                    character.effectCTRL.AddEffect(effect, c);
                }
            }
            PlayAnimation(character);
        }

    }
    private void PlayAnimation(CharacterCTRL c)
    {
        if (c.characterStats.CharacterId == 27)
        {
            GameObject rollcakeGO = Instantiate(mikaEmote, mainCanvas.transform);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(c.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainCanvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
            float prefix = c.transform.position.x * 5f;
            rollcakeGO.transform.GetComponent<RectTransform>().anchoredPosition = localPos + new Vector2(prefix, 115);
            Transform rollcakeChild = rollcakeGO.transform.Find("rollcake");
            if (rollcakeChild == null)
            {
                CustomLogger.LogError(this, "rollcake child not found.");
                return;
            }

            rollcakeChild.localScale = new Vector3(0.5f, 0.5f, 1f);
            StartCoroutine(ScaleRollCakeCoroutine(rollcakeChild, new Vector3(0.2f, 0.5f, 1f), 3f));
            Destroy(rollcakeGO, 3f);
        }
        else
        { // 加點高度
            Vector3 screenPos = Camera.main.WorldToScreenPoint(c.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainCanvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
            float prefix = c.transform.position.x * 5f;
            int randIndex = Random.Range(0, cakes.Count);
            Sprite selectedCake = cakes[randIndex];

            // 建立 plate
            GameObject plateGO = Instantiate(imagePrefab, mainCanvas.transform);
            RectTransform plateRect = plateGO.GetComponent<RectTransform>();
            Image plateImage = plateGO.GetComponent<Image>();

            plateImage.sprite = plate;
            plateRect.sizeDelta = new Vector2(123, 70);
            plateRect.transform.GetComponent<RectTransform>().anchoredPosition = localPos + new Vector2(prefix, 110);

            // 建立 cake 作為子物件
            GameObject cakeGO = Instantiate(imagePrefab, plateRect);
            RectTransform cakeRect = cakeGO.GetComponent<RectTransform>();
            Image cakeImage = cakeGO.GetComponent<Image>();

            cakeImage.sprite = selectedCake;
            cakeRect.sizeDelta = new Vector2(114, 126);
            cakeRect.anchoredPosition = new Vector2(0, 50);

            // 3 秒後刪除 plate（cake 是其子物件也會一起刪）
            Destroy(plateGO, 3f);
        }
    }
    private IEnumerator ScaleRollCakeCoroutine(Transform target, Vector3 endScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale;
    }

    private IEnumerator FallComet(GameObject comet, Vector3 targetPosition, GameObject groundEffect, string detailSource, HexNode h, CharacterCTRL c)
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
        if (c == null) yield break;
        comet.transform.position = targetPosition;
        if (SelectedAugments.Instance.CheckAugmetExist(105, c.IsAlly))
        {
            h.TempDesert = true;
            h.TempDesert1 = false;
        }
        var observer = c.traitController.GetObserverForTrait(Traits.Trinity) as TrinityObserver;
        int dmg = observer.GetCuurDmg();
        (bool iscrit, int dmg1) = c.CalculateCrit(dmg);

        int range = 1;
        if (SelectedAugments.Instance.CheckAugmetExist(113, c.IsAlly))
        {
            float ratio = PressureManager.Instance.GetPressure(true) * 0.01f;
            dmg1 = (int)(dmg1 * (1 + ratio));
            PressureManager.Instance.AddPressure(5, c.IsAlly);
            range = 2;
        }
        if (SelectedAugments.Instance.CheckAugmetExist(122, c.IsAlly))
        {
            range++;
        }
        if (SelectedAugments.Instance.CheckAugmetExist(118, c.IsAlly))
        {
            foreach (var item in Utility.GetCharacterInrange(h, range, c, false))
            {
                item.Heal(dmg, c);
            }
            foreach (var item in Utility.GetCharacterInrange(h, range, c, true))
            {
                item.Heal(dmg, c);
            }
            int rand = Utility.GetRand(c);
            if (rand <= 10)
            {
                ResourcePool.Instance.GetGoldPrefab(c.transform.position);
            }
            else if (rand <= 75)
            {
                ResourcePool.Instance.GetGoldPrefab(c.transform.position);
            }
            else
            {
                ResourcePool.Instance.GetRandRewardPrefab(c.transform.position);
            }
            GameObject ef = Instantiate(Effect, targetPosition, Quaternion.identity);
            
            Destroy(ef, 2f);
            Destroy(groundEffect, 2f);
            Destroy(comet);
            yield return null;
        }
        foreach (var item in Utility.GetCharacterInrange(h, range, c, false))
        {
            item.GetHit(dmg1, c, detailSource, iscrit);
        }
        CustomLogger.Log(this, $"item.GetHit({dmg1},{c},{detailSource}, {iscrit});");
        GameObject instantiatedEffect = Instantiate(Effect, targetPosition, Quaternion.identity);
        Destroy(instantiatedEffect, 2f);
        Destroy(groundEffect, 2f);
        Destroy(comet);
    }

}
