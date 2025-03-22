using UnityEngine;

[CreateAssetMenu(menuName = "Augments/AugmentConfig")]
public class AugmentConfig : ScriptableObject
{
    public string augmentName;  // 強化名稱
    public Sprite augmentIcon;  // 圖片
    [TextArea(3,10)]
    public string description;  // 描述
    public int augmentIndex;    // 與代碼層面類別聯繫的索引
    public int CharacterSkillEnhanceIndex = -1;  // 與技能強化類別聯繫的索引
}
