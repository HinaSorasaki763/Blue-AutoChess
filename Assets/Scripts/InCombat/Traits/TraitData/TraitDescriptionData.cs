using GameEnum;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTraitDescription", menuName = "Traits/TraitDescriptionData")]
public class TraitDescriptionData : ScriptableObject
{
    public Traits trait; // ��������
    [TextArea(3, 10)]
    public string description; // ����²��
    [TextArea(3, 10)]
    public string[] descrtipns;
}
