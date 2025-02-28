using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//¼Æ­È­¿²v
            {0, new TraitLevelStats(100)},
            {1, new TraitLevelStats(120)},
            {2, new TraitLevelStats(150)}
        };
        return statsByStarLevel;
    }
    public int GetCurrStat()
    {
        return GetTraitObserverLevel()[traitLevel].Data1;
    }
    public LogisticObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
    }
}
