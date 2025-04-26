using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using TMPro;

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
            selectedTile.GetComponent<Image>().color = Color.white;
        }

        selectedTile = tile;
        tile.GetComponent<Image>().color = Color.yellow;
        CustomLogger.Log(this, $"選擇了空白HexTile: {tile.name}");
    }

    // 當點擊一個角色按鈕
    public void OnCharacterButtonClicked(Character characterData)
    {
        if (selectedTile != null && selectedTile.occupant == null)
        {
            // 指派角色給這個 tile
            selectedTile.SetOccupant(characterData);
            CustomLogger.Log(this, $"選擇了{characterData}，放置在{selectedTile}");
            selectedTile = null; // 取消選中

        }
        else
        {
            // 代表沒有選到空白格，就純粹按了角色按鈕；可能可以給個提示
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
                gridSlotData.Level = 1;
                enemyWaveData.gridSlots.Add(gridSlotData);
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
        }
        EnemyWave enemyWave = EnemyWaveConverter.FromData(enemyWaveData);
        // 1) 先拿到使用者輸入的檔名
        string userInput = fileNameInputField.text.Trim();
        if (string.IsNullOrEmpty(userInput))
        {
            // 若沒輸入，則用預設檔名
            userInput = DefaultName;
        }
        string json = JsonUtility.ToJson(enemyWaveData, true);
        string fileName = userInput + ".json";
        string finalPath = Path.Combine(snapshotPath, fileName);
        if (File.Exists(finalPath))
        {
            if (overrideToggle == null || !overrideToggle.isOn)
            {
                // 沒有選覆蓋 => 自動產生新檔名
                // 例如  "MyWave.json" 變成 "MyWave (1).json"、"MyWave (2).json" 等
                int index = 1;
                string baseName = userInput; // 不含 .json
                string extension = ".json";
                string tryPath;
                do
                {
                    string newName = $"{baseName} ({index}){extension}";
                    tryPath = Path.Combine(snapshotPath, newName);
                    index++;
                }
                while (File.Exists(tryPath));

                // 找到可用的 path
                finalPath = tryPath;
            }
            else
            {
                // overrideToggle.isOn = true => 直接覆蓋
                // 就什麼都不用做
            }
        }

        // 5) 寫出檔案
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

        // 從外部檔案讀 JSON 字串，反序列化成 Data，再回寫到 ScriptableObject
        string json = File.ReadAllText(snapshotPath);
        EnemyWaveData data = JsonUtility.FromJson<EnemyWaveData>(json);
    }

    // 這裡可以示範「在遊戲中修改 targetWave」的流程
    // 假設有個公開的方法給 UI Button 來加減數值
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
    }
    void LoadResources<T>(string path, ref List<T> list) where T : Object
    {
        list = Resources.LoadAll<T>(path).ToList();
    }
}
