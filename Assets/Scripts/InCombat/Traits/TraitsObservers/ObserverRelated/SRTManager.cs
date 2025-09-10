using GameEnum;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SRTManager : MonoBehaviour
{
    public static SRTManager instance;
    public StatsContainer SRTPernamentStats = new StatsContainer();
    public StatsContainer SRT_GehennaPernamentStats = new StatsContainer();
    public int SRT_Mill_CostMoney = 0;
    public void Awake()
    {
        instance = this;
    }
    public StatsContainer GetStats()
    {
        if (SelectedAugments.Instance.CheckAugmetExist(107))
        {
            CharacterParent c = ResourcePool.Instance.ally;
            List<CharacterCTRL> Arius = c.GetCharacterWithTraits(Traits.Arius);
            List<CharacterCTRL> SRT = c.GetCharacterWithTraits(Traits.SRT);
            if (!Utility.CompareTwoGroups(SRT,Arius))
            {
                return new StatsContainer();
            }
        }
        StatsContainer stats = SRTPernamentStats.Clone();
        stats.AddFrom(SRT_GehennaPernamentStats);
        if (SelectedAugments.Instance.CheckAugmetExist(124))
        {
            return stats.MultiplyBy(1.4f);
        }
        if (SelectedAugments.Instance.CheckAugmetExist(112))
        {
            float val = PressureManager.Instance.GetPressure(true)*0.02f;
            return stats.MultiplyBy(1+val);
        }
        return stats;
    }
    
    public void Update()
    {
        
    }
    public void AddStat(int index)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(121)) SRT_Mill_CostMoney += 2;
        var observer = Observers.Instance.GetObserverForTrait(Traits.SRT) as SRTObserver;
        ResourcePool.Instance.ally.currTraits.TryGetValue(Traits.SRT, out int count);
        int lvl = count / 2;
        CustomLogger.Log(this, $" observer = {observer} , index = {index},list count = {observer.GetTraitObserverLevel().Count}");
        float i = 0;
        switch (index)
        {
            case 0:
                i = observer.GetTraitObserverLevel()[lvl].Data2;
                SRTPernamentStats.AddValue(StatsType.Attack, i);
                break;
            case 1:
                i = observer.GetTraitObserverLevel()[lvl].Data3;
                SRTPernamentStats.AddValue(StatsType.Resistence, i);
                break;
            case 2:
                i = observer.GetTraitObserverLevel()[lvl].Data4;
                SRTPernamentStats.AddValue(StatsType.Health, i);
                break;
            case 3:
                i = observer.GetTraitObserverLevel()[lvl].Data5;
                SRTPernamentStats.AddValue(StatsType.AttackSpeed, i);
                break;
            default:
                break;
        }
        ResourcePool.Instance.ally.FroceRefrshStats();
    }
    public void AddSRT_GehennaStat(int index)
    {
        var observer = Observers.Instance.GetObserverForTrait(Traits.SRT) as SRTObserver;
        ResourcePool.Instance.ally.currTraits.TryGetValue(Traits.SRT, out int count);
        int lvl = count / 2;
        CustomLogger.Log(this, $" observer = {observer} , index = {index},list count = {observer.GetTraitObserverLevel().Count}");
        float i = 0;
        switch (index)
        {
            case 0:
                i = observer.GetTraitObserverLevel()[lvl].Data2;
                SRT_GehennaPernamentStats.AddValue(StatsType.Attack, i);
                break;
            case 1:
                i = observer.GetTraitObserverLevel()[lvl].Data3;
                SRT_GehennaPernamentStats.AddValue(StatsType.Resistence, i);
                break;
            case 2:
                i = observer.GetTraitObserverLevel()[lvl].Data4;
                SRT_GehennaPernamentStats.AddValue(StatsType.Health, i);
                break;
            case 3:
                i = observer.GetTraitObserverLevel()[lvl].Data5;
                SRT_GehennaPernamentStats.AddValue(StatsType.AttackSpeed, i);
                break;
            default:
                break;
        }
        ResourcePool.Instance.ally.FroceRefrshStats();
    }
}
