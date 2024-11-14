using UnityEngine;
using System.Collections.Generic;
using GameEnum;
using Newtonsoft.Json.Serialization;

public class TextEffectPool : MonoBehaviour
{
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
    public void ShowTextEffect(BattleDisplayEffect effect, int number, Vector3 screenPosition,bool empty)
    {
        Sprite sprite = GetEffectSprite(effect);
        TextEffect textEffect = GetTextEffect();
        textEffect.Initialize(sprite, number, screenPosition,empty);
    }

}
