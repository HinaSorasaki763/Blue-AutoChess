using GameEnum;
using System.Collections.Generic;

public class SupermacyObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0)},
            {1, new TraitLevelStats(15)},
            {2, new TraitLevelStats(0)},
            {3, new TraitLevelStats(30)}
        };
        return statsByStarLevel;
    }
    public SupermacyObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.character = character;
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        Utility.CheckExecuted(target,source, GetTraitObserverLevel()[traitLevel].Data1 * 0.01f,detailedSource);
    }
}
