using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AriusManager : MonoBehaviour
{
    public static AriusManager Instance { get; private set; }
    public List<CharacterCTRL> OnBoard = new();
    public CharacterCTRL SonOfGod;
    public GameObject SelectorRef;
    public EquipmentSO SelectorEquipmentData;
    public IEquipment equipment;
    public void Awake()
    {
        Instance = this;

    }
    public void Initiate()
    {
        foreach (var item in EquipmentManager.Instance.availableEquipments)
        {
            if (item.Id == 27)
            {
                equipment = item;
            }
        }
    }
    public void RemoveSelector()
    {
        if (SelectorRef != null)
        {
            EquipmentManager.Instance.RemoveEquipmentItem(equipment, SelectorRef);
            SelectorRef = null;
        }
    }
    public void GetSelector()
    {
        if (SelectorRef == null)
        {
            SelectorRef = EquipmentManager.Instance.AddEquipmentItem(equipment);
        }

    }
    public void AddArius(CharacterCTRL character)
    {
        if (!OnBoard.Contains(character))
        {
            OnBoard.Add(character);
        }

    }

    public void RemoveArius(CharacterCTRL character)
    {
        if (OnBoard.Contains(character))
        {
            OnBoard.Remove(character);
        }

    }
    public void ResetAllGodOfSonFlags()
    {
        foreach (var child in OnBoard)
        {
            CharacterCTRL character = child.GetComponent<CharacterCTRL>();
            if (character == null) continue;
            var observer = character.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            if (observer != null && observer.IsSonOfGod)
            {
                observer.SetGodOfSon(false);
                CustomLogger.Log(this, $"Reset IsGodOfSon for {character.name}");
            }
        }
    }
    public List<CharacterCTRL> GetOnBoard(bool includeSonOfGod)
    {
        List<CharacterCTRL> validCharacters = new List<CharacterCTRL>();
        foreach (var item in OnBoard)
        {
            if (!item.gameObject.activeInHierarchy || item.characterStats.logistics || !item.isAlive)
                continue;

            if (!includeSonOfGod)
            {
                var observer = item.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
                // 若 observer 為 null 或判定為 SonOfGod 則略過該角色
                if (observer != null && observer.IsSonOfGod)
                    continue;
            }
            validCharacters.Add(item);
        }
        return validCharacters;
    }

}
