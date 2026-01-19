using GameEnum;
using System.Collections.Generic;
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
    public int SRT_RerollCount;
    public int freeReroll;
    public List<Sprite> SRT_statsImage = new List<Sprite>();
    public int SRT_RandStatIndex;
    private bool augment1002ReplaceToggle = false;
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
        for (int i = 0; i < prices.Count; i++)
        {
            bool affordable = prices[i] <= gold;
            bool notSold = !Selled.Contains(i);
            ShopButtons[i].interactable = affordable && notSold;
        }
        RefreshButton.interactable = gold >= 2;
    }

    public void GoldLessRefresh()
    {
        GetCharacter();
    }

    public void Refresh()
    {
        Selled.Clear();
        if (freeReroll > 0)
            freeReroll--;
        else
            GameController.Instance.AddGold(-2);
        if (SelectedAugments.Instance.CheckAugmetExist(1039, true))
        {
            if (Utility.GetRandfloat() > 0.5f)
            {
                freeReroll++;
            }
        }
        GetCharacter();
    }

    public void SpawnCharacter(int index)
    {
        CustomLogger.Log(this, $"SpawnCharacter calling at {index}");

        if (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2 && SRT_RandStatIndex != -1)
        {
            GameController.Instance.AddGold(-2);
            SRTManager.instance.AddStat(SRT_RandStatIndex, true);
            SRT_RandStatIndex = -1;
            ShopButtons[index].interactable = false;
            images[index].color = new Color(1, 1, 1, 0);
            images[index].sprite = null;
            prices.RemoveAt(index);
            priceSprite[index] = null;
            return;
        }

        if (benchManager.IsBenchFull())
        {
            PopupManager.Instance.CreatePopup("Full Board", 2);
            return;
        }

        if (benchManager.AddToBench(Characters[index]))
        {
            ShopButton shopButton = ShopButtons[index].GetComponent<ShopButton>();
            Selled.Add(index);
            images[index].sprite = null;
            priceSprite[index] = null;
            images[index].color = new Color(1, 1, 1, 0);

            ShopButtons[index].interactable = false;
            shopButton.SetImagesNull();
            UpdateShopUI();
            GameController.Instance.AddGold(-prices[index]);
        }
    }

    private int GetMaxAvailableCost(RoundProbability p)
    {
        if (p.FiveCostProbability > 0f) return 5;
        if (p.FourCostProbability > 0f) return 4;
        if (p.ThreeCostProbability > 0f) return 3;
        if (p.TwoCostProbability > 0f) return 2;
        return 1;
    }

    private List<Character> GetPoolByCost(int cost)
    {
        return cost switch
        {
            1 => ResourcePool.Instance.OneCostCharacter,
            2 => ResourcePool.Instance.TwoCostCharacter,
            3 => ResourcePool.Instance.ThreeCostCharacter,
            4 => ResourcePool.Instance.FourCostCharacter,
            _ => ResourcePool.Instance.FiveCostCharacter
        };
    }

    private void ReplaceSlotWithHighestCostRandomCharacter(int slotIndex, RoundProbability p)
    {
        int maxCost = GetMaxAvailableCost(p);
        List<Character> pool = GetPoolByCost(maxCost);

        Character character = ResourcePool.Instance.GetCharacterByID(GetRandomCharacterId(pool));
        if (character == null) return;

        Characters[slotIndex] = character.Model;
        SetupShopButton(slotIndex, character);
        ShopButtons[slotIndex].interactable = true;

        var spr = ResourcePool.Instance.Getnumber(maxCost);
        if (priceSprite.Count > slotIndex) priceSprite[slotIndex] = spr;
        if (prices.Count > slotIndex) prices[slotIndex] = maxCost;

        pricesImg[slotIndex].sprite = spr;
        pricesImg[slotIndex].color = new Color(1, 1, 1, 1);

        CustomLogger.Log(this, $"[Augment1002] Replaced slot {slotIndex} with: {character.CharacterName}, Cost: {maxCost}");
    }

    public void GetCharacter()
    {
        bool hasAug1002 = SelectedAugments.Instance.CheckAugmetExist(1002, true);

        if (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2 && ++SRT_RerollCount >= 4)
        {
            SRT_RerollCount = 0;
            freeReroll++;
        }

        priceSprite.Clear();
        prices.Clear();

        int currentRound = GameStageManager.Instance.GetRound();
        RoundProbability p = roundProbabilityData.roundProbabilities[currentRound];

        int count = 5 - (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2 ? 1 : 0);
        if (SelectedAugments.Instance.CheckAugmetExist(126, true))
        {
            count = 5;
            SRTManager.instance.AddStat(UnityEngine.Random.Range(0, 4), true);
        }

        for (int i = 0; i < count; i++)
        {
            float rand = UnityEngine.Random.Range(0, 100);
            int cost = rand < p.OneCostProbability ? 1 :
                       rand < p.OneCostProbability + p.TwoCostProbability ? 2 :
                       rand < p.OneCostProbability + p.TwoCostProbability + p.ThreeCostProbability ? 3 :
                       rand < p.OneCostProbability + p.TwoCostProbability + p.ThreeCostProbability + p.FourCostProbability ? 4 : 5;

            priceSprite.Add(ResourcePool.Instance.Getnumber(cost));
            prices.Add(cost);

            List<Character> pool = GetPoolByCost(cost);

            Character character = ResourcePool.Instance.GetCharacterByID(GetRandomCharacterId(pool));
            if (character == null) continue;

            Characters[i] = character.Model;
            SetupShopButton(i, character);
            ShopButtons[i].interactable = true;
            CustomLogger.Log(this, $"Generated character: {character.CharacterName}, Cost: {cost}");
        }

        if (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2)
        {
            Characters[4] = null;
            ShopButton shopButton = ShopButtons[4].GetComponent<ShopButton>();
            shopButton.SetImagesNull();
            SRT_RandStatIndex = UnityEngine.Random.Range(0, 4);
            images[4].sprite = SRT_statsImage[SRT_RandStatIndex];
            images[4].color = new Color(1, 1, 1, 1);
            priceSprite.Add(ResourcePool.Instance.Getnumber(2));
            prices.Add(2);
            pricesImg[4].sprite = priceSprite[4];
            pricesImg[4].color = images[4].color = new Color(1, 1, 1, 1);
            ShopButtons[4].interactable = true;

            for (int j = 0; j < 2; j++)
            {
                shopButton.traitIcon[j].sprite = null;
                shopButton.traitIcon[j].color = new Color(1, 1, 1, 0);
            }
        }
        if (hasAug1002)
        {
            augment1002ReplaceToggle = !augment1002ReplaceToggle;
            if (augment1002ReplaceToggle)
            {
                int slotIndex = (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2) ? 3 : 4;
                ReplaceSlotWithHighestCostRandomCharacter(slotIndex, p);

            }
        }
        UpdateShopUI();
    }

    private void SetupShopButton(int index, Character character)
    {
        ShopButton shopButton = ShopButtons[index].GetComponent<ShopButton>();
        List<Traits> traits = new List<Traits>(character.Traits);

        for (int k = 0; k < traits.Count; k++)
        {
            if (TraitDescriptions.Instance.GetTraitIsAcademy(traits[k]))
            {
                shopButton.AcademyIcon.sprite = TraitDescriptions.Instance.GetTraitImage(traits[k]);
                shopButton.AcademyIcon.color = new Color(1, 1, 1, 1);
                traits.RemoveAt(k);
                break;
            }
        }

        for (int j = 0; j < 2; j++)
        {
            if (j < traits.Count)
            {
                shopButton.traitIcon[j].sprite = TraitDescriptions.Instance.GetTraitImage(traits[j]);
                shopButton.traitIcon[j].color = new Color(1, 1, 1, 1);
            }
            else
            {
                shopButton.traitIcon[j].sprite = null;
                shopButton.traitIcon[j].color = new Color(1, 1, 1, 0);
            }
        }

        images[index].sprite = character.Sprite;
        pricesImg[index].sprite = priceSprite[index];
        pricesImg[index].color = images[index].color = new Color(1, 1, 1, 1);
    }

    public void UpdateShopUI()
    {
        int count = 5 - (ResourcePool.Instance.ally.GetSpecificTrait(Traits.SRT) >= 2 ? 1 : 0);

        for (int i = 0; i < count; i++)
        {
            if (!ShopButtons[i].interactable || images[i].sprite == null) continue;

            CharacterCTRL ctrl = Characters[i].GetComponent<CharacterCTRL>();
            int id = ResourcePool.Instance.GetCharacterByID(ctrl.characterStats.CharacterId).CharacterId;
            int count1 = CountOwnedCharactersWithSameStar(id, 1);
            int count2 = CountOwnedCharactersWithSameStar(id, 2);

            images[i].color = (count1 == 2) ? new Color(1f, 0.84f, 0f, 1) :
                              (count1 == 1 || count2 == 1) ? new Color(0.7f, 0.7f, 0.7f, 1) :
                              new Color(1, 1, 1, 1);
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
            int randIndex = UnityEngine.Random.Range(0, characterList.Count);
            int result = characterList[randIndex].CharacterId;
            if (!SelectedAugments.Instance.CheckAugmetExist(4, true) && result == 504)
            {
                result = 4;
            }
            if (SelectedAugments.Instance.CheckAugmetExist(4, true) && result == 4)
            {
                result += 500;
            }
            return result;
        }
        return -1;
    }
}
