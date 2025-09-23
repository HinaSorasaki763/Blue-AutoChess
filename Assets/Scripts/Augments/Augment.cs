using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Augment
{
    public AugmentConfig config;  // 配置數據

    public Augment(AugmentConfig config)
    {
        this.config = config;
    }

    public string Name => config.augmentName;
    public Sprite Icon => config.augmentIcon;
    public string Description => config.description;
    public string DescriptionEnglish => config.descriptionEnglish;
    public virtual void Apply()
    {

    }
    public virtual void Trigger(bool isally)
    {

    }
    public virtual bool OnConditionMatch()
    {
        return false;
    }

}
public class CharacterEnhanceAugment : Augment
{
    public CharacterEnhanceAugment(AugmentConfig config) : base(config)
    {

    }
    public override void Apply()
    {
        int characterIndex = config.CharacterSkillEnhanceIndex;
        if (config.CharacterSkillEnhanceIndex == 4)
        {
            characterIndex += 500;
        }
        if (characterIndex == 31)
        {
            characterIndex = 22;
        }
        ResourcePool.Instance.GetRandCharacterPrefab(characterIndex);
        CustomLogger.Log(this, $"CharacterEnhanceAugment applying character {characterIndex}");
        ResourcePool.Instance.ally.AddEnhancedSkill(characterIndex);
    }
}
public class AcademyAugment : Augment
{
    public AcademyAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        AcademyAugment academyAugment = null;
        CustomLogger.Log(this, $"{config.augmentName} applying");
        switch (config.augmentIndex)
        {
            case 100:
                Abydos_AriusAugment abydos_arius = new Abydos_AriusAugment(config);
                abydos_arius.Apply();
                academyAugment = abydos_arius;
                break;
            case 101:
                Abydos_GehennaAugment abydos_gehenna = new Abydos_GehennaAugment(config);
                abydos_gehenna.Apply();
                academyAugment = abydos_gehenna;
                break;
            case 102:
                Abydos_HyakkiyakoAugment abydos_hyakkiyako = new Abydos_HyakkiyakoAugment(config);
                abydos_hyakkiyako.Apply();
                academyAugment = abydos_hyakkiyako;
                break;
            case 103:
                Abydos_MillenniumAugment abydos_millennium = new Abydos_MillenniumAugment(config);
                abydos_millennium.Apply();
                academyAugment = abydos_millennium;
                break;
            case 104:
                Abydos_SRTAugment abydos_srt = new Abydos_SRTAugment(config);
                abydos_srt.Apply();
                academyAugment = abydos_srt;
                break;
            case 105:
                Abydos_TrinityAugment abydos_trinity = new Abydos_TrinityAugment(config);
                abydos_trinity.Apply();
                academyAugment = abydos_trinity;
                break;
            case 106:
                AbydosAugment abydos = new AbydosAugment(config);
                abydos.Apply();
                academyAugment = abydos;
                break;
            case 107:
                Arius_SRTAugment arius_srt = new Arius_SRTAugment(config);
                arius_srt.Apply();
                academyAugment = arius_srt;
                break;
            case 108:
                AriusAugment arius = new AriusAugment(config);
                arius.Apply();
                academyAugment = arius;
                break;
            case 109:
                Gehenna_AriusAugment gehenna_arius = new Gehenna_AriusAugment(config);
                gehenna_arius.Apply();
                academyAugment = gehenna_arius;
                break;
            case 110:
                Gehenna_HyakkiyakoAugment gehenna_hyakkiyako = new Gehenna_HyakkiyakoAugment(config);
                gehenna_hyakkiyako.Apply();
                academyAugment = gehenna_hyakkiyako;
                break;
            case 111:
                Gehenna_MillenniumAugment gehenna_millennium = new Gehenna_MillenniumAugment(config);
                gehenna_millennium.Apply();
                academyAugment = gehenna_millennium;
                break;
            case 112:
                Gehenna_SRTAugment gehenna_srt = new Gehenna_SRTAugment(config);
                gehenna_srt.Apply();
                academyAugment = gehenna_srt;
                break;
            case 113:
                Gehenna_TrinityAugment gehenna_trinity = new Gehenna_TrinityAugment(config);
                gehenna_trinity.Apply();
                academyAugment = gehenna_trinity;
                break;
            case 114:
                GehennaAugment gehenna = new GehennaAugment(config);
                gehenna.Apply();
                academyAugment = gehenna;
                break;
            case 115:
                Hyakkiyako_AriusAugment hyakkiyako_arius = new Hyakkiyako_AriusAugment(config);
                hyakkiyako_arius.Apply();
                academyAugment = hyakkiyako_arius;
                break;
            case 116:
                Hyakkiyako_MillenniumAugment hyakkiyako_millennium = new Hyakkiyako_MillenniumAugment(config);
                hyakkiyako_millennium.Apply();
                academyAugment = hyakkiyako_millennium;
                break;
            case 117:
                Hyakkiyako_SRTAugment hyakkiyako_srt = new Hyakkiyako_SRTAugment(config);
                hyakkiyako_srt.Apply();
                academyAugment = hyakkiyako_srt;
                break;
            case 118:
                Hyakkiyako_TrinityAugment hyakkiyako_trinity = new Hyakkiyako_TrinityAugment(config);
                hyakkiyako_trinity.Apply();
                academyAugment = hyakkiyako_trinity;
                break;
            case 119:
                HyakkiyakoAugment hyakkiyako = new HyakkiyakoAugment(config);
                hyakkiyako.Apply();
                academyAugment = hyakkiyako;
                break;
            case 120:
                Millennium_AriusAugment millennium_arius = new Millennium_AriusAugment(config);
                millennium_arius.Apply();
                academyAugment = millennium_arius;
                break;
            case 121:
                SRT_MillenniumAugment srt_millennium = new SRT_MillenniumAugment(config);
                srt_millennium.Apply();
                academyAugment = srt_millennium;
                break;
            case 122:
                Millennium_TrinityAugment millennium_trinity = new Millennium_TrinityAugment(config);
                millennium_trinity.Apply();
                academyAugment = millennium_trinity;
                break;
            case 123:
                MillenniumAugment millennium = new MillenniumAugment(config);
                millennium.Apply();
                academyAugment = millennium;
                break;
            case 124:
                SRTAugment srt = new SRTAugment(config);
                srt.Apply();
                academyAugment = srt;
                break;
            case 125:
                Trinity_AriusAugment trinity_arius = new Trinity_AriusAugment(config);
                trinity_arius.Apply();
                academyAugment = trinity_arius;
                break;
            case 126:
                Trinity_SRTAugment trinity_srt = new Trinity_SRTAugment(config);
                trinity_srt.Apply();
                academyAugment = trinity_srt;
                break;
            case 127:
                TrinityAugment trinity = new TrinityAugment(config);
                trinity.Apply();
                academyAugment = trinity;
                break;
            default:
                CustomLogger.LogWarning(this, $"Unknown augment index: {config.augmentIndex}");
                break;
        }

