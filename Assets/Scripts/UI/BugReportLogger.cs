using System;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static UnityEditor.Progress;

/// <summary>
/// �o�ӰO�����Ω�Τ@�O���C���U�ؾާ@�A�ÿ�XBug���i�C
/// </summary>
public class BugReportLogger : MonoBehaviour
{
    public static BugReportLogger Instance { get; private set; }
    [SerializeField]
    private TMP_InputField bugDescriptionInput;  // ���V��������TMP_InputField

    // �����Ҧ��ާ@�T��
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
    /// �Τ@�I�s����k�H�g�JLog�P������x
    /// </summary>
    /// <param name="msg">�n�O�����T��</param>
    private void RecordAction(string msg)
    {
        // �ϥΦۭqLogger
        CustomLogger.Log(this, msg);

        // �o�̱N�ɶ� + ²�u�T���g�JStringBuilder
        string entry = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {msg}";
        _logBuilder.AppendLine(entry);
    }

    /// <summary>
    /// �ʶR����
    /// </summary>
    /// <param name="charName">����W��</param>
    public void GetCharacter(string charName)
    {
        RecordAction($"Get char: {charName}");
    }

    /// <summary>
    /// �N���Ⲿ�ʨ���w��l
    /// </summary>
    /// <param name="charName">����W��</param>
    /// <param name="tileName">�ؼЮ�l�W��</param>
    public void MoveCharacterToTile(string charName, string tileName)
    {
        RecordAction($"Move char: {charName} to tile: {tileName}");
    }

    /// <summary>
    /// ��������˳�
    /// </summary>
    /// <param name="charName">����W��</param>
    /// <param name="itemName">�˳ƦW��</param>
    public void EquipItemToCharacter(string charName, string itemName)
    {
        RecordAction($"Equip item: {itemName} to char: {charName}");
    }

    /// <summary>
    /// �ϥή��ӫ~�b���w���⨭�W
    /// </summary>
    /// <param name="charName">����W��</param>
    /// <param name="consumableName">���ӫ~�W��</param>
    public void UseConsumableOnCharacter(string charName, string consumableName)
    {
        RecordAction($"Use consumable: {consumableName} on char: {charName}");
    }

    /// <summary>
    /// �﨤��ϥίS��D��
    /// </summary>
    /// <param name="charName">����W��</param>
    public void UseSpecialItemOnCharacter(string charName)
    {
        RecordAction($"Use special item on char: {charName}");
    }

    /// <summary>
    /// ��ܼĤH
    /// </summary>
    /// <param name="roundIndex">�^�X��</param>
    /// <param name="enemyIndex">�ĤH�s��</param>
    /// <param name="enemyChars">�ĤH������M��</param>
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
    /// �}�l�԰�
    /// </summary>
    public void StartBattle()
    {
        RecordAction("Battle start");
    }

    /// <summary>
    /// �����԰�
    /// </summary>
    public void EndBattle()
    {
        RecordAction("Battle end");
    }

    /// <summary>
    /// ��ܱj��
    /// </summary>
    /// <param name="enhancementName">�j�ƦW��</param>
    public void ChooseEnhancement(string enhancementName)
    {
        RecordAction($"Choose enhancement: {enhancementName}");
    }

    /// <summary>
    /// �N�����쪺Log�A�H�Ϊ��a����J�]�Y���^�g�J��r��
    /// </summary>
    public void ExportLog()
    {
        // �N���a��J(�Y��)�]�[�J��x
        if (bugDescriptionInput != null && !string.IsNullOrWhiteSpace(bugDescriptionInput.text))
        {
            RecordAction($"BugDesc: {bugDescriptionInput.text}");
        }

        // �եX�ɦW
        string fileName = $"BugReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // �g�J�ɮ�
        File.WriteAllText(filePath, _logBuilder.ToString());

        // �i��ܦs�ɦ�m���ܵ����a
        CustomLogger.Log(this, $"Exported to: {filePath}");
    }
}
