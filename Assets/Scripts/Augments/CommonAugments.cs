using GameEnum;
using UnityEngine;
public class CommonAugment : Augment
{
    public CommonAugment(AugmentConfig config) : base(config)
    {
    }

    public override void Apply()
    {
        switch (config.augmentIndex)
        {
            case 1000: new Augment1000(config).Apply(); break;
            case 1001: new Augment1001(config).Apply(); break;
            case 1002: new Augment1002(config).Apply(); break;
            case 1003: new Augment1003(config).Apply(); break;
            case 1004: new Augment1004(config).Apply(); break;
            case 1005: new Augment1005(config).Apply(); break;
            case 1006: new Augment1006(config).Apply(); break;
            case 1007: new Augment1007(config).Apply(); break;
            case 1008: new Augment1008(config).Apply(); break;
            case 1009: new Augment1009(config).Apply(); break;
            case 1010: new Augment1010(config).Apply(); break;
            case 1011: new Augment1011(config).Apply(); break;
            case 1012: new Augment1012(config).Apply(); break;
            case 1013: new Augment1013(config).Apply(); break;
            case 1014: new Augment1014(config).Apply(); break;
            case 1015: new Augment1015(config).Apply(); break;
            case 1016: new Augment1016(config).Apply(); break;
            case 1017: new Augment1017(config).Apply(); break;
            case 1018: new Augment1018(config).Apply(); break;
            case 1019: new Augment1019(config).Apply(); break;
            case 1020: new Augment1020(config).Apply(); break;
            case 1021: new Augment1021(config).Apply(); break;
            case 1022: new Augment1022(config).Apply(); break;
            case 1023: new Augment1023(config).Apply(); break;
            case 1024: new Augment1024(config).Apply(); break;
            case 1025: new Augment1025(config).Apply(); break;
            case 1026: new Augment1026(config).Apply(); break;
            case 1027: new Augment1027(config).Apply(); break;
            case 1028: new Augment1028(config).Apply(); break;
            case 1029: new Augment1029(config).Apply(); break;
            case 1030: new Augment1030(config).Apply(); break;
            case 1031: new Augment1031(config).Apply(); break;
            case 1032: new Augment1032(config).Apply(); break;
            case 1033: new Augment1033(config).Apply(); break;
            case 1034: new Augment1034(config).Apply(); break;
            case 1035: new Augment1035(config).Apply(); break;
            case 1036: new Augment1036(config).Apply(); break;
            case 1037: new Augment1037(config).Apply(); break;
            case 1038: new Augment1038(config).Apply(); break;
            case 1039: new Augment1039(config).Apply(); break;
            case 1040: new Augment1040(config).Apply(); break;
            case 1041: new Augment1041(config).Apply(); break;
            case 1042: new Augment1042(config).Apply(); break;
            case 1043: new Augment1043(config).Apply(); break;
            case 1044: new Augment1044(config).Apply(); break;
            case 1045: new Augment1045(config).Apply(); break;
            case 1046: new Augment1046(config).Apply(); break;
            case 1047: new Augment1047(config).Apply(); break;
            case 1048: new Augment1048(config).Apply(); break;
            case 1049: new Augment1049(config).Apply(); break;
            case 1050: new Augment1050(config).Apply(); break;
        }
    }
    public override void Trigger(bool isally)
    {
        if (!isally) CustomLogger.LogError(this, $"is not ally,cant trigger");
        switch (config.augmentIndex)
        {
            case 1035: new Augment1035(config).Trigger(isally); break;
            default:
                CustomLogger.LogError(this, $"Augment {config.augmentIndex} is triggered but not having an actual method");
                break;
        }
    }
    public override bool OnConditionMatch()
    {
        switch (config.augmentIndex)
        {
            default:
                CustomLogger.LogError(this, $"augment {config.augmentIndex} not added yet");
                return false;
            case 1035: return new Augment1035(config).OnConditionMatch();


        }
    }
}

