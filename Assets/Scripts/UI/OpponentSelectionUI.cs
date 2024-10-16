using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    public void Show(List<EnemyWave> waves)
    {
        panel.SetActive(true);
        for (int i = 0; i < opponentPanels.Count; i++)
        {
            if (i < waves.Count)
            {
                opponentPanels[i].SetActive(true);
                var revealedSlots = enemySpawner.GetRevealedCharacters(waves[i]);

                // ��ܴ��S������Ϥ�
                for (int j = 0; j < opponentImages[i].Count; j++)
                {
                    if (j < revealedSlots.Count)
                    {
                        Character characterData = ResourcePool.Instance.GetCharacterByID(revealedSlots[j].CharacterID);
                        opponentImages[i][j].sprite = characterData.Sprite;
                    }
                    else
                    {
                        opponentImages[i][j].sprite = null; // �p�G�S�����������S����A�M�ŹϤ�
                    }
                }

                // ��s�����`�����
                characterCountTexts[i].text = $"character count: {waves[i].gridSlots.Count}";

                // �]�w���s���I���ƥ�A�ǤJ��e������
                int index = i; // �ϥΧ����ܼ��קK���]���D
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
