using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            // 璝à︹计秖ぃìㄢ钡处臩┮Τà︹
            foreach (var slot in availableSlots)
            {
                // 安砞Τ处臩à︹矪瞶よΑ琌砞﹚à︹陪ボ篈
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

    // Step 4: Player selects a wave, then spawn the enemies
    public void PlayerSelectWave(int waveIndex)
    {
        CustomLogger.Log(this, "PlayerSelectWave");
        if (waveIndex < 0 || waveIndex >= selectedEnemyWaves.Count)
        {
            Debug.LogError("Invalid wave index selected.");
            return;
        }

        chosenWave = selectedEnemyWaves[waveIndex];
        SpawnEnemiesNextStage();
    }

    public void SpawnEnemiesNextStage()
    {
        CustomLogger.Log(this, "SpawnEnemiesNextStage");
        if (chosenWave == null)
        {
            Debug.LogError("No wave chosen. Please select a wave first.");
            return;
        }
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }
        foreach (var slot in chosenWave.gridSlots)
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
                        GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, hexNode, enemyParent, isAlly: false);
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
        SpawnLogisticCharacter(chosenWave.logisticSlot1, ResourcePool.Instance.EnemylogisticSlotNode1);
        SpawnLogisticCharacter(chosenWave.logisticSlot2, ResourcePool.Instance.EnemylogisticSlotNode2);
        ResourcePool.Instance.enemy.UpdateTraitEffects();
    }

    private void SpawnLogisticCharacter(EnemyWave.GridSlotData logisticSlot, HexNode logisticSlotNode)
    {
        if (logisticSlot.CharacterID != -1)
        {
            Vector3 position = logisticSlotNode.Position;
            Character characterData = ResourcePool.Instance.GetCharacterByID(logisticSlot.CharacterID);
            GameObject characterPrefab = characterData.Model;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, logisticSlotNode, enemyParent, isAlly: false);
        }
    }
    public List<EnemyWave.GridSlotData> GetRevealedCharacters(EnemyWave wave)
    {
        List<EnemyWave.GridSlotData> availableSlots = wave.gridSlots.Where(x => x.CharacterID != -1).ToList();
        if (availableSlots.Count <= 2)
        {
            return availableSlots; // 璝à︹计秖ぃìㄢ┮Τà︹
        }
        return availableSlots.OrderBy(x => Random.value).Take(2).ToList(); // 繦诀处臩ㄢà︹
    }

}
