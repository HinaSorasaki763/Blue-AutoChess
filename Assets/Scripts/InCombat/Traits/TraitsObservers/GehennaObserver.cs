// GehennaObserver.cs
using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class GehennaObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public GehennaObserver(int level, CharacterCTRL character)
    {
        traitLevel = level;
        this.character = character;
    }

    public override void OnKilledEnemy(CharacterCTRL character)
    {
        if (character.IsAlly)
        {
            PressureManager.Instance.IncreasePressure(1);
        }

        Debug.Log($"[GehennaObserver] {character.name} 击杀了敌方单位，威压层数增加 1。");
    }

    public override void OnBattleEnd(bool isVictory)
    {
        if (isVictory)
        {
            if (character.IsAlly)
            {
                PressureManager.Instance.IncreasePressure(1);
            }
            Debug.Log($"[GehennaObserver] 回合胜利，原有的威壓：{PressureManager.Instance.GetPressure()}，威压层数增加 {GameController.Instance.GetAliveCount()}。");
        }
    }

    public override void OnCastedSkill(CharacterCTRL character)
    {
        if (character == this.character)
        {
            // 根据威压层数增强技能
            EnhanceSkillBasedOnPressure();
        }
    }

    private void EnhanceSkillBasedOnPressure()
    {
        int pressure = PressureManager.Instance.CurrentPressure;
        // 假设每层威压增加1%的技能效果
        float enhancementFactor = 1 + (pressure * 0.01f);
        //character.characterStats.ApplySkillEnhancement(enhancementFactor);
        Debug.Log($"[GehennaObserver] {character.name} 的技能根据威压层数增强，当前增强倍率：{enhancementFactor}");
    }
}
