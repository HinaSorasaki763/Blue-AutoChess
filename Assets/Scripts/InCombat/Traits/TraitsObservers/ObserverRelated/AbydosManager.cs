using System.Collections.Generic;
using UnityEngine;
using GameEnum;

public class AbydosManager : MonoBehaviour
{
    public static AbydosManager Instance { get; private set; }
    public int level;
    public int SRT_AbydosAugmentAffectedHpCount = 0;
    public int Augment102_LivedCount;
    public int Augment102_EarnedCount;
    private bool Aug102Triggered = false;
    public Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(8,30,2)},
            {2, new TraitLevelStats(10,45,3)},
            {3, new TraitLevelStats(12,70,5)},
            {4, new TraitLevelStats(20,100,10)}
        };
        return statsByStarLevel;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
    public void UpdateDesertifiedTiles()
    {
        SpawnGrid.Instance.UpdateDesertifiedTiles(ResourcePool.Instance.RandomKeyThisGame, AbydosManager.Instance.GetTraitObserverLevel()[level].Data1);
    }
    public void AddSRTCounter(int amount)
    {
        SRT_AbydosAugmentAffectedHpCount += amount;
        if (SRT_AbydosAugmentAffectedHpCount >= 1000)
        {
            SRT_AbydosAugmentAffectedHpCount -= 1000;
            int rand = Random.Range(0, 5);
            SRTManager.instance.AddStat(rand);
        }
    }
    public void Check102Stack(CharacterCTRL c )//TODO: add this
    {
        while (Augment102_LivedCount >= 5 && Augment102_EarnedCount < 5)
        {
            Augment102_LivedCount -= 5;
            Augment102_EarnedCount++;
        }
        if (!Aug102Triggered && Augment102_EarnedCount >=5)
        {
            Aug102Triggered = true;
            //TODO: Add prize
        }

    }

}
