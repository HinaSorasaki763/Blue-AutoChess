using GameEnum;
using System;
using System.Text;
using UnityEngine;

public class SRTManager : MonoBehaviour
{
    public static SRTManager instance;
    public StatsContainer SRTPernamentStats = new StatsContainer();
    public void Awake()
    {
        instance = this;
    }
    public StatsContainer GetStats()
    {
        if (SelectedAugments.Instance.CheckAugmetExist(124))
        {
            return SRTPernamentStats.MultiplyBy(1.4f);
        }
        else
        {
            return SRTPernamentStats;
        }
    }
    public void Update()
    {
        
    }
    public void AddStat(int index)
    {
        var observer = Observers.Instance.GetObserverForTrait(GameEnum.Traits.SRT) as SRTObserver;
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
}
