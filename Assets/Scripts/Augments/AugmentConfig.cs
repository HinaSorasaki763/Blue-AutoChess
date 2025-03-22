using UnityEngine;

[CreateAssetMenu(menuName = "Augments/AugmentConfig")]
public class AugmentConfig : ScriptableObject
{
    public string augmentName;  // �j�ƦW��
    public Sprite augmentIcon;  // �Ϥ�
    [TextArea(3,10)]
    public string description;  // �y�z
    public int augmentIndex;    // �P�N�X�h�����O�pô������
    public int CharacterSkillEnhanceIndex = -1;  // �P�ޯ�j�����O�pô������
}
