using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysticObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(100)},
            {1, new TraitLevelStats(80)},
            {2, new TraitLevelStats(50)},
            {3, new TraitLevelStats(0)}
        };
        return statsByStarLevel;
    }
    public override int BeforeDealtDmg(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        int ratio = GetTraitObserverLevel()[traitLevel].Data1;
        
        int reduced = (int)(damage * (1-ratio*0.01f));
        target.GetHitByTrueDamage(reduced, source, "Mythic", iscrit);
        return reduced;
    }
    public MysticObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
    }
}
