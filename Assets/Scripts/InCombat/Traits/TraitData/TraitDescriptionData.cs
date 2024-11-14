using GameEnum;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTraitDescription", menuName = "Traits/TraitDescriptionData")]
public class TraitDescriptionData : ScriptableObject
{
    public bool IsAcademy;
    public Traits trait; // Åù²ÌÃş«¬
    public Sprite sprite;
    [TextArea(3, 10)]
    public string description; // Åù²ÌÂ²¤¶
    [TextArea(3, 10)]
    public string[] descrtipns;

}
