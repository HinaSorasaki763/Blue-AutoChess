using UnityEngine;
using System.Collections.Generic;
using GameEnum;
using Newtonsoft.Json.Serialization;

public class TextEffectPool : MonoBehaviour
{
    private List<TextEffect> activeEffects = new List<TextEffect>(); // 記錄目前活躍的 TextEffect
    private const float minOffset = 25.0f; // 最小間距
    public TextEffect textEffectPrefab;
    private List<TextEffect> pool = new List<TextEffect>();
    public List<Sprite> effectImages = new();
    public static TextEffectPool Instance { get; set; }
    public void OnEnable()
    {
        Instance = this;
    }
    public TextEffect GetTextEffect()
    {
        foreach (var effect in pool)
        {
            if (!effect.gameObject.activeInHierarchy)
            {
                return effect;
            }
        }

        TextEffect newEffect = Instantiate(textEffectPrefab, transform);
        pool.Add(newEffect);
        return newEffect;
    }
    public Sprite GetEffectSprite(BattleDisplayEffect effect)
    {
        switch (effect)
        {
            case BattleDisplayEffect.Weak:
                return effectImages[0];
            case BattleDisplayEffect.Resist:
                return effectImages[1];
            case BattleDisplayEffect.Immune:
                return effectImages[2];
            case BattleDisplayEffect.Block:
                return effectImages[3];
            case BattleDisplayEffect.Miss:
                return effectImages[4];
            case BattleDisplayEffect.None:
                return null;
            default:
                CustomLogger.LogError(this,"waring!");
                return null;
        }
    }
    public void ShowTextEffect(BattleDisplayEffect effect, int number, Vector3 screenPosition, bool empty, bool healing = false)
    {
        CustomLogger.Log(this, $"ShowTextEffect called ,{effect},{number},{screenPosition},{empty},{healing}");
        Sprite sprite = GetEffectSprite(effect);
        TextEffect textEffect = GetTextEffect();
        if (number >= 99999) number = 99999;
        // 根據當前所有活躍的特效動態調整 Y 軸，確保不會重疊
        screenPosition = AdjustPositionDynamically(screenPosition);

        textEffect.Initialize(effect, sprite, number, screenPosition, empty, healing);

        activeEffects.Add(textEffect); // 加入活躍特效清單

        textEffect.OnEffectFinished += () => activeEffects.Remove(textEffect); // 當效果消失時，移除
    }
    private Vector3 AdjustPositionDynamically(Vector3 basePosition)
    {
        foreach (var effect in activeEffects)
        {
            RectTransform effectTransform = effect.GetComponent<RectTransform>();
            float currentY = effectTransform.position.y;

            // 若新位置與任何活躍特效的當前位置過於接近，則上移
            if (Mathf.Abs(currentY - basePosition.y) < minOffset)
            {
                basePosition.y += minOffset;
            }
        }
        return basePosition;
    }

}
