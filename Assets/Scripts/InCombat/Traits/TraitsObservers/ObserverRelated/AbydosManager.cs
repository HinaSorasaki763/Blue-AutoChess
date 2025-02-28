using System.Collections.Generic;
using UnityEngine;

public class AbydosManager : MonoBehaviour
{
    public static AbydosManager Instance { get; private set; }
    public int level;
    public Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {
            {0, new TraitLevelStats(0,0,0)},
            {1, new TraitLevelStats(6,30,2)},
            {2, new TraitLevelStats(8,45,3)},
            {3, new TraitLevelStats(10,70,5)},
            {4, new TraitLevelStats(12,70,10)}
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
}
