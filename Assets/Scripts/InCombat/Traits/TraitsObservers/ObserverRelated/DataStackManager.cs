// DataLayerManager.cs
using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStackManager : MonoBehaviour
{
    public static DataStackManager Instance { get; private set; }
    public int CurrentDataStack { get; private set; }
    public GameObject MillienumIndicator;
    public TraitsText TraitText;
    public RewardPopup rewardPopup;
    private Dictionary<int, Action> floorRewardMapping;
    private HashSet<int> claimedFloors = new HashSet<int>();

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            CurrentDataStack = 0;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeFloorRewardMapping();
        StartCoroutine(Temp());
    }

    public void ResetData()
    {
        CurrentDataStack = 0;
        claimedFloors.Clear();
    }
    private void Start()
    {
        UpdateIndicator();
    }
    public void CheckDataStackRewards()
    {
        var sortedKeys = new List<int>(floorRewardMapping.Keys);
        sortedKeys.Sort();
        foreach (int floorKey in sortedKeys)
        {
            if (CurrentDataStack >= floorKey && !claimedFloors.Contains(floorKey))
            {
                floorRewardMapping[floorKey]?.Invoke();
                claimedFloors.Add(floorKey);
            }
            else if (CurrentDataStack < floorKey)
            {
                break;
            }
        }
    }

    public IEnumerator Temp()
    {
        yield return new WaitForSeconds(3);
    }
    private void InitializeFloorRewardMapping()
    {
        floorRewardMapping = new Dictionary<int, Action>
    {
        {
            100, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.ShowRewards(context, 1);
            }
        },
        {
            200, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.ShowRewards(context, 1);
            }
        },
        {
            300, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            400, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 2, 2, 16);
                rewardPopup.ShowRewards(context, 1);
            }
        },
        {
            500, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 16);
                rewardPopup.ShowRewards(context, 1);
            }
        },
        {
            600, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 16);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            700, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4,2,2,16);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            800, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0,0,0,40);
                rewardPopup.ShowRewards(context, 1);
            }
        },
        {
            900, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(5, 3, 0, 40);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            1000, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(1, 0, 2, 10);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            1100, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 0, 1, 20);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            1200, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0, 0, 3, 100);
                rewardPopup.ShowRewards(context, 3);
            }
        },
        {
            1300, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(1, 0, 2, 10);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            1400, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 0, 1, 20);
                rewardPopup.ShowRewards(context, 2);
            }
        },
        {
            1500, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0, 0, 3, 100);
                rewardPopup.ShowRewards(context, 3);
            }
        },
    };
    }

    public void IncreaseDataStack(int amount)
    {
        CurrentDataStack += amount;
        UpdateIndicator();
    }
    public int GetData()
    {
        return CurrentDataStack;
    }
    public void ResetDataLayer()
    {
        CurrentDataStack = 0;
    }
    public void UpdateIndicator()
    {
        int data = Utility.GetInBattleCharactersWithTrait(Traits.Millennium, true).Count;
        if (data > 0)
        {
            MillienumIndicator.SetActive(true);
            TraitText.gameObject.SetActive(true);
            TraitText.TextMesh.text = $"Data : {GetData()}";
        }
        else
        {
            TraitText.TextMesh.text = string.Empty;
            MillienumIndicator.SetActive(false);
        }
    }
}
