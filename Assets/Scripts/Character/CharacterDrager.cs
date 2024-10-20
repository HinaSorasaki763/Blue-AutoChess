using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDrager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public HexNode PreSelectedGrid;
    private CharacterCTRL characterCTRL;

    private void Start()
    {
        characterCTRL = GetComponent<CharacterCTRL>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (characterCTRL.enterBattle)
        {
            return;
        }
        GetComponent<CustomAnimatorController>().ChangeState(CharacterState.PickedUp);

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, LayerMask.GetMask("Grid")))
        {
            transform.position = hit.collider.transform.position + Vector3.up * 0.5f;
            PreSelectedGrid = hit.collider.GetComponent<HexNode>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.Instance.ShowCharacterStats(characterCTRL);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (characterCTRL.enterBattle)
        {
            return;
        }
        GetComponent<CustomAnimatorController>().ChangeState(CharacterState.Idling);
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
