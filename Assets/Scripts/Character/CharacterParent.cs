using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterParent : MonoBehaviour
{
    public List<GameObject> childCharacters = new();
    public GameEvent startBattle;
    public bool isEnemy;
    public Dictionary<Traits, int> currTraits = new Dictionary<Traits, int>();
    public bool IsBattling = false;
    private List<GameObject> totems = new List<GameObject>();
    private Dictionary<CharacterCTRL, GameObject> logisticsDummies = new Dictionary<CharacterCTRL, GameObject>();

    public void Start()
    {

    }
    public void OnStartBattle()
    {
        IsBattling = true;
        foreach (var character in childCharacters)
        {
            CharacterCTRL c = character.GetComponent<CharacterCTRL>();
            if (c.CurrentHex.IsBattlefield||c.CurrentHex.IsLogistics)
            {
                c.EnterBattle();
            }
        }
        GameStageManager.Instance.ChangeGamePhase(GamePhase.Battling);
    }
    public void ResetAllGodOfSonFlags()
    {
        foreach (var child in childCharacters)
        {
            CharacterCTRL character = child.GetComponent<CharacterCTRL>();
            if (character == null) continue;
            var observer = character.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            if (observer != null && observer.IsGodOfSon)
            {
                observer.SetGodOfSon(false);
                CustomLogger.Log(this, $"Reset IsGodOfSon for {character.name}");
            }
        }
    }
    public void GetAllGodOfSonFlags()
    {
        StringBuilder sb = new();
        foreach (var child in childCharacters)
        {
            CharacterCTRL character = child.GetComponent<CharacterCTRL>();
            if (character == null) continue;
            var observer = character.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;

            if (observer != null)
            {
                sb.AppendLine($"character {character} flag is {observer.IsGodOfSon}");
            }
        }
        CustomLogger.Log(this, sb.ToString());
    }

    public void CheckAndCombineCharacters()
    {
        Dictionary<int, Dictionary<int, List<CharacterCTRL>>> characterGroups = new();
        foreach (var character in childCharacters)
        {
            CharacterCTRL ctrl = character.GetComponent<CharacterCTRL>();
            int characterId = ctrl.characterStats.CharacterId;
            int star = ctrl.star;

            if (!characterGroups.ContainsKey(characterId))
            {
                characterGroups[characterId] = new Dictionary<int, List<CharacterCTRL>>();
            }
            if (!characterGroups[characterId].ContainsKey(star))
            {
                characterGroups[characterId][star] = new List<CharacterCTRL>();
            }
            if (!ctrl.isObj)
            {
                characterGroups[characterId][star].Add(ctrl);
            }
        }

        bool hasCombined;
        do
        {
            hasCombined = false;
            foreach (var group in characterGroups)
            {
                foreach (var starGroup in group.Value)
                {
                    if (starGroup.Value.Count >= 3)
                    {
                        if (starGroup.Value[0].isObj) continue;

                        // 暫存升級的角色，並即時更新
                        CharacterCTRL upgradedCharacter = starGroup.Value[0];
                        bool result = CombineCharacters(starGroup.Value);
                        if (result)
                        {
                            hasCombined = true;

                            // 移除舊分組資料
                            characterGroups[group.Key][upgradedCharacter.star - 1].Remove(upgradedCharacter);

                            // 重新加入升級後的角色到新分組
                            if (!characterGroups[group.Key].ContainsKey(upgradedCharacter.star))
                            {
                                characterGroups[group.Key][upgradedCharacter.star] = new List<CharacterCTRL>();
                            }
                            characterGroups[group.Key][upgradedCharacter.star].Add(upgradedCharacter);

                            break; // 跳出內層迴圈，重新檢查
                        }
                    }
                }
                if (hasCombined) break; // 如果有合成，跳出外層迴圈
            }
        }
        while (hasCombined);

        UpdateStrongestMarks(characterGroups);
        UpdateTraitEffects();
    }


    private bool CombineCharacters(List<CharacterCTRL> charactersToCombine)
    {
        if (charactersToCombine == null || charactersToCombine.Count < 3) return false;

        CharacterCTRL mainCharacter = charactersToCombine[0];
        mainCharacter.star++;
        for (int i = 1; i < 3; i++)
        {
            GameObject character = charactersToCombine[i].gameObject;
            childCharacters.Remove(character);
            Destroy(character);
        }

        if (mainCharacter.star > 3)
        {
            mainCharacter.star = 3;
        }

        mainCharacter.characterStats.ApplyLevelUp(mainCharacter.characterStats.Level + 1);
        mainCharacter.characterBars.UpdateStarLevel();  // 更新星級顯示
        mainCharacter.AudioManager.PlayOnStarUp();
        CustomLogger.Log(this, $"{mainCharacter.name} 已升級至 {mainCharacter.star} 星");

        return true;
    }


    // 查找 "最強" 的角色並設置標誌
    private void UpdateStrongestMarks(Dictionary<int, Dictionary<int, List<CharacterCTRL>>> characterGroups)
    {
        foreach (var group in characterGroups)
        {
            List<CharacterCTRL> allCharactersWithSameId = new List<CharacterCTRL>();

            // 將所有星級的角色放入一個列表中
            foreach (var starGroup in group.Value)
            {
                allCharactersWithSameId.AddRange(starGroup.Value);
            }

            // 找到 "最強" 角色
            CharacterCTRL strongestCharacter = FindStrongestCharacter(allCharactersWithSameId);

            // 對所有角色設置標誌
            foreach (var character in allCharactersWithSameId)
            {
                character.characterBars.SetStrongestMark(character == strongestCharacter);
            }
        }
    }

    // 根據屬性比較找出最強的角色
    private CharacterCTRL FindStrongestCharacter(List<CharacterCTRL> characters)
    {
        CharacterCTRL strongestCharacter = null;

        foreach (var character in characters)
        {
            if (strongestCharacter == null)
            {
                strongestCharacter = character;
            }
            else
            {
                // 依次比較星級、裝備數量、攻擊力、生命值
                if (character.star > strongestCharacter.star ||
                    (character.star == strongestCharacter.star && character.GetEquippedItemCount() > strongestCharacter.GetEquippedItemCount()) ||
                    (character.star == strongestCharacter.star && character.GetEquippedItemCount() == strongestCharacter.GetEquippedItemCount() && character.GetStat(StatsType.Attack) > strongestCharacter.GetStat(StatsType.Attack)) ||
                    (character.star == strongestCharacter.star && character.GetEquippedItemCount() == strongestCharacter.GetEquippedItemCount() && character.GetStat(StatsType.Attack) == strongestCharacter.GetStat(StatsType.Attack) && character.GetStat(StatsType.Health) > strongestCharacter.GetStat(StatsType.Health)))
                {
                    strongestCharacter = character;
                }
            }
        }

        return strongestCharacter;
    }

    public void AddChild(GameObject obj)
    {
        childCharacters.Add(obj);
        if (!isEnemy)
        {
            CheckAndCombineCharacters();
        }
        AddTraitEffects();
    }
    public void Update()
    {
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Preparing) return;
        bool allDisabled = true;

        foreach (var item in childCharacters)
        {
            if (item.activeInHierarchy && !item.GetComponent<CharacterCTRL>().CurrentHex.IsLogistics)
            {
                allDisabled = false;
                break;
            }
        }
        if (allDisabled&& GameStageManager.Instance.CurrGamePhase == GamePhase.Battling)
        {
            GameStageManager.Instance.NotifyTeamDefeated(this);
            
        }
    }

    public void UpdateTraitEffects()
    {
        List<CharacterCTRL> battlefieldCharacters = GetBattleFieldCharacter();
        Dictionary<Traits, int> traitCounts = TraitsEffectManager.Instance.CalculateTraitCounts(battlefieldCharacters);
        StringBuilder sb = new StringBuilder();
        int totalActivatedTraits = 0; // 激活的羁绊数量总和

        foreach (var trait in traitCounts.Keys)
        {
            int traitLevel = traitCounts[trait];
            sb.AppendLine($"{trait} 拥有 {traitLevel} 名角色激活");
            totalActivatedTraits += traitLevel;
            ApplyTraitEffect(trait, traitLevel, battlefieldCharacters, isEnemy);
        }
        sb.AppendLine($"激活的羁绊数量总和：{totalActivatedTraits}");
        CustomLogger.Log(this, sb.ToString());
        currTraits = traitCounts;
        if (!isEnemy)
        {
            TraitUIManager.Instance.UpdateTraitUI(traitCounts);
        }
        if (!currTraits.TryGetValue(Traits.Abydos, out int count) || count <= 0)
        {
            SpawnGrid.Instance.ResetDesertifiedTiles();
        }
        foreach (var item in totems)
        {
            item.SetActive(false);
        }
        UpdateLogisticsDummies(battlefieldCharacters);
    }
    public void AddTraitEffects()
    {
        List<CharacterCTRL> battlefieldCharacters = GetAllCharacter();
        Dictionary<Traits, int> traitCounts = TraitsEffectManager.Instance.CalculateTraitCounts(battlefieldCharacters);
        StringBuilder sb = new StringBuilder();
        int totalActivatedTraits = 0; // 激活的羁绊数量总和

        foreach (var trait in traitCounts.Keys)
        {
            int traitLevel = traitCounts[trait];
            sb.AppendLine($"{trait} 拥有 {traitLevel} 名角色激活");
            totalActivatedTraits += traitLevel;
            ApplyTraitEffect(trait, traitLevel, battlefieldCharacters, isEnemy);
        }
    }
    private void UpdateLogisticsDummies(List<CharacterCTRL> battlefieldCharacters)
    {
        foreach (var dummy in logisticsDummies.Values)
        {
            dummy.SetActive(false);
        }
        foreach (var item in battlefieldCharacters)
        {
            CharacterCTRL character = item.GetComponent<CharacterCTRL>();
            if (character.characterStats.logistics)
            {
                if (logisticsDummies.ContainsKey(character))
                {
                    logisticsDummies[character].GetComponent<CharacterCTRL>().CurrentHex.HardReserve(logisticsDummies[character].GetComponent<CharacterCTRL>());
                    logisticsDummies[character].SetActive(true);
                }
                else
                {
                    HexNode h = SpawnGrid.Instance.GetEmptyHex();
                    GameObject obj = Instantiate(ResourcePool.Instance.LogisticDummy, h.Position + new Vector3(0, 0.14f, 0), Quaternion.Euler(new Vector3(-90, 0, 0)));
                    CustomLogger.Log(this,"spawned dummy");
                    CharacterBars bar = ResourcePool.Instance.GetBar(h.Position).GetComponent<CharacterBars>();
                    childCharacters.Add(obj);
                    CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
                    ctrl.SetBarChild(bar);
                    ctrl.characterBars = bar;
                    CustomLogger.Log(this, $"get bar to {obj.name},bar parent = {ctrl},child = {ctrl.characterBars}");
                    bar.SetBarsParent(obj.transform);
                    logisticsDummies[character] = obj;
                    ctrl.CurrentHex = h;
                    h.OccupyingCharacter = ctrl;
                    h.Reserve(ctrl);
                }

            }
        }
        foreach (var item in SpawnGrid.Instance.hexNodes.Values)
        {
            if (item.OccupyingCharacter!=null&& !item.OccupyingCharacter.gameObject.activeInHierarchy)
            {
                item.HardRelease();
            }
        }
    }

    private void ApplyTraitEffect(Traits trait, int traitLevel, List<CharacterCTRL> battlefieldCharacters, bool isEnemy)
    {
        CustomLogger.Log(this, $"应用 {trait} 的效果，角色数量：{traitLevel}，是否为敌方：{isEnemy}");
        foreach (var character in battlefieldCharacters)
        {
            if (character.traitController.HasTrait(trait))
            {
                character.traitController.CreateObserverForTrait(trait);
            }
        }
    }
    public int GetActiveCharacter()
    {
        int i = 0;
        foreach (var item in childCharacters)
        {
            if (item.activeInHierarchy && !item.GetComponent<CharacterCTRL>().CurrentHex.IsLogistics)
            {
                i++;
            }
        }
        return i;
    }
    public List<CharacterCTRL> GetAllCharacter()
    {
        List<CharacterCTRL> L = new List<CharacterCTRL>();
        foreach (var item in childCharacters)
        {
            L.Add(item.GetComponent<CharacterCTRL>());
        }
        return L;
    }
    public List<CharacterCTRL> GetBattleFieldCharacter()
    {
        List < CharacterCTRL > L = new List<CharacterCTRL >();
        foreach (var item in childCharacters)
        {
            CharacterCTRL C = item.GetComponent<CharacterCTRL>();
            if (C.CurrentHex.IsBattlefield)
            {
                L.Add(item.GetComponent<CharacterCTRL>());
            }
        }
        return L;
    }
}
