using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ResourcePool : MonoBehaviour
{

    public static ResourcePool Instance { get; private set; }
    public GameObject floorPrefab, bulletPrefab, characterBarPrefab, FloatingTextPrefab, wallPrefab;
    public Transform floorParent, bulletParent, characterBarParent, FloatingTextParent, wallParent;
    public const int floorCount = 64, bulletCount = 50, barCount = 20, TextCount = 50, wallCount = 50;
    public List<GameObject> floorPool = new(), bulletPool = new(), barPool = new(), textPool = new(), wallPool = new();
    public List<Character> OneCostCharacter, TwoCostCharacter, ThreeCostCharacter, FourCostCharacter, FiveCostCharacter, SpecialCharacter;
    public List<List<Character>> Lists = new();
    public GameEvent AllResoucesLoaded;

    public Transform penetrateBulletParent;
    public Transform healPackParent;
    public Transform normalBulletParent;
    public GameObject PenetrateTrailedBullet;
    public GameObject HealPack;
    public GameObject NormalTrailBullet;

    public CharacterParent ally;
    public CharacterParent enemy;

    public HexNode EnemylogisticSlotNode1;
    public HexNode EnemylogisticSlotNode2;

    public GameObject LogisticDummy;

    private Dictionary<int, Character> characterDictionary = new Dictionary<int, Character>();
    public CombinationRouteSO combinationRoute;

    public BenchManager BenchManager;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(InitializeAll());
    }
    void LoadResources<T>(string path, ref List<T> list) where T : Object
    {
        list = Resources.LoadAll<T>(path).ToList();
    }

    void PopulateCharacterDictionary()
    {
        foreach (var characterList in Lists)
        {
            StringBuilder sb = new();
            foreach (var character in characterList)
            {
                if (!characterDictionary.ContainsKey(character.CharacterId))
                {
                    sb.AppendLine($"character {character.CharacterName} ,level = {character.Level}");
                    float res = character.Stats.GetStat(StatsType.Resistence);
                    float ratio = res/(100 + res) ;
                    sb.AppendLine($"effective health = {character.Stats.GetStat(StatsType.Health)*(1/(1-ratio))}");
                    sb.AppendLine($"dps = {character.Stats.GetStat(StatsType.Attack)* character.Stats.GetStat(StatsType.AttackSpeed)}");
                    sb.AppendLine($"range = {character.Stats.GetStat(StatsType.Range)}");
                    if (character.logistics)
                    {
                        sb.AppendLine($"(logistics)");
                    }
                    sb.AppendLine("");
                    characterDictionary.Add(character.CharacterId, character);
                }
                else
                {
                    Debug.LogWarning($"CharacterId {character.CharacterId} is already in the dictionary!");
                }
            }
            Debug.Log(sb.ToString());
        }
    }
    public List<Character> GetAllCharacters()
    {
        var list = new List<Character>();
        foreach (var item in characterDictionary.Values)
        {
            list.Add(item);
        }
        return list;
    }
    public Character GetCharacterByID(int id)
    {
        if (characterDictionary.TryGetValue(id, out var character))
        {
            return character;
        }
        else
        {
            Debug.LogError($"Character with ID {id} not found!");
            return null;
        }
    }

    IEnumerator InitializeAll()
    {
        // 資源載入
        LoadResources<Character>("1Cost", ref OneCostCharacter);
        LoadResources<Character>("2Cost", ref TwoCostCharacter);
        LoadResources<Character>("3Cost", ref ThreeCostCharacter);
        LoadResources<Character>("4Cost", ref FourCostCharacter);
        LoadResources<Character>("5Cost", ref FiveCostCharacter);
        LoadResources<Character>("Special", ref SpecialCharacter);

        Lists.Add(OneCostCharacter);
        Lists.Add(TwoCostCharacter);
        Lists.Add(ThreeCostCharacter);
        Lists.Add(FourCostCharacter);
        Lists.Add(FiveCostCharacter);
        Lists.Add(SpecialCharacter);

        PopulateCharacterDictionary();

        // 初始化物件池
        yield return StartCoroutine(InitPool(floorPrefab, floorParent, floorPool, floorCount));
        yield return StartCoroutine(InitPool(bulletPrefab, bulletParent, bulletPool, bulletCount));
        yield return StartCoroutine(InitPool(characterBarPrefab, characterBarParent, barPool, barCount));
        yield return StartCoroutine(InitPool(FloatingTextPrefab, FloatingTextParent, textPool, TextCount));
        yield return StartCoroutine(InitPool(wallPrefab, wallParent, wallPool, wallCount));
        yield return StartCoroutine(InitializeBulletPool(500));

        // 確保所有資源載入並等待短時間
        yield return new WaitForSeconds(0.1f);

        // 發送所有資源載入完成的通知
        AllResoucesLoaded.Raise();
    }
    IEnumerator InitializeBulletPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 使用 SpawnObject 方法生成子彈，並且位置設置為 (0, 0, 0)
            GameObject bullet = SpawnObject(SkillPrefab.NormalTrailedBullet, Vector3.zero, Quaternion.identity);

            if (bullet != null)
            {
                bullet.SetActive(false); // 預設禁用以便重新使用
                bulletPool.Add(bullet); // 將子彈加入到池子中
            }
        }
        yield return null;
    }

    IEnumerator InitPool(GameObject prefab, Transform parent, List<GameObject> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Spawn(prefab, parent, pool);
        }
        yield return null;
    }

    public GameObject Spawn(GameObject prefab, Transform parent, List<GameObject> pool)
    {
        GameObject obj = Instantiate(prefab, parent);
        pool.Add(obj);
        obj.SetActive(false);
        return obj;
    }

    public GameObject GetFloatingText(Vector3 pos)
    {
        return GetObject(FloatingTextPrefab, FloatingTextParent, textPool, pos);
    }

    public GameObject GetBar(Vector3 pos)
    {
        foreach (var item in barPool)
        {
            if (!item.activeInHierarchy && item.GetComponent<CharacterBars>().Parent == null)
            {
                item.transform.position = pos;
                item.SetActive(true);
                return item;
            }
        }
        GameObject obj = Spawn(characterBarPrefab, characterBarParent, barPool);
        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }

    public GameObject GetFloor(Vector3 pos)
    {
        return GetObject(floorPrefab, floorParent, floorPool, pos);
    }
    public GameObject GetWall(Vector3 pos)
    {
        return GetObject(wallPrefab, wallParent, wallPool, pos);
    }
    public GameObject GetBullet(Vector3 pos)
    {
        return GetObject(bulletPrefab, bulletParent, bulletPool, pos);
    }

    public GameObject GetObject(GameObject prefab, Transform parent, List<GameObject> pool, Vector3 pos)
    {
        foreach (var item in pool)
        {
            if (!item.activeInHierarchy)
            {
                item.transform.position = pos;
                item.SetActive(true);
                return item;
            }
        }
        GameObject obj = Spawn(prefab, parent, pool);
        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }
    public GameObject SpawnCharacterAtPosition(GameObject characterPrefab, Vector3 position, HexNode hexNode, CharacterParent characterParent, bool isAlly = false)
    {
        GameObject obj = Instantiate(characterPrefab);
        obj.transform.position = position + new Vector3(0, 0.14f, 0);
        obj.transform.rotation = Quaternion.Euler(0, 180, 0);
        obj.layer = isAlly ? 8 : 9;

        CharacterDrager drager = obj.GetComponent<CharacterDrager>();
        drager.PreSelectedGrid = hexNode;


        CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
        ctrl.star = 1;
        ctrl.CurrentHex = hexNode;
        hexNode.OccupyingCharacter = ctrl;
        hexNode.Reserve(ctrl);
        CharacterBars bar = GetBar(position).GetComponent<CharacterBars>();

        ctrl.SetBarChild(bar);
        ctrl.characterBars = bar;
        if (isAlly) ctrl.AudioManager.PlaySummonedSound();
        CustomLogger.Log(this, $"get bar to {obj.name},bar parent = {ctrl},child = {ctrl.characterBars}");
        bar.SetBarsParent(obj.transform);

        TraitController traitController = obj.GetComponent<TraitController>();
        foreach (var item in ctrl.characterStats.Traits)
        {
            traitController.AddTrait(item);
        }


        characterParent.AddChild(obj);
        obj.transform.SetParent(characterParent.transform, false);
        ctrl.IsAlly = isAlly;

        return obj;
    }

    public GameObject SpawnObject(SkillPrefab skillPrefabName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = null;
        Transform parent = null; // 根據 prefab 指派的父物件

        // 根據 skillPrefabName 設置 prefab 和對應的父物件
        switch (skillPrefabName)
        {
            case SkillPrefab.PenetrateTrailedBullet:
                prefab = PenetrateTrailedBullet;
                parent = penetrateBulletParent;
                break;
            case SkillPrefab.HealPack:
                prefab = HealPack;
                parent = healPackParent;
                break;
            case SkillPrefab.NormalTrailedBullet:
                prefab = NormalTrailBullet;
                parent = normalBulletParent;
                break;
        }

        if (prefab != null && parent != null)
        {
            // 嘗試在父物件中找到已禁用的同類物件
            GameObject existingObject = FindDisabledChild(prefab.name, parent);
            if (existingObject != null)
            {
                // 找到已禁用的物件，啟用並重新定位
                existingObject.transform.position = position;
                existingObject.transform.rotation = rotation;
                existingObject.SetActive(true);
                return existingObject;
            }

            // 沒有找到已禁用的物件，生成新的並將其設置為指定父物件的子物件
            return Instantiate(prefab, position, rotation, parent);
        }
        else
        {
            Debug.LogError("Prefab or parent not found: " + skillPrefabName.ToString());
            return null;
        }
    }

    // 查找禁用的同類子物件
    private GameObject FindDisabledChild(string prefabName, Transform parent)
    {
        foreach (Transform child in parent)
        {
            // 如果子物件名稱匹配並且當前處於禁用狀態
            if (child.name.Contains(prefabName) && !child.gameObject.activeInHierarchy)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
