using GameEnum;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AriusManager : MonoBehaviour
{
    public static AriusManager Instance { get; private set; }
    public List<CharacterCTRL> OnBoard = new();
    public List<CharacterCTRL> EnemyOnBoard = new();
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
        ResetAllGodOfSonFlags();
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
        if (character.IsAlly)
        {
            if (!OnBoard.Contains(character))
            {
                OnBoard.Add(character);
            }
        }
        else
        {
            if (!EnemyOnBoard.Contains(character))
            {
                EnemyOnBoard.Add(character);
            }
        }


    }

    public void RemoveArius(CharacterCTRL character)
    {
        if (OnBoard.Contains(character))
        {
            OnBoard.Remove(character);
        }
        if (EnemyOnBoard.Contains(character))
        {
            EnemyOnBoard.Remove(character);
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
        foreach (var item in EnemyOnBoard)
        {
            CharacterCTRL character = item.GetComponent<CharacterCTRL>();
            if (character == null) continue;
            var observer = character.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
            if (observer != null && observer.IsSonOfGod)
            {
                observer.SetGodOfSon(false);
                CustomLogger.Log(this, $"Reset IsGodOfSon for {character.name}");
            }
        }
    }
    public List<CharacterCTRL> GetOnBoard(bool includeSonOfGod,bool isally)
    {
        List<CharacterCTRL> validCharacters = new List<CharacterCTRL>();
        List<CharacterCTRL> c = isally ? OnBoard : EnemyOnBoard;
        foreach (var item in c)
        {
            if (!item.gameObject.activeInHierarchy || item.characterStats.logistics || !item.isAlive)
                continue;

            if (!includeSonOfGod)
            {
                if (SelectedAugments.Instance.CheckAugmetExist(125) && item.traitController.GetAcademy() == Traits.Trinity)
                {
                    validCharacters.Add(item);
                }
                var observer = item.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
                // 若 observer 為 null 或判定為 SonOfGod 則略過該角色
                if (observer != null && observer.IsSonOfGod)
                    continue;
            }
            if (item.characterStats.logistics)
            validCharacters.Add(item);
        }
        return validCharacters;
    }

}
