
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameEnum;

public class ModifierCTRL : MonoBehaviour
{
    private List<StatModifier> statModifiers = new List<StatModifier>();

    // 添加修正器的方法
    public void AddStatModifier(ModifierType modifierType, float value, string source, bool isPermanent, float duration = 0f)
    {
        // 查找是否已有相同来源和属性类型的修正器
        StatModifier existingModifier = statModifiers.FirstOrDefault(mod => mod.ModifierType == modifierType && mod.Source == source);

        if (existingModifier != null)
        {
            // 判断新修正器是否更有效
            if (IsNewModifierMoreEffective(modifierType, value, existingModifier.Value))
            {
                // 覆盖旧的修正器
                existingModifier.Value = value;
                existingModifier.IsPermanent = isPermanent;
                existingModifier.Duration = duration;
            }
            else
            {
                // 旧的修正器更有效，保持不变
                return;
            }
        }
        else
        {
            // 添加新的修正器
            StatModifier newModifier = new StatModifier(modifierType, value, source, isPermanent, duration);
            statModifiers.Add(newModifier);
        }
    }

    // 移除修正器的方法
    public void RemoveStatModifier(ModifierType modifierType, string source)
    {
        StatModifier existingModifier = statModifiers.FirstOrDefault(mod => mod.ModifierType == modifierType && mod.Source == source);
        if (existingModifier != null)
        {
            statModifiers.Remove(existingModifier);
        }
    }

    // 判断新修正器是否更有效
    private bool IsNewModifierMoreEffective(ModifierType modifierType, float newValue, float existingValue)
    {
        if (modifierType == ModifierType.DamageTaken)
        {
            // 受到伤害的修正器，数值越小（更负），效果越好
            return newValue < existingValue;
        }
        else if (modifierType == ModifierType.DamageDealt)
        {
            // 造成伤害的修正器，数值越大，效果越好
            return newValue > existingValue;
        }
        else
        {
            return false;
        }
    }

    // 在 Update 方法中更新暂时修正器的持续时间
    public void UpdateStatModifiers(float deltaTime)
    {
        List<StatModifier> modifiersToRemove = new List<StatModifier>();

        foreach (var modifier in statModifiers)
        {
            if (!modifier.IsPermanent)
            {
                modifier.Duration -= deltaTime;
                if (modifier.Duration <= 0f)
                {
                    modifiersToRemove.Add(modifier);
                }
            }
        }

        // 移除已过期的修正器
        foreach (var modifier in modifiersToRemove)
        {
            statModifiers.Remove(modifier);
        }
    }

    // 获取特定属性类型的修正器总值
    public float GetTotalStatModifierValue(ModifierType modifierType)
    {
        float totalValue = 0f;
        foreach (var modifier in statModifiers.Where(mod => mod.ModifierType == modifierType))
        {
            totalValue += modifier.Value;
        }
        return totalValue;
    }
}
