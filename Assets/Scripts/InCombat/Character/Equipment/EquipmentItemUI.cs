using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using GameEnum;
using TMPro;

public class EquipmentItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image icon;
    public IEquipment equipmentData;

    private Transform originalParent;
    private Canvas canvas;
    private EquipmentManager equipmentManager;
    public GameObject Detail;
    public Button Btn;
    public LayerMask characterLayerMask;
    // 用來判斷是否在拖動中
    private bool isDragging;

    public void Setup(IEquipment equipment, EquipmentManager manager, Transform parent)
    {
        equipmentData = equipment;
        icon.sprite = equipment.Icon;
        equipmentManager = manager;
        originalParent = parent;
        EquipmentUIManager.Instance.RegisterUI(Detail);
        Detail.SetActive(false);
    }

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas.transform);
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        foreach (var result in raycastResults)
        {
            if (result.gameObject.CompareTag("Character"))
            {
                CharacterCTRL character = result.gameObject.GetComponent<CharacterCTRL>();
                if (character != null)
                {
                    BasicEquipment combinableEquipment = null;
                    foreach (var item in character.equipmentManager.GetEquippedItems())
                    {
                        if (item is BasicEquipment basicItem)
                        {
                            if (character.equipmentManager.CanCombine(basicItem, equipmentData,out IEquipment resultEquipment))
                            {
                                combinableEquipment = basicItem;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        // 遍歷Raycast結果，只選擇指定圖層的物件
        foreach (var result in raycastResults)
        {
            if (((1 << result.gameObject.layer) & characterLayerMask) != 0) // 確認物件在指定的圖層上
            {
                CharacterCTRL character = result.gameObject.GetComponent<CharacterCTRL>();
                if (character != null)
                {
                    bool equipped = character.EquipItem(equipmentData);
                    CustomLogger.Log(this, $"try equip {equipmentData.EquipmentName} on {character.name} success = {equipped}");
                    if (equipped)
                    {
                        equipmentManager.RemoveEquipmentItem(equipmentData,gameObject);
                    }
                    else
                    {
                        transform.SetParent(originalParent);
                        transform.localPosition = Vector3.zero;
                    }
                    return;
                }
            }
        }
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 只有在非拖動模式下才觸發點擊事件
        if (!isDragging)
        {
            EquipmentUIManager.Instance.ToggleUI(Detail);
        }
    }
}
