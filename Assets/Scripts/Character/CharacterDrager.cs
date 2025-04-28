using GameEnum;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDrager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler,IPointerClickHandler
{
    public HexNode PreSelectedGrid;
    private CharacterCTRL characterCTRL;

    private void Start()
    {
        characterCTRL = GetComponent<CharacterCTRL>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!characterCTRL.Dragable())
        {
            return;
        }
        if (!characterCTRL.isObj)
        {
            GetComponent<CustomAnimatorController>().ChangeState(CharacterState.PickedUp);

        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, LayerMask.GetMask("Grid")))
        {
            transform.position = hit.collider.transform.position + Vector3.up * 0.5f;
            PreSelectedGrid = hit.collider.GetComponent<HexNode>();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        characterCTRL.RecalculateStats();
        StringBuilder sb = new();
        foreach (var item in characterCTRL.traitController.GetCurrentTraits())
        {
            sb.AppendLine(item.ToString());
        }
        foreach (var item in characterCTRL.equipmentManager.equippedItems)
        {
            sb.AppendLine(item.ToString());
        }
        sb.AppendLine("Character Stats:");
        foreach (StatsType type in Enum.GetValues(typeof(StatsType)))
        {
            float value = characterCTRL.GetStat(type);
            sb.AppendLine($"{type}: {value}");
        }
        CustomLogger.Log(this,sb.ToString());
        if (!UIManager.Instance.TryClose(characterCTRL))
        {
            UIManager.Instance.ShowCharacterStats(characterCTRL);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        characterCTRL.AudioManager.PlayPickedUpSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!characterCTRL.Dragable())
        {
            return;
        }
        if (!characterCTRL.isObj)
        {
            GetComponent<CustomAnimatorController>().ChangeState(CharacterState.Idling);
        }

        (bool, bool) tuple = GameController.Instance.TryMoveCharacter(characterCTRL, PreSelectedGrid);
        if (PreSelectedGrid != null && PreSelectedGrid.CompareTag("SellGrid"))
        {
            PreSelectedGrid.GetComponent<SellGrid>().OnCharacterEnter(characterCTRL);
        }

        if (!tuple.Item2)
        {
            
        }
    }

}
