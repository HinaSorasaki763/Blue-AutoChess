using GameEnum;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character")]
public class Character : ScriptableObject
{
    public string CharacterName;
    public int CharacterId;
    public int Level;
    public Sprite Sprite;
    public GameObject Model;
    public bool logistics;
    public StatsContainer Stats = new StatsContainer();
    public int[] Health = new int[3];
    public int[] Attack = new int[3];
    public List<Traits> Traits;
    public int BarrageIntervalAngle;
    public int BarrageInitAngle;
    [TextArea(3, 10)]
    public List<string> Tooltips = new List<string>(2);
    public bool isObj;

    public bool TestEnhanceSkill;
    public bool TestBuildInvinvicble;
    public void ApplyLevelUp(int newLevel)
    {
        Level = newLevel;
    }
    private void SetStatIfBelowThreshold(StatsType statType, float threshold, int value)
    {
        if (!isObj)
        {
            if (Stats.GetStat(statType) <= threshold)
            {
                Stats.SetStat(statType, value);
            }
        }

    }

    private void OnValidate()
    {
        // 檢查基本屬性
        if (CharacterName == "") CharacterName = "DefaultName";
        if (Level <= 0) Level = 1;

        // 使用抽取出來的方法來設置數值
        SetStatIfBelowThreshold(StatsType.Health, 0, 100);
        SetStatIfBelowThreshold(StatsType.currHealth, 0, 0);
        SetStatIfBelowThreshold(StatsType.MaxMana, 0, 50);
        SetStatIfBelowThreshold(StatsType.Mana, 0, 0);
        SetStatIfBelowThreshold(StatsType.Attack, 0, 10);
        SetStatIfBelowThreshold(StatsType.AttackSpeed, 0, 1);
        SetStatIfBelowThreshold(StatsType.Resistence, 0, 5);
        SetStatIfBelowThreshold(StatsType.Range, 0, 5);
        SetStatIfBelowThreshold(StatsType.HealthGrowth, 0, 10);
        SetStatIfBelowThreshold(StatsType.ManaGrowth, 0, 5);
        SetStatIfBelowThreshold(StatsType.AttackSpeedGrowth, 0, 0);
        SetStatIfBelowThreshold(StatsType.ResistenceGrowth, 0, 10);
        SetStatIfBelowThreshold(StatsType.CritChance, 0, 25);
        SetStatIfBelowThreshold(StatsType.DodgeChance, 0, 0);
        SetStatIfBelowThreshold(StatsType.Accuracy, 0, 15);
        SetStatIfBelowThreshold(StatsType.CritRatio, 0, 55);
    }
}
