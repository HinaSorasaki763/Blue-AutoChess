using UnityEngine;

[CreateAssetMenu(menuName = "Augments/AugmentConfig")]
public class AugmentConfig : ScriptableObject
{
    public string augmentName;  // 強化名稱
    public Sprite augmentIcon;  // 圖片
    public string description;  // 描述
    public int augmentIndex;    // 與代碼層面類別聯繫的索引
}
