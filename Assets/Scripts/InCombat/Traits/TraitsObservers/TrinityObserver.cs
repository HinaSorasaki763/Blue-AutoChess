using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinityObserver : CharacterObserverBase
{
    public int traitLevel {  get; private set; }
    public CharacterCTRL parent;
    
    public TrinityObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        traitLevel = level;
        parent = character;
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            //基礎傷害,觸發次數,傷害倍率,點心加成
            {0, new TraitLevelStats(300,0,30,0)},
            {1, new TraitLevelStats(300,5,30,50)},
            {2, new TraitLevelStats(500,10,40,80)},
            {3, new TraitLevelStats(1000,15,50,200)}
        };
        return statsByStarLevel;
    }
    public int GetCuurDmg()
    {
        return GetTraitObserverLevel()[traitLevel].Data1;
    }
    public int GetData4()
    {
        return GetTraitObserverLevel()[traitLevel].Data4;
    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        if (!activated) return;
        base.OnDamageDealt(source, target, damage, detailedSource, iscrit);
        CustomLogger.Log(this, $"character {source} dealt {damage} to {target} at {Time.time}");
        TrinityManager.Instance.AddStack(target.transform.position,detailedSource,target.CurrentHex,source);
    }
}
