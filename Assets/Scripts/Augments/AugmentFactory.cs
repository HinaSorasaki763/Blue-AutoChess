using UnityEngine;

public static class AugmentFactory
{
    public static Augment CreateAugment(AugmentConfig config)
    {
        switch (config.augmentIndex)
        {
            case 0:
                return new SpeedBoostAugment(config);
            case 1:
                return new HealthBoostAugment(config);
            case 2:
                return new Tempaugment1(config);
            case 3:
                return new Tempaugment2(config);
            case 4:
                return new Tempaugment3(config);
            default:
                return null;
        }
    }
}