        CustomLogger.Log(this, $"{academyAugment.Name} special effect from AcademyAugment ,{academyAugment.Name}");
    }
    public override void Trigger(bool isally)
    {
        if (!isally)
        {
            CustomLogger.LogError(this, $"is not ally,cant trigger");
        }
        switch (config.augmentIndex)
        {
            case 109:
                Gehenna_AriusAugment gehenna_ = new Gehenna_AriusAugment(config);
                gehenna_.Trigger(isally);
                break;
            default:
                CustomLogger.LogError(this, $"Augment {config.augmentIndex} is triggered but not having an actual method");
                break;
            case 124:
                SRTAugment SRTAugment = new SRTAugment(config);
                SRTAugment.Trigger(isally);
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
            case 101:
                Abydos_GehennaAugment abydos_Gehenna = new Abydos_GehennaAugment(config);
                return abydos_Gehenna.OnConditionMatch();
            case 107:
                Arius_SRTAugment SRTAugment = new Arius_SRTAugment(config);
                return SRTAugment.OnConditionMatch();
            case 111:
                Gehenna_MillenniumAugment gehenna_Millennium = new Gehenna_MillenniumAugment(config);
                return gehenna_Millennium.OnConditionMatch();

        }
    }
}
public class Gehenna_AriusAugment : AcademyAugment
{
    public Gehenna_AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 3; i++)
        {
            BenchManager.Instance.AddToBench(Utility.GetSpecificCharacterToSpawn(25));
        }

    }
    public override void Trigger(bool isally)
    {
        PressureManager.Instance.AddPressure(200, true);
    }
}
public class Trinity_GehennaAugment : AcademyAugment
{
    public Trinity_GehennaAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        IEquipment equipment = Utility.GetExchangeCirtificate(Traits.Gehenna, Traits.Trinity);
        EquipmentManager.Instance.AddEquipmentItem(equipment);

    }
}
public class Abydos_SRTAugment : AcademyAugment
{
    public Abydos_SRTAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {

    }
}
public class Abydos_GehennaAugment : AcademyAugment
{
    public Abydos_GehennaAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {

    }
    public override void Trigger(bool isally)
    {
        ResourcePool.Instance.GetRandCharacterPrefab(25);
        ResourcePool.Instance.GetRandCharacterPrefab(26);
    }
    public override bool OnConditionMatch()
    {
        int stars = ResourcePool.Instance.ally
            .GetNoneRepeatCharacterOnField()
            .Where(c =>
            {
                var academy = c.traitController.GetAcademy();
                return academy == Traits.Abydos || academy == Traits.Gehenna;
            })
            .Sum(c => c.star);
        return stars >= 15;
    }

}
public class Arius_SRTAugment : AbydosAugment
{
    public Arius_SRTAugment(AugmentConfig config) : base(config) { }
    public override bool OnConditionMatch()
    {
        int Arius_stars = 0, Arius_Attack = 0, Arius_Health = 0;
        int SRT_stars = 0, SRT_Attack = 0, SRT_Health = 0;
        var AriusStudents = ResourcePool.Instance.ally.GetCharacterWithTraits(Traits.Arius);
        var SRTStudents = ResourcePool.Instance.ally.GetCharacterWithTraits(Traits.SRT);
        foreach (var c in AriusStudents)
        {
            Arius_stars += c.star;
            Arius_Attack += (int)c.GetStat(StatsType.Attack, false);
            Arius_Health += (int)c.GetStat(StatsType.Health, false);
        }
        foreach (var c in SRTStudents)
        {
            SRT_stars += c.star;
            SRT_Attack += (int)c.GetStat(StatsType.Attack, false);
            SRT_Health += (int)c.GetStat(StatsType.Health, false);
        }
        if (Arius_stars != SRT_stars)
            return Arius_stars > SRT_stars;
        if (Arius_Attack != SRT_Attack)
            return Arius_Attack > SRT_Attack;
        if (Arius_Health != SRT_Health)
            return Arius_Health > SRT_Health;

        return false;
    }
}
public class AriusAugment : AcademyAugment
{
    public AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {

    }
}
public class AbydosAugment : AcademyAugment
{
    public AbydosAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from AbydosAugment");
        IEquipment equipment = Utility.GetSpecificEquipment(101);
        if (equipment is SpecialEquipment special)
        {
            special.Traits.Add(Traits.Abydos);
            special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
            special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
            EquipmentManager.Instance.AddEquipmentItem(special);
        }
    }
}
public class MillenniumAugment : AcademyAugment
{
    public MillenniumAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from MillenniumAugment");
        HexNode hexNode = SpawnGrid.Instance.GetEmptyHex();
        Vector3 position = hexNode.Position;
        Character characterData = ResourcePool.Instance.GetCharacterByID(40);
        GameObject characterPrefab = characterData.Model;
        GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
            characterPrefab,
            position,
            hexNode,
            ResourcePool.Instance.ally,
            true,
            1
        );

    }
}
public class SRT_MillenniumAugment : AcademyAugment
{
    public SRT_MillenniumAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from {config.augmentName}");
    }
    public override void Trigger(bool isally)
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.SRT);
                special.Traits.Add(Traits.Millennium);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}
