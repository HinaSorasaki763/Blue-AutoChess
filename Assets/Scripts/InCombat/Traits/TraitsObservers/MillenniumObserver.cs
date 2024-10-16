// MillenniumObserver.cs
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
        traitLevel = level;
        this.character = character;
        isAlive = true;
    }

    public override void CharacterUpdate()
    {
        if (!isAlive || !character.enterBattle||!character.IsAlly) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            OnIncreaseDataLayer();
        }
    }

    private void OnIncreaseDataLayer()
    {
        DataStackManager.Instance.IncreaseDataLayer(1);
        Debug.Log($"[MillenniumObserver] {character.name} 增加了1层数据层数。当前总层数：{DataStackManager.Instance.CurrentDataStack}");
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
