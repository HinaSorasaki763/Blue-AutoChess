// GehennaObserver.cs
using System.Collections.Generic;
using UnityEngine;

public class GehennaObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public GehennaObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        traitLevel = level;
        this.character = character;
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0)},
            {1, new TraitLevelStats(1,0)},
            {2, new TraitLevelStats(2,0)},
            {3, new TraitLevelStats(3,0)},
            {4, new TraitLevelStats(5,0)},
        };
        return statsByStarLevel;
    }
    public override void ActivateTrait()
    {
        PressureManager.Instance.UpdateIndicater();
    }
    public override void DeactivateTrait()
    {
        PressureManager.Instance.UpdateIndicater();
    }
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {
        int amount = GetTraitObserverLevel()[traitLevel].Data1;
        if (character.IsAlly)
        {
            PressureManager.Instance.IncreasePressure(amount);
        }
        else
        {
            EnemyTraitRelated.Instance.AddPressure(amount);
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
            Debug.Log($"[GehennaObserver] 回合胜利，原有的威壓：{PressureManager.Instance.GetPressure(true)}，威压层数增加 {GameController.Instance.GetAliveCount()}。");
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
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        List<int> i = new List<int>() { 3, 4, 504, 12, 15, 17, 25 };//角色編號
        if (detailedSource != "GehennaTraitDamage" && !i.Contains(source.characterStats.CharacterId))
        {
            int dmg = damage * (int)(PressureManager.Instance.GetPressure(source.IsAlly) * 0.01f);
            target.GetHit(dmg, source, "GehennaTraitDamage", iscrit);
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
