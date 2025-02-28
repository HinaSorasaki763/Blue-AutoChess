using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRTObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {1, new TraitLevelStats(3,30,80)},
            {2, new TraitLevelStats(5,45,90)},
            {3, new TraitLevelStats(10,70,100)}
        };
        return statsByStarLevel;
    }
    public SRTObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
    }
}
