using System;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static UnityEditor.Progress;

/// <summary>
/// 這個記錄器用於統一記錄遊戲各種操作，並輸出Bug報告。
/// </summary>
public class BugReportLogger : MonoBehaviour
{
    public static BugReportLogger Instance { get; private set; }
    [SerializeField]
    private TMP_InputField bugDescriptionInput;  // 指向場景中的TMP_InputField

    // 收集所有操作訊息
    private StringBuilder _logBuilder = new StringBuilder();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    /// <summary>
    /// 統一呼叫此方法以寫入Log與內部日誌
    /// </summary>
    /// <param name="msg">要記錄的訊息</param>
    private void RecordAction(string msg)
    {
        // 使用自訂Logger
        CustomLogger.Log(this, msg);

        // 這裡將時間 + 簡短訊息寫入StringBuilder
        string entry = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {msg}";
        _logBuilder.AppendLine(entry);
    }

    /// <summary>
    /// 購買角色
    /// </summary>
    /// <param name="charName">角色名稱</param>
    public void GetCharacter(string charName)
    {
        RecordAction($"Get char: {charName}");
    }

    /// <summary>
    /// 將角色移動到指定格子
    /// </summary>
    /// <param name="charName">角色名稱</param>
    /// <param name="tileName">目標格子名稱</param>
    public void MoveCharacterToTile(string charName, string tileName)
    {
        RecordAction($"Move char: {charName} to tile: {tileName}");
    }

    /// <summary>
    /// 角色穿戴裝備
    /// </summary>
    /// <param name="charName">角色名稱</param>
    /// <param name="itemName">裝備名稱</param>
    public void EquipItemToCharacter(string charName, string itemName)
    {
        RecordAction($"Equip item: {itemName} to char: {charName}");
    }

    /// <summary>
    /// 使用消耗品在指定角色身上
    /// </summary>
    /// <param name="charName">角色名稱</param>
    /// <param name="consumableName">消耗品名稱</param>
    public void UseConsumableOnCharacter(string charName, string consumableName)
    {
        RecordAction($"Use consumable: {consumableName} on char: {charName}");
    }

    /// <summary>
    /// 對角色使用特殊道具
    /// </summary>
    /// <param name="charName">角色名稱</param>
    public void UseSpecialItemOnCharacter(string charName)
    {
        RecordAction($"Use special item on char: {charName}");
    }

    /// <summary>
    /// 選擇敵人
    /// </summary>
    /// <param name="roundIndex">回合數</param>
    /// <param name="enemyIndex">敵人編號</param>
    /// <param name="enemyChars">敵人的角色清單</param>
    public void ChooseEnemyInRound(int roundIndex, int enemyIndex, List<string> enemies,List<string> hexs)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < enemies.Count; i++)
        {
            sb.AppendLine($"{enemies[i]} at {hexs[i]}");
        }
        RecordAction($"Round [{roundIndex}], choose enemy [{enemyIndex}] \n {sb}");
    }

    /// <summary>
    /// 開始戰鬥
    /// </summary>
    public void StartBattle()
    {
        RecordAction("Battle start");
    }

    /// <summary>
    /// 結束戰鬥
    /// </summary>
    public void EndBattle()
    {
        RecordAction("Battle end");
    }

    /// <summary>
    /// 選擇強化
    /// </summary>
    /// <param name="enhancementName">強化名稱</param>
    public void ChooseEnhancement(string enhancementName)
    {
        RecordAction($"Choose enhancement: {enhancementName}");
    }

    /// <summary>
    /// 將收集到的Log，以及玩家的輸入（若有）寫入文字檔
    /// </summary>
    public void ExportLog()
    {
        // 將玩家輸入(若有)也加入日誌
        if (bugDescriptionInput != null && !string.IsNullOrWhiteSpace(bugDescriptionInput.text))
        {
            RecordAction($"BugDesc: {bugDescriptionInput.text}");
        }

        // 組出檔名
        string fileName = $"BugReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // 寫入檔案
        File.WriteAllText(filePath, _logBuilder.ToString());

        // 可顯示存檔位置提示給玩家
        CustomLogger.Log(this, $"Exported to: {filePath}");
    }
}
