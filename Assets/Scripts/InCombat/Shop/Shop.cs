using GameEnum;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop Instance;
    public List<int> Selled = new List<int>();
    public List<Image> pricesImg = new List<Image>();
    private List<Sprite> priceSprite = new List<Sprite>();
    public List<Button> ShopButtons = new List<Button>();
    private List<Image> images = new List<Image>();
    private List<GameObject> Characters = new List<GameObject>();
    private List<int> prices = new List<int>();
    public BenchManager benchManager;
    public RoundProbabilityData roundProbabilityData;
    public Button RefreshButton;
    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        for (int i = 0; i < ShopButtons.Count; i++)
        {
            images.Add(ShopButtons[i].GetComponent<Image>());
            Characters.Add(null);
            prices.Add(0);
            int indx = i;
            ShopButtons[indx].onClick.AddListener(() => SpawnCharacter(indx));
        }
    }
    public void Update()
    {
        int gold = GameController.Instance.GetGoldAmount();
        for (int i = 0; i <5; i++)
        {
            if (prices[i] > gold)
            {
                ShopButtons[i].interactable = false;
            }
            else if (!Selled.Contains(i))
            {
                ShopButtons[i].interactable = true;
            }
        }
        if (gold < 2)
        {
            RefreshButton.interactable = false;
        }
    }
    public void GoldLessRefresh()
    {
        GetCharacter();
    }
    public void Refresh()
    {
        Selled.Clear();
        GameController.Instance.AddGold(-2);
        GetCharacter();
    }

    public void SpawnCharacter(int index)
    {
        var shopButton = ShopButtons[index].gameObject.GetComponent<ShopButton>();
        GameController.Instance.AddGold(-prices[index]);
        shopButton.SetImagesNull();
        if (!benchManager.IsBenchFull())
        {
            bool added = benchManager.AddToBench(Characters[index]);
            if (added)
            {
                Selled.Add(index);
                images[index].sprite = null;
                priceSprite[index] = null;
                images[index].color = new Color(1, 1, 1, 0);
                ShopButtons[index].interactable = false;
                UpdateShopUI();
            }
        }
        else
        {
            
        }
    }

    public void GetCharacter()
    {
        priceSprite.Clear();
        prices.Clear();
        int currentRound = GameStageManager.Instance.GetRound();
        RoundProbability p = roundProbabilityData.roundProbabilities[currentRound];
        CustomLogger.Log(this,$"p.OneCostProbability={p.OneCostProbability},p.TwoCostProbability={p.TwoCostProbability},p.ThreeCostProbability={p.ThreeCostProbability},p.p.FourCostProbability={p.FourCostProbability},p.FiveCostProbability={p.FiveCostProbability}");
        for (int i = 0; i < ShopButtons.Count; i++)
        {
            float rand = Random.Range(0, 100);
            int cost;
            if (rand < p.OneCostProbability) cost = 1;
            else if (rand < p.OneCostProbability + p.TwoCostProbability) cost = 2;
            else if (rand < p.OneCostProbability + p.TwoCostProbability + p.ThreeCostProbability) cost = 3;
            else if (rand < p.OneCostProbability + p.TwoCostProbability + p.ThreeCostProbability + p.FourCostProbability) cost = 4;
            else cost = 5;
            priceSprite.Add(ResourcePool.Instance.Getnumber(cost));
            prices.Add(cost);

            int selectedCharacterId = GetRandomCharacterId(
                cost == 1 ? ResourcePool.Instance.OneCostCharacter :
                cost == 2 ? ResourcePool.Instance.TwoCostCharacter :
                cost == 3 ? ResourcePool.Instance.ThreeCostCharacter :
                cost == 4 ? ResourcePool.Instance.FourCostCharacter :
                            ResourcePool.Instance.FiveCostCharacter
            );

            Character character = ResourcePool.Instance.GetCharacterByID(selectedCharacterId);
            if (character == null) continue;

            ShopButton shopButton = ShopButtons[i].GetComponent<ShopButton>();
            List<Traits> temp = new List<Traits>(character.Traits);

            for (int k = 0; k < temp.Count; k++)
            {
                if (TraitDescriptions.Instance.GetTraitIsAcademy(temp[k]))
                {
                    shopButton.AcademyIcon.sprite = TraitDescriptions.Instance.GetTraitImage(temp[k]);
                    shopButton.AcademyIcon.color = new Color(1, 1, 1, 1);
                    temp.RemoveAt(k);
                    break;
                }
            }

            for (int j = 0; j < 2; j++)
            {
                if (j < temp.Count)
                {
                    shopButton.traitIcon[j].sprite = TraitDescriptions.Instance.GetTraitImage(temp[j]);
                    shopButton.traitIcon[j].color = new Color(1, 1, 1, 1);
                }
                else
                {
                    shopButton.traitIcon[j].sprite = null;
                    shopButton.traitIcon[j].color = new Color(1, 1, 1, 0);
                }
            }

            if (images[i] != null)
            {
                images[i].sprite = character.Sprite;
                pricesImg[i].sprite = priceSprite[i];
                pricesImg[i].color = new Color(1, 1, 1, 1);
                images[i].color = new Color(1, 1, 1, 1);
            }

            Characters[i] = character.Model;
            if (ShopButtons[i] != null) ShopButtons[i].interactable = true;

            CustomLogger.Log(this, $"Generated character: {character.CharacterName}, Cost: {cost}");
        }

        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        for (int i = 0; i < Characters.Count; i++)
        {
            // 確保該按鈕的圖片和角色都有效
            if (ShopButtons[i].interactable && images[i].sprite != null)
            {
                CharacterCTRL characterCTRL = Characters[i].GetComponent<CharacterCTRL>();
                int characterId = ResourcePool.Instance.GetCharacterByID(characterCTRL.characterStats.CharacterId).CharacterId;

                // 計算場上和備戰席的該角色
                int owned1StarCount = CountOwnedCharactersWithSameStar(characterId, 1);  // 只計算一星角色
                int owned2StarCount = CountOwnedCharactersWithSameStar(characterId, 2);
                // 根據擁有數量調整高亮顏色
                if (owned1StarCount == 0 && owned2StarCount == 0)
                {
                    images[i].color = new Color(1, 1, 1, 1); // 重設為默認顏色
                }
                else if (owned1StarCount == 2)
                {
                    images[i].color = new Color(1f, 0.84f, 0f, 1); // 金黃色高亮，僅在有兩個一星角色時
                }
                else if (owned1StarCount == 1 || owned2StarCount == 1)
                {
                    images[i].color = new Color(0.7f, 0.7f, 0.7f, 1); // 變暗顯示
                }

            }
        }
    }

    // 計算相同角色ID且星級為指定星級的角色數量
    private int CountOwnedCharactersWithSameStar(int characterId, int starLevel)
    {
        int count = 0;
        foreach (var character in benchManager.characterParent.childCharacters)
        {
            CharacterCTRL ctrl = character.GetComponent<CharacterCTRL>();
            if (ctrl.characterStats.CharacterId == characterId && ctrl.star == starLevel)
            {
                count++;
            }
        }
        return count;
    }
    private int GetRandomCharacterId(List<Character> characterList)
    {
        if (characterList.Count > 0)
        {
            int randIndex = Random.Range(0, characterList.Count);
            return characterList[randIndex].CharacterId;
        }
        return -1;
    }
}
