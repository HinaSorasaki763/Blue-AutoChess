// DataLayerManager.cs
using GameEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataStackManager : MonoBehaviour
{
    public static DataStackManager Instance { get; private set; }
    public int CurrentDataStack { get; private set; }
    public GameObject MillienumIndicator;
    public TraitsText TraitText;
    public RewardPopup rewardPopup;
    public Dictionary<int, Action> floorRewardMapping;
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
                if (SelectedAugments.Instance.CheckAugmetExist(121, true))
                {
                    int amount = SRTManager.instance.SRT_Mill_CostMoney / 10 * (floorKey / 100);
                    for (int i = 0; i < amount; i++)
                    {
                        ResourcePool.Instance.GetGoldPrefab(new Vector3(0, 0, 0));
                        SRTManager.instance.SRT_Mill_CostMoney--;
                    }

                }
                claimedFloors.Add(floorKey);
            }
            else if (CurrentDataStack < floorKey)
            {
                break;
            }
        }
    }
    private void InitializeFloorRewardMapping()
    {
        floorRewardMapping = new Dictionary<int, Action>
    {
        {
            100, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            200, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            300, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            400, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 2, 2, 16);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            500, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 16);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            600, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 16);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            700, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4,2,2,16);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            800, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0,0,0,40);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            900, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(5, 3, 0, 40);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            1000, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(1, 0, 2, 10);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            1100, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 0, 1, 20);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            1200, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0, 0, 3, 100);
                rewardPopup.AddRewards(context, 3);
            }
        },
        {
            1300, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(1, 0, 2, 10);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            1400, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 0, 1, 20);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            1500, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0, 0, 3, 100);
                rewardPopup.AddRewards(context, 3);
            }
        },
        };
    }

    public void AddDataStack(int amount)
    {
        if (SelectedAugments.Instance.CheckAugmetExist(111, true))
        {
            if (SelectedAugments.Instance.CheckIfConditionMatch(111, true))
            {
                PressureManager.Instance.AddPressure(amount, true);
                DataStackManager.Instance.AddDataStack(amount);
                return;
            }
        }
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
        if (data >= 2)
        {
            MillienumIndicator.SetActive(true);
            TraitText.gameObject.SetActive(true);
            TraitText.TextMesh.text = GetData().ToString();
        }
        else
        {
            TraitText.TextMesh.text = string.Empty;
            MillienumIndicator.SetActive(false);
        }
    }
}
