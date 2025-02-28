using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Balancing_SO", menuName = "Balancing_SO")]

public class Balancing_SO : ScriptableObject
{
    [TextArea(3,10)]
    public string Note;
}
