using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "ScriptableObjects/EnemyWave", order = 1)]
public class EnemyWave : ScriptableObject
{
    [Serializable]
    public class GridSlotData
    {
        public int GridIndex;
        public int CharacterID; 
    }
    public string EnemyName;
    public List<GridSlotData> gridSlots = new List<GridSlotData>();
    public GridSlotData logisticSlot1 = new GridSlotData();
    public GridSlotData logisticSlot2 = new GridSlotData();

}
