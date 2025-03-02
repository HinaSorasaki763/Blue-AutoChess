using System.Collections.Generic;

public class SupermacyObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(15)},
            {1, new TraitLevelStats(0)},
            {2, new TraitLevelStats(0)},
            {3, new TraitLevelStats(30)}
        };
        return statsByStarLevel;
    }
    public SupermacyObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        int threshold = (int)(target.GetStat(GameEnum.StatsType.Health) * GetTraitObserverLevel()[traitLevel].Data1 * 0.01f);
        if (target.GetStat(GameEnum.StatsType.currHealth) <= threshold)
        {
            target.Executed(source, detailedSource);
        }
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
    }
}
