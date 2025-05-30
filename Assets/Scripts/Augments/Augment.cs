using GameEnum;
using System;
using UnityEngine;

public abstract class Augment
{
    public AugmentConfig config;  // �t�m�ƾ�

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
        ResourcePool.Instance.GetRandCharacterPrefab(new Vector3(0, 0, 0), characterIndex);
        CustomLogger.Log(this, $"CharacterEnhanceAugment applying character {characterIndex}");
        ResourcePool.Instance.ally.AddEnhancedSkill(characterIndex);
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