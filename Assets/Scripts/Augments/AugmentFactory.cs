using UnityEngine;

public static class AugmentFactory
{
    public static Augment CreateAugment(AugmentConfig config)
    {
        int index = config.augmentIndex;
        if (index < 100)
            return new CharacterEnhanceAugment(config);
        else if (index < 200)
            return new AcademyAugment(config);
        else
            return new GeneralAugment(config);
    }

}
