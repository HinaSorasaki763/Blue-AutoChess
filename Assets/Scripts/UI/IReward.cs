using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameEnum;

public interface IReward
{
    // �o����y���欰
    void Award();
    // �^�Ǽ��y�y�z��r�A�� UI ���
    string GetRewardDescription();
    string GetRewardName();
}

// �Ҧp�G�˳Ƽ��y
public class EquipmentReward : IReward
{
    public IEquipment Equipment { get; private set; }

    public EquipmentReward(IEquipment equipment)
    {
        Equipment = equipment;
    }

    public void Award()
    {
        CustomLogger.Log(this, $"�o��˳Ƽ��y: {Equipment.EquipmentName}");
        // ���B���]���@�Ӥ�k�Ω�o��˳ơA�p SpawnEquipment(Equipment)
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

// �Ҧp�G�������y
public class CurrencyReward : IReward
{
    public int Amount { get; private set; }

    public CurrencyReward(int amount)
    {
        Amount = amount;
    }

    public void Award()
    {
        CustomLogger.Log(this, $"�o��������y: {Amount}");
        GameController.Instance.AddGold(Amount);
    }

    public string GetRewardDescription()
    {
        return $"���� x {Amount}";
    }
    public string GetRewardName()
    {
        return $"���� x {Amount}";
    }
}

// �ƦX���y�G��@�ﶵ���i��]�t�h�Ӽ��y
public class CompositeReward : IReward
{
    public List<IReward> Rewards { get; private set; }

    public CompositeReward(List<IReward> rewards)
    {
        Rewards = rewards;
    }

    public void Award()
    {
        CustomLogger.Log(this, "�}�l�o��ƦX���y");
        foreach (var reward in Rewards)
        {
            reward.Award();
        }
    }

    public string GetRewardDescription()
    {
        // �Ҧp�H�u+�v�s���U���y�y�z
        return string.Join(" + ", Rewards.Select(r => r.GetRewardDescription()).ToArray());
    }
    public string GetRewardName()
    {
        return string.Join(" + ", Rewards.Select(r => r.GetRewardName()).ToArray());
    }

}
// �@�Ӽ��y���ءA�i�]�t�@�өΦh�Ӽ��y
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

    // �o�񦹱��ؤ��Ҧ����y
    public void AwardAll()
    {
        CustomLogger.Log(this, "AwardAll RewardEntry");
        foreach (var reward in Rewards)
        {
            reward.Award();
        }
    }

    // ���o����y�z�G�Y���ۭq�y�z�h���[���y�y�z
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

// ���y context�A�]�˩Ҧ� RewardEntry
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
        CustomLogger.Log(this, $"�s�W RewardEntry: {entry.GetName()}");
    }

    // �p�G�ݭn�@���o��Ҧ����y
    public void AwardAllRewards()
    {
        CustomLogger.Log(this, "�}�l�o��Ҧ����y");
        foreach (var entry in RewardEntries)
        {
            entry.AwardAll();
        }
    }
}
