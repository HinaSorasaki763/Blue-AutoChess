using GameEnum;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
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
    public TextMeshProUGUI currCharacterLimit;
    public GameObject BarParent;
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
    }
    public void Start()
    {
        CalculateGold();
        PressureManager.Instance.UpdateIndicater();
    }
    private IEnumerator ShowEndGamePopup(bool isEnemy)
    {
        yield return new WaitForSeconds(2f);
        CalculateGold();
        EndBattleModal.Instance.UpdateText(isEnemy);
        endGamePopup.SetActive(true);
        ChangeGamePhase(GamePhase.Preparing);
        
    }
    public void StartBattle()
    {
        if (CurrGamePhase == GamePhase.Battling) return;
        // 隨機選擇三個敵人波次並顯示選擇 UI
        //TODO: 根據玩家選擇決定波次，目前PVE不需要
        /*EnemySpawner.Instance.SelectRandomEnemyWaves();
        opponentSelectionUI.Show(EnemySpawner.Instance.selectedEnemyWaves);*/
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
        yield return new WaitForSeconds(1.5f);
        startBattle.Raise();
        startBattleFlag = true;
        AbydosManager.Instance.UpdateDesertifiedTiles();
    }
    private void OnContinueButtonClicked()
    {
        endGamePopup.SetActive(false);
        ResourcePool.Instance.enemy.ClearAllCharacter();
        DataStackManager.Instance.CheckDataStackRewards();
        foreach (var item in SpawnGrid.Instance.hexNodes.Values)
        {
            item.HardRelease();
        }
        foreach (var item in ResourcePool.Instance.ally.childCharacters)
        {
            item.SetActive(true);
            item.GetComponent<CharacterCTRL>().ResetToBeforeBattle();
            if (item.GetComponent<CharacterCTRL>().HexWhenBattleStart != null)
            {
                item.GetComponent<CharacterCTRL>().HexWhenBattleStart.HardReserve(item.GetComponent<CharacterCTRL>());
                item.transform.position = item.GetComponent<CharacterCTRL>().HexWhenBattleStart.Position + offset;
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
    }
    public void NotifyTeamDefeated(CharacterParent defeatedTeam)
    {
        currentRound++;
        if (defeatedTeam.isEnemy)
        {
            netWin++;
            WinStreak++;
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
        //
    }
    public void UpdateTexts()
    {
        currGamePhase.text = CurrGamePhase.ToString();
        if (CurrGamePhase == GamePhase.Battling)
        {
            currCharacterLimit.gameObject.SetActive(false);
        }
        else
        {
            currCharacterLimit.gameObject.SetActive(true);
            int count = 0;
            foreach (var item in ResourcePool.Instance.ally.childCharacters)
            {
                if (item.activeInHierarchy&&item.GetComponent<CharacterCTRL>().CurrentHex.IsBattlefield && !item.GetComponent<CharacterCTRL>().isObj)
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
                    item.effectCTRL.AddEffect(effect);
                }
                foreach (var item in Utility.GetAllBattlingCharacter(ResourcePool.Instance.enemy))
                {
                    Effect effect = EffectFactory.OverTimeEffect();
                    item.effectCTRL.AddEffect(effect);
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
        DamageStatisticsManager.Instance.ClearAll();
        enteringBattleCounter = 0;
        ChangeGamePhase(GamePhase.Preparing);
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
        if (winningTeam.childCharacters.Count>0)
        {
            do
            {
                int randomIndex = Random.Range(0, winningTeam.childCharacters.Count);
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
        allyParent.childCharacters.Clear();
        enemyParent.childCharacters.Clear();
        OnGameStageChanged?.Invoke(CurrentStage);
        SpawnGrid.Instance.ResetAll();
        SpawnGrid.Instance.RestorePreparationPositions();
    }
    private void CalculateGold()
    {
        int gold = GameController.Instance.GetGoldAmount();
        int streakBonus = CalculateStreakBonus()*2;
        int amount = streakBonus + CurrentStage*2 + 10;
        GameController.Instance.AddGold(amount);
        CustomLogger.Log(this, $"Gold: {gold}, Streak Bonus: {streakBonus},stagebouns = {CurrentStage + 3}, Total: {amount}");
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
            if(currentRound >= round[i])
            {
                additionalLimit++;
            }
        }

        return baseLimit + additionalLimit;
    }
}
