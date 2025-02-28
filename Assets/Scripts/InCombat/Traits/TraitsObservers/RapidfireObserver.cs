using GameEnum;
using System.Collections.Generic;

public class RapidfireObserver : CharacterObserverBase
{
    private CharacterCTRL parent;
    private Effect RapidfireObserverEffect;
    private int level;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0)},
            {1, new TraitLevelStats(5)},
            {2, new TraitLevelStats(7)},
            {3, new TraitLevelStats(10)},
            {4, new TraitLevelStats(20)},
        };
        return statsByStarLevel;
    }
    public RapidfireObserver(int level, CharacterCTRL character)
    {
        this.level = level;
        this.parent = character;
        character.effectCTRL.characterCTRL = character;
        Effect effect = EffectFactory.StatckableStatsEffct(0,"RapidfireObserver", 0, StatsType.AttackSpeed,parent,true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
        );
        parent.effectCTRL.AddEffect(effect);
        RapidfireObserverEffect = effect;
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        if (!activated) return;
        base.OnAttacking(character);
        CustomLogger.Log(this,$"character {character } override onAttacking , attakspeed = {character.GetStat(StatsType.AttackSpeed)}");
        float val = GetTraitObserverLevel()[level].Data1 * 0.01f;
        RapidfireObserverEffect.AddValue(val);
    }
}
