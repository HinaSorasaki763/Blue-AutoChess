using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0)},
            {1, new TraitLevelStats(20)},
            {2, new TraitLevelStats(30)},
            {3, new TraitLevelStats(50)}
        };
        return statsByStarLevel;
    }
    public HealerObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.character = character;
    }
    public override int BeforeHealing(CharacterCTRL characterCTRL, int amount)
    {
        float ratio = GetTraitObserverLevel()[traitLevel].Data1 * 0.01f;
        amount = (int)(amount* (1+ratio));
        return base.BeforeHealing(characterCTRL, amount);
    }
}