public class SRTAugment : AcademyAugment
{
    public SRTAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from {config.augmentName}");
    }
    public override void Trigger(bool isally)
    {
        CustomLogger.Log(this, $"Trigger {config.augmentName} special effect from {config.augmentName}");
        int rand = UnityEngine.Random.Range(25, 31);
        BenchManager.Instance.AddToBench(Utility.GetSpecificCharacterToSpawn(rand));
    }
}
public class TrinityAugment : AcademyAugment
{
    public TrinityAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from TrinityAugment");
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }

    }
}
public class Abydos_AriusAugment : AcademyAugment
{
    public Abydos_AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        ResourcePool.Instance.GetRandCharacterPrefab(26);
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Abydos);
                special.Traits.Add(Traits.Arius);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}
public class Abydos_HyakkiyakoAugment : AcademyAugment
{
    public Abydos_HyakkiyakoAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Abydos);
                special.Traits.Add(Traits.Hyakkiyako);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Abydos_MillenniumAugment : AcademyAugment
{
    public Abydos_MillenniumAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        IEquipment equipment = Utility.GetSpecificEquipment(29);
        EquipmentManager.Instance.AddEquipmentItem(equipment);
        for (int i = 0; i < 2; i++)
        {
            IEquipment eq = Utility.GetSpecificEquipment(101);
            if (eq is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Abydos);
                special.Traits.Add(Traits.Millennium);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }

    }
}
public class Abydos_TrinityAugment : AcademyAugment
{
    public Abydos_TrinityAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.Abydos);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Gehenna_HyakkiyakoAugment : AcademyAugment
{
    public Gehenna_HyakkiyakoAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Hyakkiyako);
                special.Traits.Add(Traits.Gehenna);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Gehenna_MillenniumAugment : AcademyAugment
{
    public Gehenna_MillenniumAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Gehenna);
                special.Traits.Add(Traits.Millennium);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
    /// <summary>
    /// true為格黑娜，false為千年，比較人數/星級/攻擊/生命，都相同時默認回傳true
    /// </summary>
    /// <returns></returns>
    public override bool OnConditionMatch()
    {
        var ally = ResourcePool.Instance.ally;
        List<CharacterCTRL> gehenna = ally.GetCharacterWithTraits(Traits.Gehenna);
        List<CharacterCTRL> millennium = ally.GetCharacterWithTraits(Traits.Millennium);
        int gCount = gehenna.Count, mCount = millennium.Count;
        if (gCount != mCount) return gCount > mCount;
        int gStars = 0, mStars = 0, gAtk = 0, mAtk = 0, gHp = 0, mHp = 0;
        for (int i = 0; i < gCount; i++)
        {
            var c = gehenna[i];
            gStars += c.star;
            gAtk += (int)c.GetStat(StatsType.Attack);
            gHp += (int)c.GetStat(StatsType.Health);
        }
        for (int i = 0; i < mCount; i++)
        {
            var c = millennium[i];
            mStars += c.star;
            mAtk += (int)c.GetStat(StatsType.Attack);
            mHp += (int)c.GetStat(StatsType.Health);
        }
        if (gStars != mStars) return gStars > mStars;
        if (gAtk != mAtk) return gAtk > mAtk;
        if (gHp != mHp) return gHp > mHp;
        return true;
    }

}

