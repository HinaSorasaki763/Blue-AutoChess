using UnityEngine;

public static class AugmentFactory
{
    public static Augment CreateAugment(AugmentConfig config)
    {
        switch (config.augmentIndex)
        {
            default:
            return new CharacterEnhanceAugment(config);
        }
    }
}
