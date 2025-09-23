using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class SRTManager : MonoBehaviour
{
    public static SRTManager instance;
    public int SRT_Mill_CostMoney = 0;
    public void Awake()
    {
        instance = this;
    }
    public StatsContainer GetStats(bool isAlly)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(107, isAlly))
        {
            CharacterParent c = ResourcePool.Instance.ally;
            List<CharacterCTRL> Arius = c.GetCharacterWithTraits(Traits.Arius);
            List<CharacterCTRL> SRT = c.GetCharacterWithTraits(Traits.SRT);
            if (!Utility.CompareTwoGroups(SRT, Arius))
            {
                return new StatsContainer();
            }
        }
        StatsContainer stats = BattlingProperties.Instance.GetSRTStats(isAlly).Clone();
        stats.AddFrom(BattlingProperties.Instance.GetSRT_GehennaStats(isAlly).Clone());
        if (SelectedAugments.Instance.CheckAugmetExist(124, isAlly))
        {
            return stats.MultiplyBy(1.4f);
        }
        if (SelectedAugments.Instance.CheckAugmetExist(112, isAlly))
        {
            float val = PressureManager.Instance.GetPressure(true) * 0.02f;
            return stats.MultiplyBy(1 + val);
        }
        return stats;
    }

    public void Update()
    {

    }
    public void AddStat(int index, bool isAlly)
    {
        if (isAlly && SelectedAugments.Instance.CheckAugmetExist(121, isAlly)) SRT_Mill_CostMoney += 2;
        var observer = Observers.Instance.GetObserverForTrait(Traits.SRT) as SRTObserver;
        ResourcePool.Instance.ally.currTraits.TryGetValue(Traits.SRT, out int count);
        int lvl = count / 2;
        CustomLogger.Log(this, $" observer = {observer} , index = {index},list count = {observer.GetTraitObserverLevel().Count}");
        float i = 0;
        switch (index)
        {
            case 0:
                i = observer.GetTraitObserverLevel()[lvl].Data2;
                BattlingProperties.Instance.AddSRTStats(StatsType.Attack, i,isAlly);
                break;
            case 1:
                i = observer.GetTraitObserverLevel()[lvl].Data3;
                BattlingProperties.Instance.AddSRTStats(StatsType.Resistence, i, isAlly);
                break;
            case 2:
                i = observer.GetTraitObserverLevel()[lvl].Data4;
                BattlingProperties.Instance.AddSRTStats(StatsType.Health, i, isAlly);
                break;
            case 3:
                i = observer.GetTraitObserverLevel()[lvl].Data5;
                BattlingProperties.Instance.AddSRTStats(StatsType.AttackSpeed, i, isAlly);
                break;
            default:
                break;
        }
        ResourcePool.Instance.ally.FroceRefrshStats();
    }
    public void AddSRT_GehennaStat(int index,bool isAlly)
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
                BattlingProperties.Instance.AddSRT_GehennaStats(StatsType.Attack, i, isAlly);
                break;
            case 1:
                i = observer.GetTraitObserverLevel()[lvl].Data3;
                BattlingProperties.Instance.AddSRT_GehennaStats(StatsType.Resistence, i, isAlly);
                break;
            case 2:
                i = observer.GetTraitObserverLevel()[lvl].Data4;
                BattlingProperties.Instance.AddSRT_GehennaStats(StatsType.Health, i, isAlly);
                break;
            case 3:
                i = observer.GetTraitObserverLevel()[lvl].Data5;
                BattlingProperties.Instance.AddSRT_GehennaStats(StatsType.AttackSpeed, i, isAlly);
                break;
            default:
                break;
        }
        ResourcePool.Instance.ally.FroceRefrshStats();
    }
}
