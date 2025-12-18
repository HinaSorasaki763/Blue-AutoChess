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

public class Augment1002 : CommonAugment
{
    public Augment1002(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1003 : CommonAugment
{
    public Augment1003(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1004 : CommonAugment
{
    public Augment1004(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1005 : CommonAugment
{
    public Augment1005(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1006 : CommonAugment
{
    public Augment1006(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1007 : CommonAugment
{
    public Augment1007(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1008 : CommonAugment
{
    public Augment1008(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1009 : CommonAugment
{
    public Augment1009(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1010 : CommonAugment
{
    public Augment1010(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1011 : CommonAugment
{
    public Augment1011(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1012 : CommonAugment
{
    public Augment1012(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1013 : CommonAugment
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

public class Augment1018 : CommonAugment
{
    public Augment1018(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1019 : CommonAugment
{
    public Augment1019(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1020 : CommonAugment
{
    public Augment1020(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1021 : CommonAugment
{
    public Augment1021(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1022 : CommonAugment
{
    public Augment1022(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1023 : CommonAugment
{
    public Augment1023(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1024 : CommonAugment
{
    public Augment1024(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1025 : CommonAugment
{
    public Augment1025(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1026 : CommonAugment
{
    public Augment1026(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1027 : CommonAugment
{
    public Augment1027(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1028 : CommonAugment
{
    public Augment1028(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1029 : CommonAugment
{
    public Augment1029(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1030 : CommonAugment
{
    public Augment1030(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1031 : CommonAugment
{
    public Augment1031(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1032 : CommonAugment
{
    public Augment1032(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1033 : CommonAugment
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

public class Augment1035 : CommonAugment
{
    public Augment1035(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1036 : CommonAugment
{
    public Augment1036(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1037 : CommonAugment
{
    public Augment1037(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1038 : CommonAugment
{
    public Augment1038(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1039 : CommonAugment
{
    public Augment1039(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1040 : CommonAugment
{
    public Augment1040(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1041 : CommonAugment
{
    public Augment1041(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1042 : CommonAugment
{
    public Augment1042(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"Applying {config.augmentIndex},name {config.name}");
    }
}

public class Augment1043 : CommonAugment
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
