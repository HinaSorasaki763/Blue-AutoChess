using GameEnum;
using System;
using UnityEngine;

public abstract class Augment
{
    public AugmentConfig config;  // °t¸m¼Æ¾Ú

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
    public virtual void Trigger()
    {
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
        ResourcePool.Instance.GetRandCharacterPrefab(new Vector3(0, 0, 0), characterIndex);
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
            case 106:
                AbydosAugment abydosAugment = new AbydosAugment(config);
                abydosAugment.Apply();
                academyAugment = abydosAugment;
                break;
            case 113:
                Trinity_GehennaAugment trinity_GehennaAugment = new Trinity_GehennaAugment(config);
                trinity_GehennaAugment.Apply();
                academyAugment = trinity_GehennaAugment;
                break;
            case 123:
                MillenniumAugment millenniumAugment = new MillenniumAugment(config);
                millenniumAugment.Apply();
                academyAugment = millenniumAugment;
                break;
            case 124:
                SRTAugment SRTAugment = new SRTAugment(config);
                SRTAugment.Apply();
                academyAugment = SRTAugment;
                break;
            case 127:
                TrinityAugment trinityAugment = new TrinityAugment(config);
                trinityAugment.Apply();
                academyAugment = trinityAugment;
                break;
            default:
                break;
        }
        CustomLogger.Log(this, $"{academyAugment.Name} special effect from AcademyAugment ,{academyAugment.Name}");

    }
    public override void Trigger()
    {
        switch (config.augmentIndex)
        {
            default:
            case 124:
                SRTAugment SRTAugment = new SRTAugment(config);
                SRTAugment.Trigger();
                break;
        }
    }
}
public class Trinity_GehennaAugment : AcademyAugment
{
    public Trinity_GehennaAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from Trinity_GehennaAugment");
        IEquipment equipment = Utility.GetSpecificEquipment(101);
        if (equipment is SpecialEquipment special)
        {
            special.Traits.Add(Traits.Gehenna);
            special.Traits.Add(Traits.Trinity);
            special.equipmentDetail = $"{special.Traits[0]} and {special.Traits[1]} exchange certificate";
            special.equipmentDescriptionEnglish = $"{special.Traits[0]} and {special.Traits[1]} exchange certificate";
            EquipmentManager.Instance.AddEquipmentItem(special);
        }

    }
}
public class AbydosAugment : AcademyAugment
{
    public AbydosAugment(AugmentConfig config) : base(config) { }
    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from AbydosAugment");
        IEquipment equipment = Utility.GetSpecificEquipment(102);
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
public class SRTAugment : AcademyAugment
{
    public SRTAugment(AugmentConfig config) : base(config) { }

    public override void Apply()
    {
        CustomLogger.Log(this, $"{config.augmentName} special effect from {config.augmentName}");
    }
    public override void Trigger()
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
        character.Addmana( amount);
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