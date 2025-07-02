using UnityEngine;

[CreateAssetMenu(menuName = "Augments/AugmentConfig")]
public class AugmentConfig : ScriptableObject
{
    public string augmentName;
    public Sprite augmentIcon;
    [TextArea(3,10)]
    public string description;
    [TextArea(3, 10)]
    public string descriptionEnglish;
    public int augmentIndex;
    public int CharacterSkillEnhanceIndex = -1;
}
