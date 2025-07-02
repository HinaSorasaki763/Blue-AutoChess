using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BenchManager : MonoBehaviour
{
    public List<GameObject> benchSlots = new();
    public CharacterParent characterParent;
    public static BenchManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
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

            if (characterPrefab.GetComponent<CharacterCTRL>().characterStats.CharacterId == 999)
            {

                HexNode h = SpawnGrid.Instance.GetEmptyHex();
                GameObject obj = Instantiate(ResourcePool.Instance.LogisticDummy, h.Position + new Vector3(0, 0.14f, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
                obj.transform.SetParent(ResourcePool.Instance.ally.transform);
                CharacterBars bar = ResourcePool.Instance.GetBar(h.Position).GetComponent<CharacterBars>();
                ResourcePool.Instance.ally.childCharacters.Add(obj);
                CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
                StaticObject staticObj = obj.GetComponent<StaticObject>();
                ctrl.SetBarChild(bar);
                ctrl.characterBars = bar;
                bar.SetBarsParent(obj.transform);
                staticObj.GetComponent<StaticObject>().InitNoParentDummy(20000, 35, false);
                ctrl.CurrentHex = h;
                h.OccupyingCharacter = ctrl;
                ctrl.IsAlly = true;
                obj.layer = 8;
                h.Reserve(ctrl);
                return true;
            }
            if (gridSlot == null || gridSlot.OccupyingCharacter != null)
            {
                continue;
            }

            Vector3 position = gridSlot.transform.position;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position,gridSlot, characterParent, isAlly: true,1);

            BugReportLogger.Instance.GetCharacter(characterPrefab.name);
            Debug.Log($"spawned at {position},{gridSlot.name}");
            return true;
        }

        return false;
    }
}
