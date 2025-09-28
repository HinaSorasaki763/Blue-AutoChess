using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Firebase.Firestore;
using System;

public class OpponentSelectionUI : MonoBehaviour
{
    public GameObject panel; // ��� UI �����O
    public Button startBattleButton; // �}�l�԰����s
    public List<GameObject> opponentPanels; // �T�Ӫ����� UI
    public List<List<Image>> opponentImages = new List<List<Image>>();
    public List<Image> opponentImages1;
    public List<Image> opponentImages2;
    public List<Image> opponentImages3;
    public List<Button> opponentButtons; // �C�Ӫ���������ܫ��s
    public List<TextMeshProUGUI> selectionIndicators; // ���ܥثe�Q��������
    public List<TextMeshProUGUI> characterCountTexts; // ��ܨC�ӹ�⪺�����`��

    private EnemySpawner enemySpawner;
    private int selectedOpponentIndex = -1;

    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();
        panel.SetActive(false); // �@�}�l���ÿ�� UI
        startBattleButton.interactable = false;

        // ��l�� opponentImages �����c
        opponentImages.Add(opponentImages1);
        opponentImages.Add(opponentImages2);
        opponentImages.Add(opponentImages3);

        // ��l�ƿ�ܫ��ܾ�
        UpdateSelectionIndicators(-1);
    }

    public void Show(List<DocumentSnapshot> opponents)
    {
        panel.SetActive(true);

        for (int i = 0; i < opponentPanels.Count; i++)
        {
            if (i < opponents.Count)
            {
                opponentPanels[i].SetActive(true);

                var data = opponents[i].ToDictionary();

                // ����Ū slots
                if (!data.TryGetValue("slots", out object slotsObj) || !(slotsObj is List<object> slotsList))
                {
                    Debug.LogError("Opponent doc has no valid slots.");
                    opponentPanels[i].SetActive(false);
                    continue;
                }

                // ��ܨ���Ϥ�
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

                // ��s�����`�����
                characterCountTexts[i].text = $"character count: {slotsList.Count}";

                // �]�w���s�ƥ�
                int index = i;
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
            }
            else
            {
                selectionIndicators[i].text = "";
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
