using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameEnum;

public interface IReward
{
    // 發放獎勵的行為
    void Award();
    // 回傳獎勵描述文字，供 UI 顯示
    string GetRewardDescription();
    string GetRewardName();
}

// 例如：裝備獎勵
public class EquipmentReward : IReward
{
    public IEquipment Equipment { get; private set; }

    public EquipmentReward(IEquipment equipment)
    {
        Equipment = equipment;
    }

    public void Award()
    {
        CustomLogger.Log(this, $"發放裝備獎勵: {Equipment.EquipmentName}");
        // 此處假設有一個方法用於發放裝備，如 SpawnEquipment(Equipment)
        EquipmentManager.Instance.AddEquipmentItem(Equipment);
    }

    public string GetRewardDescription()
    {
        return Equipment.EquipmentDetail;
    }
    public string GetRewardName()
    {
        return Equipment.EquipmentName;
    }
}

// 例如：金幣獎勵
public class CurrencyReward : IReward
{
    public int Amount { get; private set; }

    public CurrencyReward(int amount)
    {
        Amount = amount;
    }

    public void Award()
    {
        CustomLogger.Log(this, $"發放金幣獎勵: {Amount}");
        GameController.Instance.AddGold(Amount);
    }

    public string GetRewardDescription()
    {
        return $"金幣 x {Amount}";
    }
    public string GetRewardName()
    {
        return $"金幣 x {Amount}";
    }
}

// 複合獎勵：單一選項中可能包含多個獎勵
public class CompositeReward : IReward
{
    public List<IReward> Rewards { get; private set; }

    public CompositeReward(List<IReward> rewards)
    {
        Rewards = rewards;
    }

    public void Award()
    {
        CustomLogger.Log(this, "開始發放複合獎勵");
        foreach (var reward in Rewards)
        {
            reward.Award();
        }
    }

    public string GetRewardDescription()
    {
        // 例如以「+」連接各獎勵描述
        return string.Join(" + ", Rewards.Select(r => r.GetRewardDescription()).ToArray());
    }
    public string GetRewardName()
    {
        return string.Join(" + ", Rewards.Select(r => r.GetRewardName()).ToArray());
    }

}
// 一個獎勵條目，可包含一個或多個獎勵
public class RewardEntry
{
    public List<IReward> Rewards { get; private set; }
    public string Name { get; private set; }
    public string EnglishName { get; private set; }
    public string ChineseDescription { get; private set; }
    public string EnglishDescription { get; set; }
    public Sprite Sprite { get; private set; }
    public int Index { get; private set; }

    public RewardEntry(List<IReward> rewards,Sprite sprite,string chineseName,string englishName, string chineseDescription ,string englishDescription,int index)
    {
        Rewards = rewards;
        Name = chineseName;
        EnglishName = englishName;
        ChineseDescription = chineseDescription;
        EnglishDescription = englishDescription;
        Sprite = sprite;
        Index = index;
    }

    // 發放此條目內所有獎勵
    public void AwardAll()
    {
        CustomLogger.Log(this, "AwardAll RewardEntry");
        foreach (var reward in Rewards)
        {
            reward.Award();
        }
    }

    // 取得完整描述：若有自訂描述則附加獎勵描述
    public string GetName()
    {
        if (PlayerSettings.SelectedDropdownValue == 0)
        {
            return Name;
        }
        else
        {
            return EnglishName;
        }
    }
}

// 獎勵 context，包裝所有 RewardEntry
public class RewardContext
{
    public List<RewardEntry> RewardEntries { get; private set; }

    public RewardContext()
    {
        RewardEntries = new List<RewardEntry>();
    }

    public void AddRewardEntry(RewardEntry entry)
    {
        RewardEntries.Add(entry);
        CustomLogger.Log(this, $"新增 RewardEntry: {entry.GetName()}");
    }

    // 如果需要一次發放所有獎勵
    public void AwardAllRewards()
    {
        CustomLogger.Log(this, "開始發放所有獎勵");
        foreach (var entry in RewardEntries)
        {
            entry.AwardAll();
        }
    }
}
