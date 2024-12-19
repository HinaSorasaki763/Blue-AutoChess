using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class AriusManager : MonoBehaviour
{
    public static AriusManager Instance { get; private set; }
    public List<CharacterCTRL> OnBoard = new();
    public void Awake()
    {
        Instance = this;
    }
    public void AddArius(CharacterCTRL character)
    {
        OnBoard.Add(character);

        // 打印列表內所有角色
        CustomLogger.Log(this, "Current OnBoard characters:");
        foreach (var item in OnBoard)
        {
            var observer = item.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            CustomLogger.Log(this, $"Character: {item.name}, Traits: {string.Join(", ", item.traitController.GetCurrentTraits())},is god = {observer.IsGodOfSon}");
        }

        bool hasGodOfSon = false;

        foreach (var item in OnBoard)
        {
            var observer = item.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            if ( observer.IsGodOfSon)
            {
                hasGodOfSon = true;
                break; // 已找到擁有 IsGodOfSon 的角色，直接退出迴圈
            }
        }

        if (!hasGodOfSon)
        {
            ReChoose();
        }
    }

    public void RemoveArius(CharacterCTRL character)
    {
        OnBoard.Remove(character);
        StringBuilder sb = new StringBuilder();
        foreach (var item in OnBoard)
        {
            sb.AppendLine(item.ToString());
        }
        CustomLogger.Log(this, sb.ToString());
    }
    public void ReChoose()
    {
        if (OnBoard.Count <= 0)
        {
            CustomLogger.LogWarning(this, "No characters available on board to select.");
            return;
        }
        foreach (var item in OnBoard)
        {
            var ob= item.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            CustomLogger.Log(this, $"Character: {item.name}, Traits: {string.Join(", ", item.traitController.GetCurrentTraits())},is god = {ob.IsGodOfSon}");
        }

        int randomIndex = Random.Range(0, OnBoard.Count); // 生成隨機索引
        CharacterCTRL selectedCharacter = OnBoard[randomIndex];
        CustomLogger.Log(this, $"Randomly selected character: {selectedCharacter.name}");
        var observer = selectedCharacter.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
        if (observer != null)
        {
            observer.SetGodOfSon(true);
            CustomLogger.Log(this, $"Reset IsGodOfSon for {selectedCharacter.name}");
        }
        else
        {
            CustomLogger.LogWhenThingShouldntHappened(this);
        }
    }
}
