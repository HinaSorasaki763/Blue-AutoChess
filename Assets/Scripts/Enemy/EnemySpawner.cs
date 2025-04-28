using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameEnum;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    public List<EnemyWave> enemyWaves;
    public CharacterParent enemyParent;
    public List<EnemyWave> selectedEnemyWaves = new List<EnemyWave>();
    private EnemyWave chosenWave;
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
        enemyWaves = Resources.LoadAll<EnemyWave>("EnemyWaves").ToList();
        if (enemyWaves.Count < 3)
        {
            Debug.LogError("Not enough enemy waves available.");
            return;
        }
    }

    // Step 2: Randomly select three enemy waves
    public void SelectRandomEnemyWaves()
    {
        selectedEnemyWaves = enemyWaves.OrderBy(x => Random.value).Take(3).ToList();
        foreach (var wave in selectedEnemyWaves)
        {
            RevealTwoRandomCharacters(wave);
        }
    }

    // Step 3: Randomly reveal two characters in an enemy wave
    private void RevealTwoRandomCharacters(EnemyWave wave)
    {
        List<EnemyWave.GridSlotData> availableSlots = wave.gridSlots.Where(x => x.CharacterID != -1).ToList();
        if (availableSlots.Count <= 2)
        {
            Debug.LogWarning($"Wave {wave.EnemyName} has less than or equal to two characters, revealing all.");
            // �Y����ƶq������ӡA�������S�Ҧ�����
            foreach (var slot in availableSlots)
            {
                // ���]���@�Ӵ��S���⪺�B�z�覡�A�i�H�O�]�w���⬰��ܪ��A
                Debug.Log($"Revealed character: {slot.CharacterID}");
            }
            return;
        }
        var revealedCharacters = availableSlots.OrderBy(x => Random.value).Take(2).ToList();
        foreach (var slot in revealedCharacters)
        {
            
            Debug.Log($"Revealed character: {slot.CharacterID}");
        }
    }

    public void PlayerSelectWave(int waveIndex)
    {
        CustomLogger.Log(this, "PlayerSelectWave");
        if (waveIndex < 0 || waveIndex >= selectedEnemyWaves.Count)
        {
            Debug.LogError("Invalid wave index selected.");
            return;
        }

        chosenWave = selectedEnemyWaves[waveIndex];
        SpawnEnemiesNextStage(chosenWave);
    }

    public void SpawnEnemiesNextStage(EnemyWave enemyWave)
    {
        if (enemyWave == null)
        {
            Debug.LogError("No wave chosen. Please select a wave first.");
            return;
        }

        // �M�z�W�@�i�ĤH
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }

        // �ͦ��s�@�i�ĤH
        foreach (var slot in enemyWave.gridSlots)
        {
            if (slot.CharacterID != -1)
            {
                if (SpawnGrid.Instance.indexToCubeKey.TryGetValue(slot.GridIndex, out string cubeKey))
                {
                    if (SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode hexNode))
                    {
                        Vector3 position = hexNode.Position;
                        Character characterData = ResourcePool.Instance.GetCharacterByID(slot.CharacterID);
                        GameObject characterPrefab = characterData.Model;
                        GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
                            characterPrefab,
                            position,
                            hexNode,
                            enemyParent,
                            isAlly: false,
                            slot.Level
                        );

                        CharacterCTRL characterCtrl = go.GetComponent<CharacterCTRL>();
                        if (slot.CharacterID == 999)
                        {
                            StaticObject staticObject = go.GetComponent<StaticObject>();
                            staticObject.InitNoParentDummy(10000,35,false);
                        }
                        if (characterCtrl != null)
                        {
                            // ���Ө���˳Ƹ˳�
                            for (int i = 0; i < slot.EquipmentIDs.Length; i++)
                            {
                                int equipmentID = slot.EquipmentIDs[i];
                                if (equipmentID != -1)
                                {
                                    IEquipment template = EquipmentManager.Instance.GetEquipmentByID(equipmentID);
                                    if (template != null)
                                    {
                                        // �� Clone �@���s�˳ơA�A������˳�
                                        IEquipment newEquipment = template.Clone();
                                        CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(newEquipment.Id);
                                        if (c != null)
                                        {
                                            newEquipment.Observer = c;
                                        }
                                        bool equipped = characterCtrl.EquipItem(newEquipment);

                                        CustomLogger.Log(
                                            this,
                                            $"Character {characterData.name} equipped with {newEquipment.EquipmentName}: {equipped}"
                                        );
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
                    Debug.LogError($"No cube key found for GridIndex {slot.GridIndex}");
                }
            }
        }

        // �� Logistic ����t�m�˳� (�p�G�ݭn)
        SpawnLogisticCharacter(enemyWave.logisticSlot1, ResourcePool.Instance.EnemylogisticSlotNode1);
        SpawnLogisticCharacter(enemyWave.logisticSlot2, ResourcePool.Instance.EnemylogisticSlotNode2);

        ResourcePool.Instance.enemy.UpdateTraitEffects();
    }

    private void SpawnLogisticCharacter(EnemyWave.GridSlotData logisticSlot, HexNode logisticSlotNode)
    {
        if (logisticSlot.CharacterID != -1)
        {
            Vector3 position = logisticSlotNode.Position;
            Character characterData = ResourcePool.Instance.GetCharacterByID(logisticSlot.CharacterID);
            GameObject characterPrefab = characterData.Model;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, logisticSlotNode, enemyParent, isAlly: false,logisticSlot.Level);
        }
    }
    public List<EnemyWave.GridSlotData> GetRevealedCharacters(EnemyWave wave)
    {
        List<EnemyWave.GridSlotData> availableSlots = wave.gridSlots.Where(x => x.CharacterID != -1).ToList();
        if (availableSlots.Count <= 2)
        {
            return availableSlots; // �Y����ƶq������ӡA��^�Ҧ�����
        }
        return availableSlots.OrderBy(x => Random.value).Take(2).ToList(); // �H�����S��Ө���
    }

}
