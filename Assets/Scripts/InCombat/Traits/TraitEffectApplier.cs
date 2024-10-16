using GameEnum;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TraitEffectApplier
{
    private Dictionary<Traits, Action<int, CharacterCTRL>> traitEffectMap;

    public TraitEffectApplier()
    {
        // 初始化映射，每個 Traits 對應一個處理方法
        traitEffectMap = new Dictionary<Traits, Action<int, CharacterCTRL>>()
        {
            { Traits.Abydos, ApplyAbydosEffect },
            { Traits.Gehenna, ApplyGehennaEffect },
            { Traits.Hyakkiyako, ApplyHyakkiyakoEffect },
            { Traits.Millennium, ApplyMillenniumEffect },
            { Traits.Trinity, ApplyTrinityEffect },
        };
    }

    public void ApplyTraitEffects(Traits trait, int traitLevel, CharacterCTRL character)
    {
        // 如果字典中存在該羈絆的處理方法，則調用它
        if (traitEffectMap.TryGetValue(trait, out var applyEffect))
        {
            applyEffect(traitLevel, character);
        }
        else
        {
            Debug.Log($"未定義 {trait} 的羈絆效果");
        }
    }

    private void ApplyAbydosEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Abydos 羈絆等級 {traitLevel}，應用特定效果");
    }

    private void ApplyGehennaEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Gehenna 羈絆等級 {traitLevel}，應用特定效果");
    }
    private void ApplyHyakkiyakoEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Hyakkiyako 羈絆等級 {traitLevel}，應用特定效果");
    }
    private void ApplyMillenniumEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Millennium 羈絆等級 {traitLevel}，應用特定效果");
    }
    private void ApplyTrinityEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Trinity 羈絆等級 {traitLevel}，應用特定效果");
    }
}
