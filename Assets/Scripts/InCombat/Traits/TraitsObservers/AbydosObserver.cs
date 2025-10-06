// AbydosObserver.cs
using UnityEngine;
using GameEnum;
using System.Collections.Generic;
public class AbydosObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    private bool isOnDesertTile;
    public AbydosObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.character = character;
        UpdateDesertifiedTiles();
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(8,30,2)},
            {2, new TraitLevelStats(10,45,3)},
            {3, new TraitLevelStats(12,70,5)},
            {4, new TraitLevelStats(20,100,10)}
        };
        return statsByStarLevel;
    }
    private void UpdateDesertifiedTiles()
    {
        CustomLogger.Log(this, $"traitLevel = {traitLevel}");
        AbydosManager.Instance.level = traitLevel;
        SpawnGrid.Instance.UpdateDesertifiedTiles(ResourcePool.Instance.RandomKeyThisGame, GetTraitObserverLevel()[traitLevel].Data1);
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        if (SelectedAugments.Instance.CheckIfConditionMatch(102,character.IsAlly))
        {
            character.AddPercentageBonus(StatsType.Null, StatsType.Health, AbydosManager.Instance.Augment102_EarnedCount * 5, "Augment102");
            character.AddPercentageBonus(StatsType.Null, StatsType.Attack, AbydosManager.Instance.Augment102_EarnedCount * 5, "Augment102");
            character.AddPercentageBonus(StatsType.Null, StatsType.AttackSpeed, AbydosManager.Instance.Augment102_EarnedCount * 5, "Augment102");
            character.AddPercentageBonus(StatsType.Null, StatsType.Resistence, AbydosManager.Instance.Augment102_EarnedCount * 5, "Augment102");
        }
        
        if (SelectedAugments.Instance.CheckIfConditionMatch(107, character.IsAlly))
        {
            AriusManager.Instance.AddArius(character);
        }
    }
}
