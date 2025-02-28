using GameEnum;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTraitDescription", menuName = "Traits/TraitDescriptionData")]
public class TraitDescriptionData : ScriptableObject
{
    public bool IsAcademy;
    public Traits trait;
    public Sprite sprite;
    [TextArea(3, 10)]
    public string description;
    [TextArea(3, 10)]
    public string[] descrtipns;
    public List<TraitThreshold> thresholds;
}
[System.Serializable]
public class TraitThreshold
{
    public int level;
    public int requiredCount;
    public float effectValue;
    [TextArea(3, 10)]
    public string effectDescription;
}