public class Gehenna_SRTAugment : AcademyAugment
{
    public Gehenna_SRTAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Gehenna);
                special.Traits.Add(Traits.SRT);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Gehenna_TrinityAugment : AcademyAugment
{
    public Gehenna_TrinityAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.Gehenna);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class GehennaAugment : AcademyAugment
{
    public GehennaAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        BenchManager.Instance.AddToBench(Utility.GetSpecificCharacterToSpawn(41));
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Gehenna);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Hyakkiyako_AriusAugment : AcademyAugment
{
    public Hyakkiyako_AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Hyakkiyako);
                special.Traits.Add(Traits.Arius);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Hyakkiyako_MillenniumAugment : AcademyAugment
{
    public Hyakkiyako_MillenniumAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Hyakkiyako);
                special.Traits.Add(Traits.Millennium);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Hyakkiyako_SRTAugment : AcademyAugment
{
    public Hyakkiyako_SRTAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Hyakkiyako);
                special.Traits.Add(Traits.SRT);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Hyakkiyako_TrinityAugment : AcademyAugment
{
    public Hyakkiyako_TrinityAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.Hyakkiyako);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class HyakkiyakoAugment : AcademyAugment
{
    public HyakkiyakoAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Hyakkiyako);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Millennium_AriusAugment : AcademyAugment
{
    public Millennium_AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Millennium);
                special.Traits.Add(Traits.Arius);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Millennium_TrinityAugment : AcademyAugment
{
    public Millennium_TrinityAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.Millennium);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Trinity_AriusAugment : AcademyAugment
{
    public Trinity_AriusAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        AriusManager.Instance.RemoveSelector();
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.Arius);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}

public class Trinity_SRTAugment : AcademyAugment
{
    public Trinity_SRTAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        for (int i = 0; i < 2; i++)
        {
            IEquipment equipment = Utility.GetSpecificEquipment(101);
            if (equipment is SpecialEquipment special)
            {
                special.Traits.Add(Traits.Trinity);
                special.Traits.Add(Traits.SRT);
                special.equipmentDetail = $"{special.Traits[0]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} exchange certificate";
                EquipmentManager.Instance.AddEquipmentItem(special);
            }
        }
    }
}
public class GeneralAugment : Augment
{
    public GeneralAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} applying");
    }
}
public class SpeedBoostAugment : Augment
{
    public SpeedBoostAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {

    }
}

public class HealthBoostAugment : Augment
{
    public HealthBoostAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {

    }
}
public class Tempaugment1 : Augment
{
    public Tempaugment1(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        AugmentEventHandler.Instance.AddObserver(new AddManaObserver(2));
        CustomLogger.Log(this, "Tempaugment1 applying");
    }
}
public class AddManaObserver : CharacterObserverBase
{
    public int amount;
    public AddManaObserver(int Amount)
    {
        amount = Amount;
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        character.Addmana(amount);
        base.OnAttacking(character);
    }
}
public class Tempaugment2 : Augment
{
    public Tempaugment2(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, "Tempaugment2 applying");
    }
}
public class Tempaugment3 : Augment
{
    public Tempaugment3(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, "Tempaugment3 applying");
    }
}