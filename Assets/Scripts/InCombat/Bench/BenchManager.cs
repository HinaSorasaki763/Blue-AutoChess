using System.Collections.Generic;
using UnityEngine;

public class BenchManager : MonoBehaviour
{
    public List<GameObject> benchSlots = new();
    public CharacterParent characterParent;

    public bool IsBenchFull()
    {
        foreach (var slot in benchSlots)
        {
            if (slot.GetComponent<HexNode>().OccupyingCharacter == null)
            {
                return false;
            }
        }
        return true;
    }

    public bool AddToBench(GameObject characterPrefab)
    {
        foreach (var slot in benchSlots)
        {
            HexNode gridSlot = slot.GetComponent<HexNode>();

            if (gridSlot == null || gridSlot.OccupyingCharacter != null)
            {
                continue;
            }

            Vector3 position = gridSlot.transform.position;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position,gridSlot, characterParent, isAlly: true);
            BugReportLogger.Instance.GetCharacter(characterPrefab.name);
            Debug.Log($"spawned at {position},{gridSlot.name}");
            return true;
        }

        return false;
    }
}
