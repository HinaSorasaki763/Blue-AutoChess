using GameEnum;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 假設您使用 Unity 的 UI 系統

public class GameStageManager : MonoBehaviour
{
    public static GameStageManager Instance { get; private set; }
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    public GameObject endGamePopup;
    public Button continueButton;
    public int CurrentStage;
    public int PlayerHealth = 20;
    private int currentRound = 0;
    private int baseLimit = 3;
    private int netWin = 0;
    public GameEvent startBattle;
    public delegate void GameStageChanged(int newStage);
    public event GameStageChanged OnGameStageChanged;
    public GamePhase CurrGamePhase = GamePhase.Preparing;
    public EnemySpawner Spawner;
    public OpponentSelectionUI opponentSelectionUI;
    public TextMeshProUGUI currGamePhase;
    public int WinStreak { get; private set; } = 0; // 連勝次數
    public int LoseStreak { get; private set; } = 0; // 連敗次數
    public int MaxInterest = 5; // 利息上限

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
    private IEnumerator ShowEndGamePopup()
    {
        yield return new WaitForSeconds(2f);
        CalculateGold();
        EndBattleModal.Instance.UpdateText();
        endGamePopup.SetActive(true);
        ChangeGamePhase(GamePhase.Preparing);
    }
    public void StartBattle()
    {
        // 隨機選擇三個敵人波次並顯示選擇 UI
        EnemySpawner.Instance.SelectRandomEnemyWaves();
        opponentSelectionUI.Show(EnemySpawner.Instance.selectedEnemyWaves);
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
        EndBattleModal.Instance.lastPressure = PressureManager.Instance.GetPressure();
        EndBattleModal.Instance.lastData = DataStackManager.Instance.GetData();
        SpawnGrid.Instance.SavePreparationPositions();
    }

    public IEnumerator StartBattleCorutine()
    {
        yield return new WaitForSeconds(1.5f);
        startBattle.Raise();
    }
    private void OnContinueButtonClicked()
    {
        endGamePopup.SetActive(false);
        AdvanceStage();
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

        StartCoroutine(ShowEndGamePopup());
    }

    public void Update()
    {
        currGamePhase.text = CurrGamePhase.ToString();
    }
    public void ChangeGamePhase(GamePhase gamePhase)
    {
        CurrGamePhase = gamePhase;
        CustomLogger.Log(this, $"change phase to {gamePhase}");
    }
    private void OnVictory(CharacterParent winningTeam, CharacterParent defeatedTeam)
    {
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
                item.GetComponent<CharacterCTRL>().traitController.Win();
                item.transform.rotation = Quaternion.identity;
            }
        }
        GameObject randomItem;
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

    private int CalculateDamageTaken(int stage)
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
        int interest = Mathf.Min(gold / 10, MaxInterest);
        int streakBonus = CalculateStreakBonus();
        int amount = interest + streakBonus + CurrentStage + 3;
        GameController.Instance.AddGold(amount);
        CustomLogger.Log(this, $"Gold: {gold}, Interest: {interest}, Streak Bonus: {streakBonus},stagebouns = {CurrentStage + 3}, Total: {amount}");
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
        int additionalLimit = (currentRound - 1) / 2;
        return baseLimit + additionalLimit;
    }
}
