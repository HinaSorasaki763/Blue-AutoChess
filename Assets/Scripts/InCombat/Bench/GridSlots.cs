using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GridSlots : MonoBehaviour
{
    public CharacterCTRL OccupyingCharacter;
    public GameObject head;
    public GameObject nail;
    public bool IsBattlefield; // 格子是否位於戰場
    public bool IsLogistics;
    public bool isDesertified = false;
    public void SetOccupyingCharacter(CharacterCTRL character)
    {
        if (character != null)
        {
            OccupyingCharacter = character;
            Debug.Log($"Slot {gameObject.name} is now occupied by {character.name}");
        }
        else
        {
            OccupyingCharacter = null;
            Debug.Log($"Slot {gameObject.name} is now empty.");
        }
    }
}
