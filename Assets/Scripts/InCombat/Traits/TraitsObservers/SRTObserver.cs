using GameEnum;
using System.Collections.Generic;

public class SRTObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            //空,攻擊力,防禦力,生命,攻擊速度
            {0, new TraitLevelStats(0,5,5,10,0.01f)},
            {1, new TraitLevelStats(0,5,5,10,0.01f)},
            {2, new TraitLevelStats(0,7,7,14,0.02f)},
            {3, new TraitLevelStats(0,10,10,20,0.04f)}
        };
        return statsByStarLevel;
    }
    public SRTObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.character = character;
    }
    public override void ActivateTrait()
    {
        if (SelectedAugments.Instance.CheckAugmetExist(107, true))
        {
            CharacterParent c = ResourcePool.Instance.ally;
            List<CharacterCTRL> Arius = c.GetCharacterWithTraits(Traits.Arius);
            List<CharacterCTRL> SRT = c.GetCharacterWithTraits(Traits.SRT);
            if (Utility.CompareTwoGroups(SRT, Arius))
            {
                character.effectCTRL.ClearEffectWithSource("AriusResistanceBuff");
            }
            else
            {

                CharacterCTRL arius = ResourcePool.Instance.ally.GetCharacterWithTraits(Traits.Arius)[0];
                AriusObserver ariusObserver = arius.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
                int amount = 100 - ariusObserver.GetTraitObserverLevel()[ariusObserver.traitLevel].Data1;
                Effect effect = EffectFactory.UnStatckableStatsEffct(20, "AriusResistanceBuff", amount, StatsType.PercentageResistence, character, true);
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.PercentageResistence, effect.Value, effect.Source),
                    (character) => character.ModifyStats(StatsType.PercentageResistence, -effect.Value, effect.Source)
                );
                character.effectCTRL.AddEffect(effect, character);
            }

        }
    }
}
public class NoneObserver : CharacterObserverBase
{
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        return new Dictionary<int, TraitLevelStats>();
    }
    public NoneObserver(int level, CharacterCTRL character)
    {

    }
    public override bool BeforeDying(CharacterCTRL parent)
    {
        return false;
    }
}
