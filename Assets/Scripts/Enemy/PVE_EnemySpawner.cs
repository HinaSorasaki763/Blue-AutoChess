using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PVE_EnemySpawner : MonoBehaviour
{
    public static PVE_EnemySpawner Instance;
    public List<EnemyWave> enemyWaves;
    public CharacterParent enemyParent;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        LoadEnemyWaves();
    }
    // Step 1: Load all enemy waves from Resources
    private void LoadEnemyWaves()
    {
        enemyWaves = Resources.LoadAll<EnemyWave>("PVE_Resources").OrderBy(wave =>
        {
            string[] parts = wave.name.Split(' ');
            int roundNumber = int.Parse(parts[1]);
            return roundNumber;
        }).ToList();
    }
    public void SpawnEnemiesNextStage()
    {
        CustomLogger.Log(this, "PVE_EnemySpawner SpawnEnemiesNextStage");
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }
        List<string> characterNames = new List<string>();
        List<string> hexnodexNames = new List<string>();
        foreach (var slot in enemyWaves[GameStageManager.Instance.currentRound].gridSlots)
        {
            if (slot.CharacterID != -1)
            {
                if (SpawnGrid.Instance.indexToCubeKey.TryGetValue(slot.GridIndex, out string cubeKey))
                {
                    if (SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode hexNode))
                    {
                        int lvl = slot.Level == 0 ? 1 : slot.Level;
                        Vector3 position = hexNode.Position;
                        Character characterData = ResourcePool.Instance.GetCharacterByID(slot.CharacterID);
                        GameObject characterPrefab = characterData.Model;
                        GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
                            characterPrefab, position, hexNode, enemyParent, false, lvl
                        );
                        CharacterCTRL characterCtrl = go.GetComponent<CharacterCTRL>();
                        characterNames.Add(characterCtrl.name);
                        hexnodexNames.Add(hexNode.name);
                        characterCtrl.ResetStats();
                        if (characterCtrl != null)
                        {
                            for (int i = 0; i < slot.EquipmentIDs.Length; i++)
                            {
                                int equipmentID = slot.EquipmentIDs[i];
                                if (equipmentID != -1)
                                {
                                    IEquipment template = EquipmentManager.Instance.GetEquipmentByID(equipmentID);
                                    if (template != null)
                                    {
                                        IEquipment newEquipment = template.Clone();
                                        CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(newEquipment.Id);
                                        if (c != null) newEquipment.Observer = c;
                                        bool equipped = characterCtrl.EquipItem(newEquipment);
                                        CustomLogger.Log(this, $"Character {characterData.name} equipped with {newEquipment.EquipmentName}: {equipped}");
                                    }
                                    else
                                    {
                                        CustomLogger.LogError(this, $"No Equipment found for ID {equipmentID}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            CustomLogger.LogError(this, $"Character {characterData.name} has no CharacterCTRL component.");
                        }
                        CustomLogger.Log(this, $"Character {characterData.name} spawned at {position}");
                    }
                    else
                    {
                        CustomLogger.LogError(this, $"No HexNode found for cube key {cubeKey}");
                    }
                }
                else
                {
                    CustomLogger.LogError(this, $"No cube key found for GridIndex {slot.GridIndex}");
                }
            }
        }
        (bool b, string s) = SpawnLogisticCharacter(enemyWaves[GameStageManager.Instance.currentRound].logisticSlot1, ResourcePool.Instance.EnemylogisticSlotNode1);
        if (b)
        {
            characterNames.Add(s);
            hexnodexNames.Add(ResourcePool.Instance.EnemylogisticSlotNode1.name);
        }
        (bool B, string S) = SpawnLogisticCharacter(enemyWaves[GameStageManager.Instance.currentRound].logisticSlot2, ResourcePool.Instance.EnemylogisticSlotNode2);
        if (B)
        {
            characterNames.Add(S);
            hexnodexNames.Add(ResourcePool.Instance.EnemylogisticSlotNode2.name);
        }
        BugReportLogger.Instance.ChooseEnemyInRound(GameStageManager.Instance.currentRound, 0, characterNames, hexnodexNames);
        ResourcePool.Instance.enemy.UpdateTraitEffects();
    }

    private (bool, string) SpawnLogisticCharacter(EnemyWave.GridSlotData logisticSlot, HexNode logisticSlotNode)
    {
        if (logisticSlot.CharacterID != -1)
        {
            Vector3 position = logisticSlotNode.transform.position;
            Character characterData = ResourcePool.Instance.GetCharacterByID(logisticSlot.CharacterID);
            GameObject characterPrefab = characterData.Model;
            int lvl = logisticSlot.Level == 0 ? 1 : logisticSlot.Level;
            GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
                characterPrefab, position, logisticSlotNode, enemyParent, false, lvl
            );
            go.transform.position = position;
            SpawnGrid.Instance.indexToCubeKey.TryGetValue(logisticSlot.DummyGridIndex, out string cubeKey);
            SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode h);
            GameObject obj = Instantiate(
                ResourcePool.Instance.LogisticDummy,
                h.Position + new Vector3(0, 0.14f, 0),
                Quaternion.Euler(Vector3.zero)
            );
            CustomLogger.Log(this, "spawned dummy");
            obj.transform.SetParent(ResourcePool.Instance.enemy.transform);
            CharacterBars bar = ResourcePool.Instance.GetBar(h.Position).GetComponent<CharacterBars>();
            ResourcePool.Instance.enemy.childCharacters.Add(obj);
            obj.name = $"dummy ({characterData.CharacterName})";
            CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
            StaticObject staticObj = obj.GetComponent<StaticObject>();
            CharacterCTRL c = go.GetComponent<CharacterCTRL>();
            staticObj.parent = c;
            ctrl.SetBarChild(bar);
            ctrl.characterBars = bar;
            CustomLogger.Log(this, $"get bar to {obj.name},bar parent = {ctrl},child = {ctrl.characterBars}");
            bar.SetBarsParent(obj.transform);
            staticObj.RefreshDummy(c);
            ctrl.CurrentHex = h;
            h.OccupyingCharacter = ctrl;
            h.Reserve(ctrl);
            return (true, c.name);
        }
        return (false, "");
    }
}
