using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DeveloperTool : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentParent;
    public BenchManager benchManager;
    public GameEvent startBattle;
    private List<GameObject> characterButtons = new List<GameObject>();
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (SpawnGrid.Instance.indexToCubeKey.TryGetValue(43, out string cubeKey))
            {
                if (SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode hexNode))
                {
                    Vector3 position = hexNode.Position;
                    Character characterData = ResourcePool.Instance.GetCharacterByID(31);
                    GameObject characterPrefab = characterData.Model;
                    GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, hexNode, ResourcePool.Instance.enemy, isAlly: false);
                    StartCoroutine(StartBattleCorutine());
                }
                else
                {
                    CustomLogger.LogError(this, $"No HexNode found for cube key {cubeKey}");
                }
            }
        }
    }
    public IEnumerator StartBattleCorutine()
    {
        yield return new WaitForSeconds(3.5f);
        startBattle.Raise();
    }
    public void GenerateCharacterButtons()
    {
        // 获取所有角色的列表
        List<Character> allCharacters = ResourcePool.Instance.GetAllCharacters();

        foreach (var character in allCharacters)
        {
            // 创建按钮实例
            GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
            Button button = buttonObj.GetComponent<Button>();
            Image image = buttonObj.GetComponent<Image>();

            // 设置按钮的头像图片
            if (character.Sprite != null)
            {
                image.sprite = character.Sprite;
                image.color = Color.white; // 确保图片不被透明化
            }
            else
            {
                Debug.LogWarning($"角色 {character.CharacterName} 缺少头像 Sprite。");
                image.color = new Color(1, 1, 1, 0); // 设置为透明
            }

            // 添加点击事件
            Character capturedCharacter = character; // 闭包问题，捕获当前角色
            button.onClick.AddListener(() => SpawnCharacter(capturedCharacter));

            // 将按钮添加到列表（可选）
            characterButtons.Add(buttonObj);
        }
    }

    private void SpawnCharacter(Character character)
    {
        if (!benchManager.IsBenchFull())
        {
            benchManager.AddToBench(character.Model);
        }
        else
        {
            // 处理备战席已满的情况
            Debug.Log("备战席已满，无法添加新角色。");
        }
    }
}
