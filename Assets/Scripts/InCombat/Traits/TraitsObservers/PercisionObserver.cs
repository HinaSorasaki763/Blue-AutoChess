using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0)},
            {1, new TraitLevelStats(30)},
            {2, new TraitLevelStats(60)},
            {3, new TraitLevelStats(115)}
        };
        return statsByStarLevel;
    }
    public override int DamageModifier(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        int rawdmg = damage;
        if (source.Target == null)
        {
            return damage;
        }
        if (target.CurrentHex.Index == source.Target.GetComponent<CharacterCTRL>().CurrentHex.Index)
        {
            damage = (int)(damage * (1 + GetTraitObserverLevel()[traitLevel].Data1 * 0.01f));
        }
        CustomLogger.Log(this,$"rawdmg from {rawdmg} modifed to {damage}");
        return damage;
    }
    public PrecisionObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.character = character;
    }
}
