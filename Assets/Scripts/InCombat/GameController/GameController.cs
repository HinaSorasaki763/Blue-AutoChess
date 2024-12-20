﻿using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    private int gold;
    public CharacterParent CharacterParent;
    public TextMeshProUGUI GoldText;
    private void Update()
    {
        GoldText.text = $"gold : {gold}";
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public (bool, bool) TryMoveCharacter(CharacterCTRL character, HexNode targetSlot)
    {
        if (!targetSlot)
        {
            Debug.Log("!targetSlot");
            return (false, false);
        }
        bool isBattlefield = targetSlot.IsBattlefield;
        bool isLogisticsSlot = targetSlot.IsLogistics;
        bool isLogisticsCharacter = character.characterStats.logistics;
        if (targetSlot.Index >= 32)
        {
            PopupManager.Instance.CreatePopup("enemy territory", 2);
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        if (!isBattlefield)
        {
            MoveOrSwapCharacter(character, targetSlot);

            return (true, isBattlefield);
        }
        if (isLogisticsCharacter != isLogisticsSlot)
        {
            Debug.Log(isLogisticsCharacter ? "後勤角色只能放置在後勤格子" : "前線角色不能放置在後勤格子");
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        CustomLogger.Log(this, $"GameStageManager.Instance.GetCharacterLimit() = {GameStageManager.Instance.GetCharacterLimit()}");
        if (ResourcePool.Instance.ally.GetBattleFieldCharacter().Count >= GameStageManager.Instance.GetCharacterLimit() && !character.CurrentHex.IsBattlefield && targetSlot.OccupyingCharacter == null)
        {
            PopupManager.Instance.CreatePopup("On Limit", 2);
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        MoveOrSwapCharacter(character, targetSlot);
        Debug.Log("other situation");
        return (true, isBattlefield);
    }


    private void ReturnToOriginalSlot(CharacterCTRL character)
    {
        if (character.CurrentHex != null)
        {
            Vector3 originalPosition = new Vector3(character.CurrentHex.transform.position.x, 0.14f, character.CurrentHex.transform.position.z);
            character.transform.position = originalPosition;
        }
        else
        {
            Debug.LogWarning("角色當前沒有有效的格子");
        }
    }

    private void MoveOrSwapCharacter(CharacterCTRL character, HexNode targetSlot)
    {
        if (character.CurrentHex == targetSlot)
        {
            Vector3 v = character.CurrentHex.transform.position;
            character.transform.position = new Vector3(v.x, 0.14f, v.z);
            targetSlot.GetComponent<HexNode>().Reserve(character);
            return;
        }

        if (targetSlot.OccupyingCharacter == null)
        {
            MoveCharacterToSlot(character, targetSlot);
        }
        else if (!targetSlot.OccupyingCharacter.isObj || (character.CurrentHex.IsBattlefield && targetSlot.IsBattlefield))
        {
            SwapCharacters(character, targetSlot.OccupyingCharacter);
        }
        else
        {
            ReturnToOriginalSlot(character);
        }
        CharacterParent.UpdateTraitEffects();
    }
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var item in CharacterParent.childCharacters)
        {
            CharacterCTRL ctrl = item.GetComponent<CharacterCTRL>();
            if (ctrl.enterBattle && ctrl.isAlive)
            {
                count++;
            }
        }
        return count;
    }
    private void MoveCharacterToSlot(CharacterCTRL character, HexNode targetSlot)
    {
        if (character.CurrentHex != null)
        {
            character.CurrentHex.SetOccupyingCharacter(null);
            if (character.CurrentHex.TryGetComponent<HexNode>(out HexNode He))
            {
                He.Release();
            }

        }
        targetSlot.SetOccupyingCharacter(character);
        character.CurrentHex = targetSlot;
        CheckSlot(character, targetSlot);
        if (targetSlot.TryGetComponent<HexNode>(out HexNode H))
        {
            H.Reserve(character);
        }
        character.transform.position = new Vector3(targetSlot.transform.position.x, 0.14f, targetSlot.transform.position.z);
    }

    private void SwapCharacters(CharacterCTRL character1, CharacterCTRL character2)
    {
        HexNode slot1 = character1.CurrentHex;
        HexNode slot2 = character2.CurrentHex;

        slot1.OccupyingCharacter = character2;
        slot2.OccupyingCharacter = character1;

        character1.CurrentHex = slot2;
        slot2.GetComponent<HexNode>().Reserve(character1);
        character2.CurrentHex = slot1;
        slot1.GetComponent<HexNode>().Reserve(character2);
        CheckSlot(character1,slot2);
        CheckSlot(character2,slot1);
        character1.transform.position = new Vector3(slot2.transform.position.x, 0.14f, slot2.transform.position.z);
        character2.transform.position = new Vector3(slot1.transform.position.x, 0.14f, slot1.transform.position.z);

        Debug.Log($"Swapped {character1.name} and {character2.name} between slots {slot1.name} and {slot2.name}");
    }
    public void CheckSlot(CharacterCTRL c,HexNode hexNode)
    {
        if (!hexNode.IsBattlefield)
        {
            c.traitController.TriggerOnLeaveBattleField();
        }
        else
        {
            c.traitController.TriggerOnEnterBattleField();
        }
    }
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"gold = {gold}");
    }
    public int GetGoldAmount()
    {
        return gold;
    }
}
