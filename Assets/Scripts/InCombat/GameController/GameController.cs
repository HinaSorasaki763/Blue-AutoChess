using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private int battlefieldCharacterLimit = 10;
    private int currentBattlefieldCharacterCount = 0;
    private int gold;
    public CharacterParent CharacterParent;
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
        if (!isBattlefield)
        {
            MoveOrSwapCharacter(character, targetSlot);
            Debug.Log("!isBattlefield");
            return (true, isBattlefield);
        }
        if (isLogisticsCharacter != isLogisticsSlot)
        {
            Debug.Log(isLogisticsCharacter ? "後勤角色只能放置在後勤格子" : "前線角色不能放置在後勤格子");
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        if (currentBattlefieldCharacterCount >= battlefieldCharacterLimit)
        {
            Debug.Log("戰場角色已達上限");
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        MoveOrSwapCharacter(character, targetSlot);
        Debug.Log("other situation");
        if (character.CurrentHex == null || !character.CurrentHex.IsBattlefield)
        {
            currentBattlefieldCharacterCount++;
        }
        return (true, isBattlefield);
    }


    private void ReturnToOriginalSlot(CharacterCTRL character)
    {
        if (character.CurrentHex != null)
        {
            Vector3 originalPosition = new Vector3(character.CurrentHex.transform.position.x, 0.14f, character.CurrentHex.transform.position.z);
            character.transform.position = originalPosition; // 返回原位
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
            character.transform.position = new Vector3(v.x,0.14f,v.z);
            targetSlot.GetComponent<HexNode>().Reserve(character);
            return;
        }

        if (targetSlot.OccupyingCharacter == null)
        {
            MoveCharacterToSlot(character, targetSlot);
        }
        else
        {
            SwapCharacters(character, targetSlot.OccupyingCharacter);
        }
        CharacterParent.UpdateTraitEffects();
    }
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var item in CharacterParent.childCharacters)
        {
            CharacterCTRL ctrl = item.GetComponent<CharacterCTRL>();
            if (ctrl.enterBattle &&ctrl.isAlive)
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
        character1.transform.position = new Vector3(slot2.transform.position.x, 0.14f, slot2.transform.position.z);
        character2.transform.position = new Vector3(slot1.transform.position.x, 0.14f, slot1.transform.position.z);

        Debug.Log($"Swapped {character1.name} and {character2.name} between slots {slot1.name} and {slot2.name}");
    }
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"gold = {gold}");
    }
}
