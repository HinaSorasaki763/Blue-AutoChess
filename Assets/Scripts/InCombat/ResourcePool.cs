using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ResourcePool : MonoBehaviour
{

    public static ResourcePool Instance { get; private set; }
    public GameObject floorPrefab, characterBarPrefab, FloatingTextPrefab, wallPrefab;
    public Transform floorParent, characterBarParent, FloatingTextParent, wallParent;
    public const int floorCount = 64,barCount = 20, TextCount = 50, wallCount = 50;
    public List<GameObject> floorPool = new(), barPool = new(), textPool = new(), wallPool = new();
    public List<Character> OneCostCharacter, TwoCostCharacter, ThreeCostCharacter, FourCostCharacter, FiveCostCharacter, SpecialCharacter ,TestBuildCharacter;
    public List<List<Character>> Lists = new();
    public GameEvent AllResoucesLoaded;

    public Transform penetrateBulletParent;
    public Transform healPackParent;
    public Transform normalBulletParent;
    public Transform MissleFragmentsParent;
    public Transform MissleParent;
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
    public GameObject DropGoldPrefab;
    public GameObject DropRewardPrefab;

    public BenchManager BenchManager;
    public GameObject MisslePrefab;
    public GameObject MissleFragmentsPrefab;
    public GameObject SmallPenetrateTrailedBulletPrefab;
    public Transform SmallPenetrateTrailedBulletParent;
    public int RandomKeyThisGame;
    public readonly int FixedRandomKey = 1854998248;
    public Sprite RandomRewardSprite;
    public Sprite GoldSprite;
    public Sprite[] numberSprites;
    public Sprite WakamosSprite;
    public Canvas EffectCanva;
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
        RandomKeyThisGame = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        CustomLogger.Log(this, $"RandomKeyThisGame = {RandomKeyThisGame}");
    }
    public int GetRandomKey()
    {
        if (true)
        {

        }
        return RandomKeyThisGame;
    }
    void LoadResources<T>(string path, ref List<T> list) where T : Object
    {
        list = Resources.LoadAll<T>(path).ToList();
    }
    public Sprite Getnumber(int num)
    {
        if (num < 0 || num > 9)
        {
            Debug.LogError("number out of range");
            return null;
        }
        return numberSprites[num];
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
                    sb.AppendLine($"character {character.CharacterName} ,skill = {character.EnhancedSkillTooltips[0]}");
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
        StringBuilder Sb = new();
        foreach (var character in characterDictionary)
        {

            Sb.AppendLine($"character tooltips chinese : {character.Value.EnhancedSkillTooltips[0]}");

        }
        Debug.Log(Sb.ToString());
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
        LoadResources<Character>("TestBuildCharacter",ref TestBuildCharacter);
        Lists.Add(OneCostCharacter);
        Lists.Add(TwoCostCharacter);
        Lists.Add(ThreeCostCharacter);
        Lists.Add(FourCostCharacter);
        Lists.Add(FiveCostCharacter);
        Lists.Add(SpecialCharacter);
        Lists.Add(TestBuildCharacter);
        PopulateCharacterDictionary();
        yield return StartCoroutine(InitPool(floorPrefab, floorParent, floorPool, floorCount));
        yield return StartCoroutine(InitPool(characterBarPrefab, characterBarParent, barPool, barCount));
        yield return StartCoroutine(InitPool(FloatingTextPrefab, FloatingTextParent, textPool, TextCount));
        yield return StartCoroutine(InitPool(wallPrefab, wallParent, wallPool, wallCount));
        yield return new WaitForSeconds(0.1f);
        AllResoucesLoaded.Raise();
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
    public void GetGoldPrefab(Vector3 pos)
    {
        float randx = Random.Range(-0.5f, 0.5f);
        float randz = Random.Range(-0.5f, 0.5f);
        Vector3 p = pos + new Vector3(randx, 0.75f, randz);
        GameObject obj = Instantiate(DropGoldPrefab, p, Quaternion.identity);
        obj.GetComponent<GoldRotate>().rewardType = CollectionRewardType.Gold;
    }
    public void GetRandRewardPrefab(Vector3 pos)
    {
        float randx = Random.Range(-0.5f, 0.5f);
        float randz = Random.Range(-0.5f, 0.5f);
        Vector3 p = pos + new Vector3(randx, 0.75f, randz);
        GameObject obj = Instantiate(DropRewardPrefab, p, Quaternion.identity);
        obj.GetComponent<GoldRotate>().rewardType = CollectionRewardType.RandComponent;
    }
    public void GetRandCharacterPrefab(Vector3 pos,int index)
    {
        float randx = Random.Range(-0.5f, 0.5f);
        float randz = Random.Range(-0.5f, 0.5f);
        Vector3 p = pos + new Vector3(randx, 0.75f, randz);
        GameObject obj = Instantiate(DropRewardPrefab, p, Quaternion.identity);
        obj.GetComponent<GoldRotate>().rewardType = CollectionRewardType.Character;
        obj.GetComponent<GoldRotate>().CharacterIndex = index;
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
    public GameObject SpawnCharacterAtPosition(GameObject characterPrefab, Vector3 position, HexNode hexNode, CharacterParent characterParent, bool isAlly = false,int star = 1)
    {
        GameObject obj = Instantiate(characterPrefab);
        obj.transform.position = position + new Vector3(0, 0.23f, 0);
        CustomLogger.Log(this, $"{characterPrefab.name}spawned at {position}");
        obj.transform.rotation = Quaternion.Euler(0, 180, 0);
        obj.layer = isAlly ? 8 : 9;

        CharacterDrager drager = obj.GetComponent<CharacterDrager>();
        drager.PreSelectedGrid = hexNode;


        CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
        ctrl.star = star;
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
        ctrl.GetSkill();
        return obj;
    }

    public GameObject SpawnObject(SkillPrefab skillPrefabName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = null;
        Transform parent = null;
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
            case SkillPrefab.MissleFragmentsPrefab:
                prefab = MissleFragmentsPrefab;
                parent = MissleFragmentsParent;
                break;
            case SkillPrefab.SmallPenetrateTrailedBullet:
                prefab = SmallPenetrateTrailedBulletPrefab;
                parent = SmallPenetrateTrailedBulletParent;
                break;
            case SkillPrefab.Missle:
                prefab = MisslePrefab;
                parent = MissleParent;
                break;
        }

        if (prefab != null && parent != null)
        {
            GameObject existingObject = FindDisabledChild(prefab.name, parent);
            if (existingObject != null)
            {
                existingObject.transform.position = position;
                existingObject.transform.rotation = rotation;
                existingObject.SetActive(true);
                return existingObject;
            }
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
