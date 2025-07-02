using GameEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CharacterParent : MonoBehaviour
{
    public List<GameObject> childCharacters = new();
    public GameEvent startBattle;
    public bool isEnemy;
    public Dictionary<Traits, int> currTraits = new Dictionary<Traits, int>();
    public bool IsBattling = false;
    private List<GameObject> totems = new List<GameObject>();
    private Dictionary<CharacterCTRL, GameObject> logisticsDummies = new Dictionary<CharacterCTRL, GameObject>();
    public HashSet<int> enhancedSkillCharacters = new HashSet<int>();
    public bool Shiroko_Terror_Postponed = false;
    private bool Shiroko_Terror_Tempmark = false;
    public List<String> BattleingCharacterNames = new List<string>();
    public void Start()
    {

    }

    public void OnStartBattle()
    {
        childCharacters.RemoveAll(c => !c.activeInHierarchy);
        foreach (var item in childCharacters)
        {
            item.GetComponent<CharacterCTRL>().RecalculateStats();
        }
        IsBattling = true;
        foreach (var character in childCharacters)
        {
            CharacterCTRL c = character.GetComponent<CharacterCTRL>();
            c.TriggerManualUpdate();
        }
        foreach (var character in childCharacters)
        {
            CharacterCTRL c = character.GetComponent<CharacterCTRL>();
            if (c.CurrentHex.IsBattlefield || c.CurrentHex.IsLogistics)
            {
                c.EnterBattle();
                DamageStatisticsManager.Instance.RegisterCharacter(c);

            }
        }
        BugReportLogger.Instance.StartBattle();
        GameStageManager.Instance.ChangeGamePhase(GamePhase.Battling);
    }
    public int CheckAlliesOnBoard()
    {
        HashSet<int> uniqueCharacterIds = new HashSet<int>();

        foreach (var item in childCharacters)
        {
            CharacterCTRL c = item.GetComponent<CharacterCTRL>();
            if (c.CurrentHex.IsBattlefield && c.isAlive)
            {
                uniqueCharacterIds.Add(c.characterStats.CharacterId);
            }
        }

        foreach (var item in logisticsDummies)
        {
            CharacterCTRL c = item.Key.GetComponent<CharacterCTRL>();
            if (c.isAlive)
            {
                uniqueCharacterIds.Add(c.characterStats.CharacterId);
            }
        }

        return uniqueCharacterIds.Count;
    }
    public void AddEnhancedSkill(int index)
    {
        enhancedSkillCharacters.Add(index);
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

    public void TriggerManualUpdate()
    {
        foreach (var item in childCharacters)
        {
            item.GetComponent<CharacterCTRL>().traitController.TriggerManualUpdate();
            item.GetComponent<CharacterCTRL>().equipmentManager.TriggerManualUpdate();
            foreach (var observer in item.GetComponent<CharacterCTRL>().observers)
            {
                observer.ManualUpdate(item.GetComponent<CharacterCTRL>());
            }
        }

    }
    public void FroceRefrshStats()
    {
        foreach (var item in childCharacters)
        {
            item.GetComponent<CharacterCTRL>().RecalculateStats();
        }
    }
    private bool CombineCharacters(List<CharacterCTRL> charactersToCombine)
    {

        if (charactersToCombine == null || charactersToCombine.Count < 3)
        {
            CustomLogger.LogWarning(this, "需要至少三個角色來進行合成");
            return false;
        }

        // 找到最強的角色作為主要角色
        CharacterCTRL mainCharacter = FindStrongestCharacter(charactersToCombine);
        if (mainCharacter.characterStats.CharacterId == 41) return false;
        if (mainCharacter == null)
        {
            CustomLogger.LogWarning(this, "未能找到主要角色");
            return false;
        }
        var extraList = new List<StatsContainer>();
        foreach (var item in charactersToCombine)
        {
            extraList.Add(item.ExtraPernamentStats);
        }
        extraList.Sort((a, b) => b.SumAllStats().CompareTo(a.SumAllStats()));
        mainCharacter.star++;
        if (mainCharacter.star > 3)
        {
            mainCharacter.star = 3;
        }
        foreach (var character in charactersToCombine)
        {
            if (character == mainCharacter) continue;

            GameObject characterGameObject = character.gameObject;
            character.equipmentManager.RemoveAllItem();
            childCharacters.Remove(characterGameObject);
            Destroy(characterGameObject);
        }
        mainCharacter.ExtraPernamentStats = extraList[0].Clone();
        mainCharacter.RecalculateStats();
        mainCharacter.GetSkillContext();
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
            foreach (var starGroup in group.Value)
            {
                allCharactersWithSameId.AddRange(starGroup.Value);
            }
            CharacterCTRL strongestCharacter = FindStrongestCharacter(allCharactersWithSameId);
            foreach (var character in allCharactersWithSameId)
            {
                character.characterBars.SetStrongestMark(character == strongestCharacter);
            }
        }
    }
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
        GetEmptyObserver(obj.GetComponent<CharacterCTRL>());
        if (!isEnemy)
        {
            CheckAndCombineCharacters();
        }
        AddTraitEffects();
    }
    
    public void GetEmptyObserver(CharacterCTRL character)
    {
        foreach (var item in character.traitController.GetCurrentTraits())
        {
            character.traitController.CreateObserverForTrait(item, 0);
            CharacterObserverBase c = character.traitController.GetObserverForTrait(item);
            c.DeactivateTrait();
        }
    }
    public void Update()
    {
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Preparing) return;
        bool allDisabled = true;
        BattleingCharacterNames.Clear();
        foreach (var item in GetBattleFieldCharacter())
        {
            if (item.isAlive && !item.CurrentHex.IsLogistics && !item.GetComponent<CharacterCTRL>().Undying)
            {
                allDisabled = false;
                BattleingCharacterNames.Add(item.name);
            }
        }
        if (allDisabled && GameStageManager.Instance.CurrGamePhase == GamePhase.Battling)
        {
            if (!Shiroko_Terror_Postponed)
            {
                GameStageManager.Instance.CurrGamePhase = GamePhase.Preparing;
                foreach (var item in GetBattleFieldCharacter())
                {
                    item.customAnimator.ForceIdle();
                    if (item.droneCTRL != null)
                    {
                        item.droneCTRL.gameObject.SetActive(false);
                    }
                    item.enterBattle = false;
                }
            }
            if (Shiroko_Terror_Postponed && !isEnemy)
            {
                if (Shiroko_Terror_Tempmark) return;
                Shiroko_Terror_Tempmark = true;
                CharacterCTRL c = Utility.GetSpecificCharacterByIndex(GetAllCharacter(), 22);
                ResourcePool.Instance.ally.childCharacters.Remove(c.gameObject);
                Destroy(c.gameObject);
                BenchManager.Instance.AddToBench(Utility.GetSpecificCharacterToSpawn(31));
                return;
            }
            foreach (var item in childCharacters)
            {
                item.GetComponent<CharacterCTRL>().ResetToBeforeBattle();
            }
            GameStageManager.Instance.NotifyTeamDefeated(this);//TODO:黑子的邏輯需要再修正
        }
    }
    public void ContinueBattleRoutine()
    {
        GameStageManager.Instance.NotifyTeamDefeated(this);
    }
    public int GetSpecificTrait(Traits traits)
    {
        UpdateTraitEffects();
        if (currTraits.ContainsKey(traits))
        {
            return currTraits[traits];
        }
        return 0;
    }
    public void UpdateTraitEffects()
    {
        List<CharacterCTRL> battlefieldCharacters = GetBattleFieldCharacter();
        Dictionary<Traits, int> traitCounts = TraitsEffectManager.Instance.CalculateTraitCounts(battlefieldCharacters);
        StringBuilder sb = new StringBuilder();
        int totalActivatedTraits = 0; // 激活的羁绊数量总和

        foreach (var trait in traitCounts.Keys)
        {
            int characterCount = traitCounts[trait];
            sb.AppendLine($"{trait} 拥有 {characterCount} 名角色激活");
            totalActivatedTraits += characterCount;
            ApplyTraitEffect(trait, characterCount, battlefieldCharacters, isEnemy);
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
        if (!isEnemy && GameStageManager.Instance.CurrGamePhase == GamePhase.Preparing)
        {
            UpdateLogisticsDummies(battlefieldCharacters);
        }
        if (SelectedAugments.Instance.CheckAugmetExist(124) && !SelectedAugments.Instance.TriggeredIndex.Contains(124))
        {
            int srtCount = 0;
            foreach (var item in childCharacters)
            {
                CharacterCTRL c = item.GetComponent<CharacterCTRL>();
                if (c.traitController.GetAcademy() == Traits.SRT && c.star == 3)
                {
                    srtCount++;
                }
            }
            if (srtCount >=0)
            {
                SelectedAugments.Instance.TriggerAugment(124);
            }

        }
    }
    public void AddTraitEffects()
    {
        List<CharacterCTRL> battlefieldCharacters = GetAllCharacter();
        Dictionary<Traits, int> traitCounts = TraitsEffectManager.Instance.CalculateTraitCounts(battlefieldCharacters);
        StringBuilder sb = new StringBuilder();
        int totalActivatedTraits = 0; // 激活的羁绊数量总和

        foreach (var trait in traitCounts.Keys)
        {
            int characterCount = traitCounts[trait];
            sb.AppendLine($"{trait} 拥有 {characterCount} 名角色激活");
            totalActivatedTraits += characterCount;
            ApplyTraitEffect(trait, characterCount, battlefieldCharacters, isEnemy);
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
                    StaticObject staticObj = logisticsDummies[character].gameObject.GetComponent<StaticObject>();
                    staticObj.RefreshDummy(character);
                }
                else
                {
                    HexNode h = SpawnGrid.Instance.GetEmptyHex();
                    GameObject obj = Instantiate(ResourcePool.Instance.LogisticDummy,h.Position + new Vector3(0, 0.23f, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
                    CustomLogger.Log(this, "spawned dummy");
                    obj.transform.SetParent(ResourcePool.Instance.ally.transform);
                    CharacterBars bar = ResourcePool.Instance.GetBar(h.Position).GetComponent<CharacterBars>();
                    childCharacters.Add(obj);
                    obj.name = $"dummy ({character.name})";
                    CharacterCTRL ctrl = obj.GetComponent<CharacterCTRL>();
                    StaticObject staticObj = obj.GetComponent<StaticObject>();
                    staticObj.parent = character;
                    ctrl.SetBarChild(bar);
                    ctrl.characterBars = bar;
                    bar.SetBarsParent(obj.transform);
                    staticObj.RefreshDummy(character);
                    logisticsDummies[character] = obj;
                    ctrl.CurrentHex = h;
                    ctrl.IsAlly = true;
                    h.OccupyingCharacter = ctrl;
                    h.Reserve(ctrl);
                }

            }
        }
        foreach (var item in SpawnGrid.Instance.hexNodes.Values)
        {
            if (item.OccupyingCharacter != null && !item.OccupyingCharacter.gameObject.activeInHierarchy)
            {
                item.HardRelease();
            }
        }
    }

    private void ApplyTraitEffect(Traits trait, int characterCount, List<CharacterCTRL> battlefieldCharacters, bool isEnemy)
    {
        TraitDescriptionData data = null;
        foreach (var item in TraitDescriptions.Instance.traitDescriptionDatabase.traitDescriptions)
        {
            if (item.trait == trait)
            {
                data = item;
            }
        }

        if (data == null)
            return;
        var sortedThresholds = data.thresholds.OrderBy(t => t.requiredCount).ToList();
        TraitThreshold matchedThreshold = null;
        foreach (var threshold in sortedThresholds)
        {
            if (characterCount >= threshold.requiredCount)
            {
                // traitLevel 符合這個門檻，就暫存下來
                matchedThreshold = threshold;
            }
            else
            {
                break;
            }
        }

        if (matchedThreshold == null)
        {
            foreach (var character in battlefieldCharacters)
            {
                if (character.traitController.HasTrait(trait))
                {
                    character.traitController.CreateObserverForTrait(trait, 0);
                    CharacterObserverBase c = character.traitController.GetObserverForTrait(trait);
                    c.DeactivateTrait();
                }
            }
            CustomLogger.Log(this, $"Trait {trait} 未達第一門檻，角色數={characterCount}，不啟動任何效果。");

            return;
        }
        CustomLogger.Log(this,
            $"Trait {trait} 達到門檻 {matchedThreshold.requiredCount}，" +
            $"角色數={characterCount}，啟動效果值={matchedThreshold.effectValue}。");

        foreach (var character in battlefieldCharacters)
        {
            if (character.traitController.HasTrait(trait))
            {
                // 這邊視你想怎麼傳遞 effectValue 與 observer
                character.traitController.CreateObserverForTrait(trait, matchedThreshold.level);
                CharacterObserverBase c = character.traitController.GetObserverForTrait(trait);
                c.ActivateTrait();
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

        List<CharacterCTRL> battlefieldCharacters = new List<CharacterCTRL>();
        foreach (var item in childCharacters)
        {
            if (!item.activeInHierarchy || item.GetComponent<CharacterCTRL>().characterStats.CharacterId == 41) continue;
            CharacterCTRL character = item.GetComponent<CharacterCTRL>();
            if (character.CurrentHex.IsBattlefield)
            {
                battlefieldCharacters.Add(character);
            }
        }
        return battlefieldCharacters;
    }
    public CharacterCTRL GetRandomCharacter(bool needLogistic)
    {
        List<CharacterCTRL> characters = GetBattleFieldCharacter();
        if (!needLogistic)
        {
            characters.RemoveAll(x => x.characterStats.logistics ==true);
        }

        Utility.SetRandKey();
        if (characters == null || characters.Count == 0)
        {
            CustomLogger.LogWarning(this, "No characters available on battlefield.");
            return null;
        }
        int randomIndex = UnityEngine.Random.Range(0, characters.Count);
        return characters[randomIndex];
    }

    public void ClearAllCharacter(GameObject exception = null)
    {

        logisticsDummies.Clear();
        for (int i = childCharacters.Count - 1; i >= 0; i--)
        {
            GameObject current = childCharacters[i];
            if (current == exception)
                continue;

            CharacterCTRL ctrl = current.GetComponent<CharacterCTRL>();
            ctrl.characterBars.ResetBars();
            ctrl.characterBars.gameObject.SetActive(false);
            current.SetActive(false);
            Destroy(current);
            childCharacters.RemoveAt(i);
        }

    }

}
