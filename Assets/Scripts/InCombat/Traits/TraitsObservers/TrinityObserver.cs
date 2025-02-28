using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinityObserver : CharacterObserverBase
{
    private int traitLevel;
    public CharacterCTRL parent;
    
    public TrinityObserver(int level, CharacterCTRL character)
    {
        traitLevel = level;
        parent = character;
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(300,5,30)},
            {2, new TraitLevelStats(500,10,40)},
            {3, new TraitLevelStats(1000,15,50)}
        };
        return statsByStarLevel;
    }
    public int GetCuurDmg()
    {
        return GetTraitObserverLevel()[traitLevel].Data1;
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (!activated) return;
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
        CustomLogger.Log(this, $"character {source} dealt {damage} to {target} at {Time.time}");
        TrinityManager.Instance.AddStack(target.transform.position,detailedSource,target.CurrentHex,source);
    }
}
