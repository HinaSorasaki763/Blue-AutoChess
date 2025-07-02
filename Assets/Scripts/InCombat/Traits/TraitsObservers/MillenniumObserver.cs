// MillenniumObserver.cs
using System.Collections.Generic;
using UnityEngine;

public class MillenniumObserver : CharacterObserverBase
{
    private int traitLevel;
    private float timer;
    private float interval = 1f; // 每秒触发一次
    private bool isAlive;
    private CharacterCTRL character;

    public MillenniumObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        traitLevel = level;
        this.character = character;
        isAlive = true;
    }
    public override void ActivateTrait()
    {
        DataStackManager.Instance.UpdateIndicator();
    }
    public override void DeactivateTrait()
    {
        DataStackManager.Instance.UpdateIndicator();
    }
    public override void OnBattleEnd(bool isVictory)
    {
        base.OnBattleEnd(isVictory);
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//層數
            {0, new TraitLevelStats(0,0)},
            {1, new TraitLevelStats(1,10)},
            {2, new TraitLevelStats(2,20)},
            {3, new TraitLevelStats(4,30)},
            {4, new TraitLevelStats(10,30)}
        };
        return statsByStarLevel;
    }


    public override void CharacterUpdate()
    {
        if (!isAlive || !character.enterBattle || !character.IsAlly) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            OnIncreaseDataLayer();
        }
    }
    public void GetAward()
    {

    }
    private void OnIncreaseDataLayer()
    {
        int stack = GetTraitObserverLevel()[traitLevel].Data1;
        DataStackManager.Instance.IncreaseDataStack(stack);
        CustomLogger.Log(this, $"{character.name} 增加了 {stack} 层数据层数。当前总层数：{DataStackManager.Instance.CurrentDataStack}");
    }

    public override void OnDying(CharacterCTRL character)
    {
        base.OnDying(character);
        if (character == this.character)
        {
            isAlive = false;
            Debug.Log($"[MillenniumObserver] {character.name} 已死亡，停止增加数据层数。");
        }
    }
}
