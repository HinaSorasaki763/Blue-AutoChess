using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentSelectionUI : MonoBehaviour
{
    public GameObject panel; // 整個 UI 的面板
    public Button startBattleButton; // 開始戰鬥按鈕
    public List<GameObject> opponentPanels; // 三個長條的 UI
    public List<List<Image>> opponentImages = new List<List<Image>>();
    public List<Image> opponentImages1;
    public List<Image> opponentImages2;
    public List<Image> opponentImages3;
    public List<Button> opponentButtons; // 每個長條中的選擇按鈕
    public List<string> opponentNames = new();
    public List<TextMeshProUGUI> selectionIndicators; // 指示目前被選取的對手
    public List<TextMeshProUGUI> characterCountTexts; // 顯示每個對手的角色總數

    private EnemySpawner enemySpawner;
    private int selectedOpponentIndex = -1;
    FirebaseAuthManager authManager;
    void Start()
    {
        authManager = FindFirstObjectByType<FirebaseAuthManager>();
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        panel.SetActive(false); // 一開始隱藏選擇 UI
        startBattleButton.interactable = false;

        // 初始化 opponentImages 的結構
        opponentImages.Add(opponentImages1);
        opponentImages.Add(opponentImages2);
        opponentImages.Add(opponentImages3);

        // 初始化選擇指示器
        UpdateSelectionIndicators(-1);
    }

    public void Show(List<DocumentSnapshot> opponents)
    {
        panel.SetActive(true);
        opponentNames.Clear();
        foreach (var doc in opponents)
        {
            var data = doc.ToDictionary();
            if (data.TryGetValue("playerId", out object name) && (name is string enemyName))
            {
                opponentNames.Add(enemyName);
                continue;
            }
            else opponentNames.Add("Unknown");
        }

        for (int i = 0; i < opponentPanels.Count; i++)
        {
            if (i < opponents.Count)
            {
                opponentPanels[i].SetActive(true);

                var data = opponents[i].ToDictionary();

                // 嘗試讀 slots
                if (!data.TryGetValue("slots", out object slotsObj) || !(slotsObj is List<object> slotsList))
                {
                    Debug.LogError("Opponent doc has no valid slots.");
                    opponentPanels[i].SetActive(false);
                    continue;
                }

                // 顯示角色圖片
                for (int j = 0; j < opponentImages[i].Count; j++)
                {
                    if (j < slotsList.Count)
                    {
                        var slotDict = slotsList[j] as Dictionary<string, object>;
                        if (slotDict == null) continue;

                        int charId = Convert.ToInt32(slotDict["CharacterID"]);
                        Character characterData = ResourcePool.Instance.GetCharacterByID(charId);

                        if (characterData != null)
                            opponentImages[i][j].sprite = characterData.Sprite;
                        else
                            opponentImages[i][j].sprite = null;
                    }
                    else
                    {
                        opponentImages[i][j].sprite = null;
                    }
                }
                int index = i;
                opponentNames.Add(opponentNames[i]);
                opponentButtons[i].onClick.RemoveAllListeners();
                opponentButtons[i].onClick.AddListener(() => SelectOpponent(index));
            }
            else
            {
                opponentPanels[i].SetActive(false);
            }
        }
    }

    public void SelectOpponent(int index)
    {
        selectedOpponentIndex = index;
        startBattleButton.interactable = true;
        Debug.Log($"Opponent {index} selected");
        UpdateSelectionIndicators(index);
    }
    private void UpdateSelectionIndicators(int selectedIndex)
    {
        for (int i = 0; i < selectionIndicators.Count; i++)
        {
            if (i == selectedIndex)
            {
                selectionIndicators[i].text = "selecting";
                characterCountTexts[i].text = opponentNames[i];
            }
            else
            {
                selectionIndicators[i].text = "";
                characterCountTexts[i].text = "";
            }
        }
    }

    public int GetSelectedOpponentIndex()
    {
        return selectedOpponentIndex;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
