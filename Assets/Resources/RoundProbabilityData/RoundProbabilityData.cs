using UnityEngine;
using System.Collections.Generic;
using GameEnum;

[CreateAssetMenu(fileName = "RoundProbabilityData", menuName = "RoundProbabilityData", order = 1)]
public class RoundProbabilityData : ScriptableObject
{
    public List<RoundProbability> roundProbabilities;
}
