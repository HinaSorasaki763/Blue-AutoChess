using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using TMPro;
using GameEnum;

public class EnemyWaveRuntimeEditor : MonoBehaviour
{
    public static EnemyWaveRuntimeEditor Instance { get; private set; }
    // 這些都從 Inspector 綁定
    [Header("UI")]
    public TMP_InputField fileNameInputField;    // 讓使用者輸入檔案名稱
    public Toggle overrideToggle;            // true = 直接覆蓋，false = 自動產生新檔名
    private string snapshotPath ;
    private const string DefaultName = "DefaultSnapshot"; // 當使用者沒輸入名稱的預設檔名
    public List<Editor_HexTile> editor_HexTiles = new List<Editor_HexTile>();
    // 目前選取的「空白 HexTile」
    private Editor_HexTile selectedTile = null;
    public Dictionary<string, HexNode> hexNodes = new Dictionary<string, HexNode>();
    public Dictionary<int, string> indexToCubeKey = new Dictionary<int, string>();
    public GameObject characterButtonPrefab;
    public Transform characterContentParent;
    public List<Character> OneCostCharacter, TwoCostCharacter, ThreeCostCharacter, FourCostCharacter, FiveCostCharacter, SpecialCharacter, TestBuildCharacter;
    public List<List<Character>> Lists = new();
    public Sprite HexagonImage;
    public List<IEquipment> availableEquipments = new List<IEquipment>();
    public Color Color;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        snapshotPath = Application.persistentDataPath;
        CustomLogger.Log(this, $"快照預設存檔資料夾: {snapshotPath}");
    }
    private void Start()
    {
        InitPool();
    }
    public void OnHexTileClicked(Editor_HexTile tile)
    {
        if (selectedTile!= null)
        {
            selectedTile.GetComponent<Image>().color = Color;
            
        }

        selectedTile = tile;
        if (tile.occupant == null)
        {
            tile.GetComponent<Image>().color = Color.yellow;
            CharacterDetailPanel.Instance.Close();
        }

    }
    public void OnCharacterButtonClicked(Character characterData)
    {
        if (selectedTile != null && selectedTile.occupant == null)
        {
            selectedTile.SetOccupant(characterData);
            CustomLogger.Log(this, $"選擇了{characterData}，放置在{selectedTile}");
            selectedTile = null;
        }
        else
        {
            CustomLogger.LogWarning(this, "尚未選擇空白HexTile就點角色了，無法放置。");
        }
    }
    public void SaveSnapshot()
    {
        EnemyWaveData enemyWaveData = new EnemyWaveData();
        foreach (var item in editor_HexTiles)
        {
            if (item.occupant!= null)
            {
                EnemyWaveData.GridSlotData gridSlotData = new EnemyWaveData.GridSlotData();
                gridSlotData.GridIndex = item.index;
                gridSlotData.CharacterID = item.occupant.CharacterId;
                gridSlotData.Level = item.editedLevel;
                gridSlotData.EquipmentIDs = item.editedEquipmentIDs;
                enemyWaveData.gridSlots.Add(gridSlotData);
            }
        }
        enemyWaveData.logisticSlot1.CharacterID = -1;
        enemyWaveData.logisticSlot1.GridIndex = -1;
        enemyWaveData.logisticSlot1.Level = 1;
        enemyWaveData.logisticSlot1.EquipmentIDs = new int[3] { -1, -1, -1 };
        enemyWaveData.logisticSlot2.CharacterID = -1;
        enemyWaveData.logisticSlot2.GridIndex = -1;
        enemyWaveData.logisticSlot2.Level = 1;
        enemyWaveData.logisticSlot2.EquipmentIDs = new int[3] { -1, -1, -1 };
        enemyWaveData.logisticSlot2.DummyGridIndex = -1;
        EnemyWave enemyWave = EnemyWaveConverter.FromData(enemyWaveData);
        string userInput = fileNameInputField.text.Trim();
        if (string.IsNullOrEmpty(userInput))
        {
            userInput = DefaultName;
        }
        string json = JsonUtility.ToJson(enemyWaveData, true);
        string fileName = userInput + ".json";
        string finalPath = Path.Combine(snapshotPath, fileName);
        if (File.Exists(finalPath))
        {
            if (overrideToggle == null || !overrideToggle.isOn)
            {
                int index = 1;
                string baseName = userInput;
                string extension = ".json";
                string tryPath;
                do
                {
                    string newName = $"{baseName} ({index}){extension}";
                    tryPath = Path.Combine(snapshotPath, newName);
                    index++;
                }
                while (File.Exists(tryPath));
                finalPath = tryPath;
            }
            else
            {
                
            }
        }
        File.WriteAllText(finalPath, json);
        CustomLogger.Log(this, $"快照儲存完成 => {finalPath}");
    }
    public void LoadSnapshot()
    {
        if (!File.Exists(snapshotPath))
        {
            CustomLogger.LogError(this, "找不到快照檔案，無法讀取");
            return;
        }
        string json = File.ReadAllText(snapshotPath);
        EnemyWaveData data = JsonUtility.FromJson<EnemyWaveData>(json);
    }
    public void ModifyPressureStack(int amount)
    {

    }
    public void InitPool()
    {
        LoadResources<Character>("1Cost", ref OneCostCharacter);
        LoadResources<Character>("2Cost", ref TwoCostCharacter);
        LoadResources<Character>("3Cost", ref ThreeCostCharacter);
        LoadResources<Character>("4Cost", ref FourCostCharacter);
        LoadResources<Character>("5Cost", ref FiveCostCharacter);
        LoadResources<Character>("Special", ref SpecialCharacter);
        LoadResources<Character>("TestBuildCharacter", ref TestBuildCharacter);
        Lists.Add(OneCostCharacter);
        Lists.Add(TwoCostCharacter);
        Lists.Add(ThreeCostCharacter);
        Lists.Add(FourCostCharacter);
        Lists.Add(FiveCostCharacter);
        Lists.Add(SpecialCharacter);
        Lists.Add(TestBuildCharacter);
        foreach (var characterList in Lists)
        {
            foreach (var character in characterList)
            {
                GameObject buttonObj = Instantiate(characterButtonPrefab, characterContentParent);
                Editor_CharacterButton e =buttonObj.AddComponent<Editor_CharacterButton>();
                e.characterData = character;
                e.characterImage = buttonObj.GetComponent<Image>();
                Button button = buttonObj.GetComponent<Button>();
                Image image = buttonObj.GetComponent<Image>();
                if (character.Sprite != null)
                {
                    image.sprite = character.Sprite;
                    image.color = Color.white;
                }
            }
        }
        EquipmentSO[] equipments = Resources.LoadAll<EquipmentSO>("Equipments");
        foreach (var equipment in equipments)
        {
            if (equipment.isSpecial)
            {
                
            }
            else if (equipment.IsConsumable)
            {
                
            }
            else if (equipment.Index >= 6)
            {
                CombinedEquipment combinedEquipment = new(equipment);
                CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(equipment.Index);
                if (c != null)
                {
                    combinedEquipment.observer = c;
                }
                availableEquipments.Add(combinedEquipment);
            }
            else if (equipment.Index < 6)
            {
                CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(equipment.Index);
                BasicEquipment basicEquipment = new(equipment);
                basicEquipment.observer = c;
                availableEquipments.Add(basicEquipment);
                CustomLogger.Log(this, $"Add {basicEquipment}, id = {basicEquipment.id},{basicEquipment.Icon}");
            }
        }
        CharacterDetailPanel.Instance.Init();
    }

    void LoadResources<T>(string path, ref List<T> list) where T : Object
    {
        list = Resources.LoadAll<T>(path).ToList();
    }
    public void DeleteAllSnapshots()
    {
        string[] jsonFiles = Directory.GetFiles(snapshotPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string filePath in jsonFiles)
        {
            try
            {
                File.Delete(filePath);
                CustomLogger.Log(this, $"已刪除檔案: {filePath}");
            }
            catch (System.Exception ex)
            {
                CustomLogger.LogError(this, $"刪除檔案失敗: {filePath}, 錯誤訊息: {ex.Message}");
            }
        }
    }

}
