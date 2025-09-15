using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 假設您使用 Unity 的 UI 系統

public class GameStageManager : MonoBehaviour
{
    private readonly Vector3 offset = new Vector3(0, 0.14f, 0);
    public static GameStageManager Instance { get; private set; }
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    public GameObject endGamePopup;
    public GameObject SupplyPopup;
    public Button continueButton;
    public int CurrentStage;
    public int PlayerHealth = 20;
    public int currentRound = 0;
    private int baseLimit = 2;
    private int netWin = 0;
    public float enteringBattleCounter = 0;
    private bool overTimeFlag;
    private bool startBattleFlag;
    public GameEvent startBattle;
    public delegate void GameStageChanged(int newStage);
    public event GameStageChanged OnGameStageChanged;
    public GamePhase CurrGamePhase = GamePhase.Preparing;
    public EnemySpawner Spawner;
    public OpponentSelectionUI opponentSelectionUI;
    public TextMeshProUGUI currGamePhase;
    public GameObject characterLimitParent;
    public TextMeshProUGUI currCharacterLimit;
    public GameObject BarParent;
    private Dictionary<int, Action> stageRewardMapping;
    public RewardPopup rewardPopup;
    readonly int OvertimeThreshold = 30;

    public int WinStreak { get; private set; } = 0; // 連勝次數
    public int LoseStreak { get; private set; } = 0; // 連敗次數

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        continueButton.onClick.AddListener(OnContinueButtonClicked);
        endGamePopup.SetActive(false);
        InitializeFloorRewardMapping();
    }

    public void Start()
    {
        CalculateGold();
        PressureManager.Instance.UpdateIndicater();
    }
    private IEnumerator ShowEndGamePopup(bool isEnemy)
    {
        ChangeGamePhase(GamePhase.Preparing);
        yield return new WaitForSeconds(2f);
        CalculateGold();
        EndBattleModal.Instance.UpdateText(isEnemy);
        endGamePopup.SetActive(true);

    }
    public void StartBattle()
    {
        if (CurrGamePhase == GamePhase.Battling) return;
        // 隨機選擇三個敵人波次並顯示選擇 UI
        //TODO: 根據玩家選擇決定波次，目前PVE不需要
        /*EnemySpawner.Instance.SelectRandomEnemyWaves();
        opponentSelectionUI.Show(EnemySpawner.Instance.selectedEnemyWaves);*/
        if (ResourcePool.Instance.enemy.childCharacters.Count != 0)
        {
            StartCoroutine(StartBattleCorutine());
            return;
        }
        GameController.Instance.SerinaEnhancedSkill_CritCount = 0;
        PVE_EnemySpawner.Instance.SpawnEnemiesNextStage();
        opponentSelectionUI.Hide();
        StartCoroutine(StartBattleCorutine());
        EndBattleModal.Instance.lastPressure = PressureManager.Instance.GetPressure(true);
        EndBattleModal.Instance.lastData = DataStackManager.Instance.GetData();
        SpawnGrid.Instance.SavePreparationPositions();
    }
    public void SimulateAdvanceRound()
    {
        currentRound++;
        CheckGameStageReward();
    }
    public void CheckGameStageReward()
    {
        if (stageRewardMapping.ContainsKey(currentRound))
        {
            stageRewardMapping[currentRound]?.Invoke();
        }
    }
    public int GetRound()
    {
        return currentRound;
    }
    public void OnStartBattleConfirmed()
    {
        CustomLogger.Log(this, "OnStartBattleConfirmed");
        int selectedIndex = opponentSelectionUI.GetSelectedOpponentIndex();
        if (selectedIndex == -1)
        {
            Debug.LogError("No opponent selected.");
            return;
        }

        // 根據選擇的對手波次生成敵人
        EnemySpawner.Instance.PlayerSelectWave(selectedIndex);
        opponentSelectionUI.Hide();
        StartCoroutine(StartBattleCorutine());
        EndBattleModal.Instance.lastPressure = PressureManager.Instance.GetPressure(true);
        EndBattleModal.Instance.lastData = DataStackManager.Instance.GetData();
        SpawnGrid.Instance.SavePreparationPositions();
    }

    public IEnumerator StartBattleCorutine()
    {
        foreach (var item in ResourcePool.Instance.ally.childCharacters)
        {
            CharacterCTRL c = item.GetComponent<CharacterCTRL>();
            c.enterBattle = false;
            if (c.CurrentHex.IsBattlefield)
            {
                c.HexWhenBattleStart = c.CurrentHex;
            }
            else
            {
                c.HexWhenBattleStart = null;
            }
        }
        ResourcePool.Instance.enemy.Trigger109();
        yield return new WaitForSeconds(1.5f);
        ResourcePool.Instance.enemy.TriggerOnBoard();
        
        startBattle.Raise();
        startBattleFlag = true;
        AbydosManager.Instance.UpdateDesertifiedTiles();
    }
    private void OnContinueButtonClicked()
    {
        endGamePopup.SetActive(false);
        ResourcePool.Instance.enemy.ClearAllCharacter();
        DataStackManager.Instance.CheckDataStackRewards();
        if (SelectedAugments.Instance.CheckAugmetExist(103))
        {
            IEquipment equipment = Utility.GetSpecificEquipment(29);
            EquipmentManager.Instance.AddEquipmentItem(equipment);
        }
        CheckGameStageReward();
        foreach (var item in SpawnGrid.Instance.hexNodes.Values)
        {
            item.HardRelease();
        }
        foreach (var item in ResourcePool.Instance.ally.childCharacters)
        {
            CharacterCTRL c = item.GetComponent<CharacterCTRL>();
            item.SetActive(true);
            c.ResetToBeforeBattle();
            if (!c.characterStats.logistics)
            {
                if (c.HexWhenBattleStart != null)
                {
                    c.HexWhenBattleStart.HardReserve(c);
                    item.transform.position = c.HexWhenBattleStart.Position + offset;
                }
            }
            else
            {

            }

        }

        AdvanceStage();
    }
    public void ResetBattleData()
    {
        CurrGamePhase = GamePhase.Preparing;
        PlayerHealth = 20;
        currentRound = 0;
        netWin = 0;
        CurrentStage = 0;
        WinStreak = 0;
        LoseStreak = 0;
        startBattleFlag = false;
        overTimeFlag = false;
        enteringBattleCounter = 0;
        baseLimit = 2;
        PressureManager.Instance.ResetPressure();
        PressureManager.Instance.UpdateIndicater();
        DataStackManager.Instance.ResetData();
        DataStackManager.Instance.UpdateIndicator();
        ResourcePool.Instance.ally.ClearAllCharacter();
        ResourcePool.Instance.enemy.ClearAllCharacter();
        SpawnGrid.Instance.ResetAll();
        GameController.Instance.SetGold(10);
        Shop.Instance.GoldLessRefresh();
        DamageStatisticsManager.Instance.ClearAll();

        Dictionary<Traits, int> traitCounts = new Dictionary<Traits, int>();
        TraitUIManager.Instance.UpdateTraitUI(traitCounts);
    }
    public void NotifyTeamDefeated(CharacterParent defeatedTeam)
    {

        currentRound++;
        if (defeatedTeam.isEnemy)
        {
            netWin++;
            WinStreak++;
            if (SelectedAugments.Instance.CheckAugmetExist(102))
            {
                foreach (var item in ResourcePool.Instance.ally.GetBattleFieldCharacter())
                {
                    if (item.traitController.GetAcademy() == Traits.Abydos)
                    {
                        AbydosManager.Instance.Augment102_LivedCount++;
                    }

                }
            }
            PressureManager.Instance.HandleAugment109();
            if (SelectedAugments.Instance.CheckAugmetExist(116))
            {
                DataStackManager.Instance.floorRewardMapping[LoseStreak * 100].Invoke();
            }
            LoseStreak = 0; // 重置連敗
            OnVictory(allyParent, defeatedTeam);
        }
        else
        {
            netWin--;
            LoseStreak++;
            WinStreak = 0; // 重置連勝
            OnVictory(enemyParent, defeatedTeam);
        }

        StartCoroutine(ShowEndGamePopup(defeatedTeam.isEnemy));
    }

    public void GainSupply()
    {

    }
    public void UpdateTexts()
    {
        currGamePhase.text = CurrGamePhase.ToString();
        if (CurrGamePhase == GamePhase.Battling)
        {
            characterLimitParent.SetActive(false);
        }
        else
        {
            characterLimitParent.SetActive(true);
            int count = 0;
            foreach (var item in ResourcePool.Instance.ally.GetBattleFieldCharacter())
            {
                if (item.characterStats.CanPutBack)
                {
                    count++;
                }
            }
            currCharacterLimit.text = $"( {count} / {GetCharacterLimit()})";
        }

    }
    public void Update()
    {
        UpdateTexts();
        if (startBattleFlag)
        {
            enteringBattleCounter += Time.deltaTime;
            if (enteringBattleCounter >= OvertimeThreshold && !overTimeFlag)
            {
                overTimeFlag = true;
                foreach (var item in Utility.GetAllBattlingCharacter(ResourcePool.Instance.ally))
                {
                    Effect effect = EffectFactory.OverTimeEffect();
                    item.effectCTRL.AddEffect(effect, item);
                }
                foreach (var item in Utility.GetAllBattlingCharacter(ResourcePool.Instance.enemy))
                {
                    Effect effect = EffectFactory.OverTimeEffect();
                    item.effectCTRL.AddEffect(effect, item);
                }
            }
        }
    }
    public void ChangeGamePhase(GamePhase gamePhase)
    {
        CurrGamePhase = gamePhase;
        CustomLogger.Log(this, $"change phase to {gamePhase}");
    }
    private void OnVictory(CharacterParent winningTeam, CharacterParent defeatedTeam)
    {
        startBattleFlag = false;
        DamageStatisticsManager.Instance.Reset125();
        if (SelectedAugments.Instance.CheckAugmetExist(125))
        {
            CharacterCTRL c = DamageStatisticsManager.Instance.GetAugment125Character(true);
            if (c == null)
            {
                c = DamageStatisticsManager.Instance.GetTopDamageDealer(true);
            }
            c.isAugment125Reinforced = true;
        }
        DamageStatisticsManager.Instance.ClearAll();
        BugReportLogger.Instance.EndBattle();
        enteringBattleCounter = 0;
        Debug.Log(winningTeam.isEnemy ? "敵方勝利！" : "友方勝利！");
        EndBattleModal.Instance.currData = DataStackManager.Instance.GetData();
        // 判斷玩家是否陣亡
        if (winningTeam.isEnemy)
        {
            int damage = CalculateDamageTaken(CurrentStage);
            PlayerHealth -= damage;

            if (PlayerHealth <= 0)
            {
                Debug.Log("玩家已經陣亡！遊戲結束");
                // 遊戲結束的邏輯
            }
        }
        EndBattleModal.Instance.UpdateHealth(PlayerHealth);

        foreach (var item in winningTeam.childCharacters)
        {
            if (item.activeInHierarchy)
            {
                item.GetComponent<CharacterCTRL>().traitController.OnBattleEnd(true);
                item.transform.rotation = Quaternion.identity;
            }
        }
        foreach (var item in defeatedTeam.childCharacters)
        {
            if (item.activeInHierarchy)
            {
                item.GetComponent<CharacterCTRL>().traitController.OnBattleEnd(false);
                item.transform.rotation = Quaternion.identity;
            }
        }
        ResourcePool.Instance.enemy.ClearAllCharacter();
        GameObject randomItem;
        if (winningTeam.childCharacters.Count > 0)
        {
            do
            {
                int randomIndex = UnityEngine.Random.Range(0, winningTeam.childCharacters.Count);
                randomItem = winningTeam.childCharacters[randomIndex];
            } while (!randomItem.activeInHierarchy);
            CharacterCTRL characterCtrl = randomItem.GetComponent<CharacterCTRL>();
            if (characterCtrl != null)
            {
                characterCtrl.AudioManager.PlayOnVictory();
            }
        }


    }

    public int CalculateDamageTaken(int stage)
    {
        int[] damageArray = { 2, 2, 4, 4, 6, 6, 8, 8, 10, 10, 10, 10, 10, 10, 10, 10 };
        return damageArray[Mathf.Min(stage, damageArray.Length - 1)];
    }

    public void AdvanceStage()
    {

        CurrentStage++;
        enemyParent.childCharacters.Clear();
        OnGameStageChanged?.Invoke(CurrentStage);
        SpawnGrid.Instance.ResetAll();
        SpawnGrid.Instance.RestorePreparationPositions();
        Shop.Instance.GoldLessRefresh();
    }
    private void CalculateGold()
    {
        int gold = GameController.Instance.GetGoldAmount();
        int streakBonus = CalculateStreakBonus() * 2;
        int interest = GetInterest(gold);
        if (SelectedAugments.Instance.CheckAugmetExist(126)) interest = 0;
        int amount = streakBonus + CurrentStage * 2 + 10 + interest;
        GameController.Instance.AddGold(amount);
        CustomLogger.Log(this, $"Gold: {gold}, Streak Bonus: {streakBonus},stagebouns = {CurrentStage + 3}, Total: {amount}");
    }
    public int GetInterest(int gold)
    {
        int max = 5;
        if (SelectedAugments.Instance.CheckAugmetExist(116))
        {
            max = 7;
        }
        return Mathf.Min(gold / 10, max);
    }
    private int CalculateStreakBonus()
    {
        int streakCount = Mathf.Max(WinStreak, LoseStreak);

        if (streakCount == 0) return 0;

        if (streakCount == 1 || streakCount == 2) return 1;
        if (streakCount == 3 || streakCount == 4) return 2;
        return 3;
    }
    public int GetCharacterLimit()
    {
        int[] round = { 1, 3, 5, 7, 10, 13, 16, 18 };
        int additionalLimit = 0;
        int rounds = currentRound - 1;

        for (int i = 0; i < round.Length; i++)
        {
            if (currentRound >= round[i])
            {
                additionalLimit++;
            }
        }

        return baseLimit + additionalLimit;
    }
    private void InitializeFloorRewardMapping()
    {
        stageRewardMapping = new Dictionary<int, Action>
    {
        {
            0, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            2, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 1);
            }
        },
        {
            4, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(2, 1, 1, 10);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            6, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(3, 2, 2, 16);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            9, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 22);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            12, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4, 2, 2, 22);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            15, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(4,2,2,22);
                rewardPopup.AddRewards(context, 2);
            }
        },
        {
            17, () =>
            {
                RewardContext context = Utility.BuildMixedRewards(0,0,0,40);
                rewardPopup.AddRewards(context, 1);
            }
        }
    };
    }
}