public class Augment1000 : CommonAugment//give 11 gold
{
    public Augment1000(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        GameController.Instance.AddGold(11);
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}
public class Augment1001 : CommonAugment// give 3 1cost 2 2cost 1 3cost
{
    public Augment1001(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 3; i++)
        {
            var list = ResourcePool.Instance.GetCharactersByLevel(1);
            if (list != null && list.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                ResourcePool.Instance.GetCharacterPrefab(list[index].CharacterId);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            var list = ResourcePool.Instance.GetCharactersByLevel(2);
            if (list != null && list.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                ResourcePool.Instance.GetCharacterPrefab(list[index].CharacterId);
            }
        }

        {
            var list = ResourcePool.Instance.GetCharactersByLevel(3);
            if (list != null && list.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                ResourcePool.Instance.GetCharacterPrefab(list[index].CharacterId);
            }
        }

        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }

}

public class Augment1002 : CommonAugment//9 blue 1 gold
{
    public Augment1002(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1003 : CommonAugment//on kill gain rewards 1
{
    public Augment1003(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1004 : CommonAugment//隨機紋章&重鑄器*1
{
    public Augment1004(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
        Utility.GetRandCertificate();
        IEquipment equipment = Utility.GetSpecificEquipment(31);
        EquipmentManager.Instance.AddEquipmentItem(equipment);
    }
}

public class Augment1005 : CommonAugment//小型英雄複製器&每升級學生的星級就獲得1金錢
{
    public Augment1005(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        IEquipment equipment = Utility.GetSpecificEquipment(32);
        EquipmentManager.Instance.AddEquipmentItem(equipment);
    }
}

public class Augment1006 : CommonAugment//獲得一個地磚選擇器，對著我方棋格使用時，可以將其轉換為強化棋格，使得在上面開始戰鬥的單位獲得10%傷害減免、10%傷害增幅以及10%攻擊速度
{
    public Augment1006(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        IEquipment equipment = Utility.GetSpecificEquipment(33);
        EquipmentManager.Instance.AddEquipmentItem(equipment);
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1007 : CommonAugment//開始戰鬥時，如果周圍沒有友軍，則獲得相當於8%的最大生命護盾
{
    public Augment1007(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1008 : CommonAugment//單位額外造成1%真實傷害，並且使得敵方"重創"，獲得治癒量減少，且無法疊加。
{
    public Augment1008(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1009 : CommonAugment//獲得8%全能吸血。並且在下一次造成傷害時，造成超過的治癒量10%的真實傷害
{
    public Augment1009(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1010 : CommonAugment//星級1或是價格1、2的單位獲得15%攻擊速度
{
    public Augment1010(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1011 : CommonAugment//戰鬥開始時，若鄰格恰好只有一個友軍，自己獲得10%攻擊速度、10防禦
{
    public Augment1011(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1012 : CommonAugment//戰鬥開始10秒後，所有單位治癒40%已損失生命
{
    public Augment1012(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1013 : CommonAugment//戰鬥開始15秒後，所有單位獲得30%傷害增幅
{
    public Augment1013(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1014 : CommonAugment
{
    public Augment1014(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1015 : CommonAugment
{
    public Augment1015(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1016 : CommonAugment
{
    public Augment1016(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1017 : CommonAugment//give 21 gold
{
    public Augment1017(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        GameController.Instance.AddGold(21);
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1018 : CommonAugment//擊倒敵方單位時，35%機率獲得隨階段提升價值的戰利品
{
    public Augment1018(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1019 : CommonAugment//獲得兩個文章、以及兩個重鑄器
{
    public Augment1019(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
        for (int i = 0; i < 2; i++)
        {
            Utility.GetRandCertificate();
            IEquipment equipment = Utility.GetSpecificEquipment(31);
            EquipmentManager.Instance.AddEquipmentItem(equipment);
        }
    }
}

public class Augment1020 : CommonAugment//戰鬥開始時，受到12%最大生命值的傷害，並且獲得12%傷害增幅。勝利時兩項數值各+1，反之-1
{
    public Augment1020(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1021 : CommonAugment//獲得一個小型英雄複製器，並且每將一個英雄升至三星，就再獲得一個小型英雄複製器
{
    public Augment1021(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
        IEquipment equipment = Utility.GetSpecificEquipment(32);
        EquipmentManager.Instance.AddEquipmentItem(equipment);
    }
    public override void Trigger(bool isally)
    {

    }
}

public class Augment1022 : CommonAugment//獲勝時，根據場上剩餘的己方單位獲得戰利品。
{
    public Augment1022(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1023 : CommonAugment//當即將受到的傷害會使得生命值低於40%時，獲得一個最大生命值30%的護盾。每單位每回合一次。
{
    public Augment1023(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1024 : CommonAugment//角色每在場上放置一回合，就會獲得1%傷害增幅、1%攻擊力、1%生命值、1%傷害減免(升級時取最高者)
{
    public Augment1024(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1025 : CommonAugment//角色每裝備一件裝備(成裝或是組件皆可)，獲得5%攻擊速度、5%攻擊力，以及5%機率在直接擊殺時掉落金錢(隨階段提升)
{//TODO:暫時沒有實裝"隨階段提升"
    public Augment1025(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1026 : CommonAugment//匹配到的對手勝場數將會比己方多1，現在獲勝時，獲得一個隨機組件。失敗時，獲得1招募點數。
{//TODO:匹配條件尚未完成
    public Augment1026(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1027 : CommonAugment//開始戰鬥時，如果周圍沒有友軍，則獲得相當於12%的最大生命護盾，以及5%傷害增幅
{
    public Augment1027(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1028 : CommonAugment//獲得15%全能吸血。並且在下一次造成傷害時，造成超過的治癒量25%的真實傷害
{
    public Augment1028(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1029 : CommonAugment//單位對於四格外的敵軍造成額外15%傷害，且如果受到傷害來自於自身攻擊範圍內，則受到傷害上升15%，並且攻擊距離4以上的單位獲得15%傷害增幅。
{
    public Augment1029(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1030 : CommonAugment//上一場戰鬥造成傷害最高的單位將會永久額外造成2%傷害以及2%攻擊力，承受最多傷害的單位將會額外獲得2%生命以及2%傷害減免
{
    public Augment1030(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1031 : CommonAugment//若裝備一件或以上的裝備，獲得300生命值。若裝備三件裝備，改為獲得450生命值。
{
    public Augment1031(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1032 : CommonAugment//我方單位獲得10%生命值、以及2%生命值的攻擊力
{
    public Augment1032(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1033 : CommonAugment//戰鬥開始15秒後，所有友軍單位獲得60%傷害增幅
{
    public Augment1033(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1034 : CommonAugment
{
    public Augment1034(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        GameController.Instance.AddGold(35);
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1035 : CommonAugment//當觸發三個學院時，給予金錢、對應的五費學生，以及總計兩件合適的裝備
{
    public Augment1035(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
    public override void Trigger(bool isally)
    {
        CustomLogger.Log(this, $"Trigger {config.augmentIndex},name {config.name}");
    }
    public override bool OnConditionMatch()
    {
        bool count = ResourcePool.Instance.ally.GetActiveAcademyTraits().Count >= 3;
        if (count)
        {
            SelectedAugments.Instance.TriggerAugment(1035, true);
            return true;
        }
        return false;
    }
}

public class Augment1036 : CommonAugment//隊伍每攻擊五次時，全體獲得1%傷害增幅，上限為20。抵達上限時，全隊造成10%額外真實傷害。
{
    public Augment1036(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 3; i++)
        {
            Utility.GetRandCertificate();
            IEquipment equipment = Utility.GetSpecificEquipment(31);
            EquipmentManager.Instance.AddEquipmentItem(equipment);
        }
        ResourcePool.Instance.GetRandRewardPrefab(Vector3.zero);
    }
}

public class Augment1037 : CommonAugment//隊伍內每裝備一件裝備，我方隊伍就獲得15生命以及2%攻速，獲得三個隨機組件
{
    public Augment1037(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 3; i++)
        {
            ResourcePool.Instance.GetRandRewardPrefab(Vector3.zero);
        }
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1038 : CommonAugment//隊伍可部署的人數上限+1，獲得一個組件
{
    public Augment1038(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        ResourcePool.Instance.GetRandRewardPrefab(Vector3.zero);
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1039 : CommonAugment//每一次刷新商店時，50%機率獲得一個免費刷新。
{
    public Augment1039(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1040 : CommonAugment//開始戰鬥時，如果周圍沒有友軍，則獲得相當於15%的最大生命護盾，以及8%傷害增幅，以及8%傷害減免
{
    public Augment1040(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1041 : CommonAugment//獲得25%全能吸血，並且在下一次造成傷害時，造成超過的治癒量30%的真實傷害
{
    public Augment1041(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1042 : CommonAugment//若裝備一件或以上的裝備，獲得500生命值。若裝備三件裝備，改為獲得750生命值。
{
    public Augment1042(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1043 : CommonAugment//獲得15%傷害增幅，戰鬥開始15秒後，再額外獲得60%傷害增幅
{
    public Augment1043(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1044 : CommonAugment
{
    public Augment1044(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1045 : CommonAugment
{
    public Augment1045(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1046 : CommonAugment
{
    public Augment1046(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1047 : CommonAugment
{
    public Augment1047(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1048 : CommonAugment
{
    public Augment1048(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1049 : CommonAugment
{
    public Augment1049(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1050 : CommonAugment
{
    public Augment1050(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}
