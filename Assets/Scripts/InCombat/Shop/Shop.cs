using GameEnum;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public List<Button> ShopButtons = new List<Button>();
    private List<Image> images = new List<Image>();
    private List<GameObject> Characters = new List<GameObject>();
    private List<int> prices = new List<int>();
    public BenchManager benchManager;
    public RoundProbabilityData roundProbabilityData;

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
            else
            {
                ShopButtons[i].interactable = true;
            }
        }
    }
    public void Refresh()
    {
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
                images[index].sprite = null;
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
        prices.Clear();
        int currentRound = 5; // ���]���Ĥ���
        RoundProbability currentProbability = roundProbabilityData.roundProbabilities[currentRound];
        for (int i = 0; i < ShopButtons.Count; i++)
        {
            float rand = Random.Range(0, 100);
            int selectedCharacterId = -1;
            if (rand < currentProbability.OneCostProbability)
            {
                prices.Add(1);
                selectedCharacterId = GetRandomCharacterId(ResourcePool.Instance.OneCostCharacter);
                if (selectedCharacterId == 4)
                {
                    selectedCharacterId += 500;
                    CustomLogger.Log(this, "switch to spring");
                }
            }
            else if (rand < currentProbability.OneCostProbability + currentProbability.TwoCostProbability)
            {
                prices.Add(2);
                selectedCharacterId = GetRandomCharacterId(ResourcePool.Instance.TwoCostCharacter);
            }
            else
            {
                prices.Add(3);
                selectedCharacterId = GetRandomCharacterId(ResourcePool.Instance.ThreeCostCharacter);
            }

            Character character = ResourcePool.Instance.GetCharacterByID(selectedCharacterId);

            if (character != null)
            {
                // �ƻs Traits �C��A�קK�����ާ@ character.Traits
                List<Traits> temp = new List<Traits>(character.Traits);
                var shopButton = ShopButtons[i].gameObject.GetComponent<ShopButton>();
                for (int k = 0; k < temp.Count; k++)
                {
                    if (TraitDescriptions.Instance.GetTraitIsAcademy(temp[k]))
                    {
                        shopButton.AcademyIcon.sprite = TraitDescriptions.Instance.GetTraitImage(temp[k]);
                        shopButton.AcademyIcon.color = new Color(1, 1, 1, 1);
                        temp.RemoveAt(k);
                    }
                }
                List<Traits> _temp = new List<Traits>(temp);
                for (int j = 0; j < 2; j++)
                {

                    if (j < _temp.Count)
                    {
                        shopButton.traitIcon[j].sprite = TraitDescriptions.Instance.GetTraitImage(_temp[j]);
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
                    images[i].color = new Color(1, 1, 1, 1);
                }

                Characters[i] = character.Model;
                if (ShopButtons[i] != null)
                {
                    ShopButtons[i].interactable = true;
                }
            }

        }

        UpdateShopUI();
    }
    public void UpdateShopUI()
    {
        for (int i = 0; i < Characters.Count; i++)
        {
            // �T�O�ӫ��s���Ϥ��M���ⳣ����
            if (ShopButtons[i].interactable && images[i].sprite != null)
            {
                CharacterCTRL characterCTRL = Characters[i].GetComponent<CharacterCTRL>();
                int characterId = ResourcePool.Instance.GetCharacterByID(characterCTRL.characterStats.CharacterId).CharacterId;

                // �p����W�M�ƾԮu���Ө���
                int owned1StarCount = CountOwnedCharactersWithSameStar(characterId, 1);  // �u�p��@�P����
                int owned2StarCount = CountOwnedCharactersWithSameStar(characterId, 2);
                // �ھھ֦��ƶq�վ㰪�G�C��
                if (owned1StarCount == 0 && owned2StarCount == 0)
                {
                    images[i].color = new Color(1, 1, 1, 1); // ���]���q�{�C��
                }
                else if (owned1StarCount == 2)
                {
                    images[i].color = new Color(1f, 0.84f, 0f, 1); // �����Ⱚ�G�A�Ȧb����Ӥ@�P�����
                }
                else if (owned1StarCount == 1 || owned2StarCount == 1)
                {
                    images[i].color = new Color(0.7f, 0.7f, 0.7f, 1); // �ܷt���
                }

            }
        }
    }

    // �p��ۦP����ID�B�P�Ŭ����w�P�Ū�����ƶq
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
