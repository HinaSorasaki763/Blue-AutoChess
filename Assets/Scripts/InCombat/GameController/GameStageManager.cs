using GameEnum;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // ���]�z�ϥ� Unity �� UI �t��

public class GameStageManager : MonoBehaviour
{
    public static GameStageManager Instance { get; private set; }
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    public GameObject endGamePopup; // �u�� UI
    public Button continueButton; // �u���W�����~�򡨫��s
    public int CurrentStage;
    public int PlayerHealth = 20; // ���a��l��q
    private int netWin = 0;
    public GameEvent startBattle;
    public delegate void GameStageChanged(int newStage);
    public event GameStageChanged OnGameStageChanged;
    public GamePhase CurrGamePhase = GamePhase.Preparing;
    public EnemySpawner Spawner;
    public OpponentSelectionUI opponentSelectionUI;
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
        EndBattleModal.Instance.UpdateText();
        endGamePopup.SetActive(true);
        CurrGamePhase = GamePhase.Preparing;
    }
    public void StartBattle()
    {
        // �H����ܤT�ӼĤH�i������ܿ�� UI
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

        // �ھڿ�ܪ����i���ͦ��ĤH
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
        if (defeatedTeam.isEnemy)
        {
            netWin++;
            OnVictory(allyParent, defeatedTeam);
        }
        else
        {
            netWin--;
            OnVictory(enemyParent, defeatedTeam);
        }

        StartCoroutine(ShowEndGamePopup());
    }
    public void Update()
    {

    }
    private void OnVictory(CharacterParent winningTeam, CharacterParent defeatedTeam)
    {
        CurrGamePhase = GamePhase.Preparing;
        Debug.Log(winningTeam.isEnemy ? "�Ĥ�ӧQ�I" : "�ͤ�ӧQ�I");
        EndBattleModal.Instance.currData = DataStackManager.Instance.GetData();
        // �P�_���a�O�_�}�`
        if (winningTeam.isEnemy)
        {
            int damage = CalculateDamageTaken(CurrentStage);
            PlayerHealth -= damage;

            if (PlayerHealth <= 0)
            {
                Debug.Log("���a�w�g�}�`�I�C������");
                // �C���������޿�
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
        // �ھڶ��q�p�⦩��q
        int[] damageArray = { 2, 2, 4, 4, 6, 6, 8, 8, 10, 10, 10, 10, 10, 10 , 10, 10};
        return damageArray[Mathf.Min(stage, damageArray.Length - 1)];
    }

    public void AdvanceStage()
    {
        CurrentStage++;
        allyParent.childCharacters.Clear();
        enemyParent.childCharacters.Clear();
        OnGameStageChanged?.Invoke(CurrentStage);
        SpawnGrid.Instance.ResetAll();
        //Spawner.SpawnEnemiesNextStage();
        SpawnGrid.Instance.RestorePreparationPositions();
    }
}
