using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlingProperties : MonoBehaviour
{
    public static BattlingProperties Instance;
    public StatsContainer Ally_SRTPernamentStats = new StatsContainer();
    public StatsContainer Ally_SRT_GehennaPernamentStats = new StatsContainer();
    public StatsContainer Enemy_SRTPernamentStats = new StatsContainer();
    public StatsContainer Enemy_SRT_GehennaPernamentStats = new StatsContainer();


    void Start()
    {
        Instance = this;
    }
    public StatsContainer GetSRTStats(bool isAlly)
    {
        StatsContainer statsContainer = isAlly ? Ally_SRTPernamentStats : Enemy_SRTPernamentStats;
        statsContainer.AddFrom(GetSRT_GehennaStats(isAlly));
        return statsContainer;
    }
    public StatsContainer GetSRT_GehennaStats(bool isAlly)
    {
        return isAlly ? Ally_SRT_GehennaPernamentStats : Enemy_SRT_GehennaPernamentStats;
    }
    public void AddSRTStats(StatsType statsType,float amount,bool isAlly)
    {
        StatsContainer statsContainer = isAlly ? Ally_SRTPernamentStats : Enemy_SRTPernamentStats;
        statsContainer.AddValue(statsType, amount);
    }
    public void AddSRT_GehennaStats(StatsType statsType, float amount, bool isAlly)
    {
        StatsContainer statsContainer = isAlly ? Ally_SRT_GehennaPernamentStats : Enemy_SRT_GehennaPernamentStats;
        statsContainer.AddValue(statsType, amount);
    }
    public void SetSRTStats(StatsContainer statsContainer,bool isAlly)
    {
        StatsContainer s = isAlly ? Ally_SRTPernamentStats : Enemy_SRTPernamentStats;
        StatsContainer newStats = new StatsContainer();
        newStats.AddFrom(statsContainer);
        s = newStats;
    }
    public void SetSRT_GehennaStats(StatsContainer statsContainer,bool isAlly)
    {
        StatsContainer s = isAlly ? Ally_SRT_GehennaPernamentStats : Enemy_SRT_GehennaPernamentStats;
        StatsContainer newStats = new StatsContainer();
        newStats.AddFrom(statsContainer);
        s = newStats;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
