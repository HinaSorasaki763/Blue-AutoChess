﻿using UnityEngine;
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
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public GameObject Detail;
    
    public Button Btn;
    public LayerMask characterLayerMask;
    private bool isDragging;

    // 新增 GridLayoutGroup 和原始索引變數
    private GridLayoutGroup gridLayoutGroup;
    private int originalIndex;
    public bool IsConsumableItem;
    // 虛影用變數
    private GameObject ghostItem;

    public void Setup(IEquipment equipment, EquipmentManager manager, Transform parent)
    {
        EquipmentUIManager.Instance.RegisterUI(Detail);
        ItemName.text = equipment.EquipmentName;
        ItemDescription.text = equipment.EquipmentDetail;
        Detail.SetActive(false);
        equipmentManager = manager;
        originalParent = parent;
        if (equipment.IsConsumable)
        {
            IsConsumableItem = true;
        }
        equipmentData = equipment;
        if (equipmentData.Observer!=null)
        {
            CustomLogger.Log(this, $"equipmentData {equipmentData.Observer.GetType()}");
        }

        icon.sprite = equipment.Icon;


    }

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();

        // 獲取 GridLayoutGroup
        gridLayoutGroup = GetComponentInParent<GridLayoutGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 禁用 GridLayoutGroup
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.enabled = false;
        }

        // 記錄當前物品的原始索引
        originalIndex = transform.GetSiblingIndex();
        Utility.ChangeImageAlpha(gameObject.GetComponentInChildren<Image>(),0.5f);
        ghostItem = new GameObject("GhostItem", typeof(RectTransform), typeof(CanvasGroup));
        ghostItem.transform.SetParent(canvas.transform);
        Image ghostImage = ghostItem.AddComponent<Image>();
        ghostImage.sprite = icon.sprite;
        CanvasGroup canvasGroup = ghostItem.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // 設置虛影透明度
        canvasGroup.blocksRaycasts = false; // 避免阻擋鼠標事件

        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {

        ghostItem.transform.position = Input.mousePosition;
        Utility.ChangeImageAlpha(gameObject.GetComponentInChildren<Image>(), 1);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        bool canCompose = false;
        foreach (var result in raycastResults)
        {
            if (((1 << result.gameObject.layer) & characterLayerMask) != 0)
            {
                CharacterCTRL character = result.gameObject.GetComponent<CharacterCTRL>();
                foreach (var item in character.equipmentManager.equippedItems)
                {
                    if (CharacterEquipmentManager.CanCombine(item, equipmentData, out IEquipment resultEq))
                    {
                        CustomLogger.Log(this,$"can combine {item} and {equipmentData} to {resultEq}");
                        canCompose = true;
                        UIManager.Instance.ShowEquipmentPreComposing(item, equipmentData, resultEq);
                    }
                }
            }
        }
        if (!canCompose)
        {
            UIManager.Instance.DisableEquipmentPreComposing();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        Utility.ChangeImageAlpha(gameObject.GetComponentInChildren<Image>(), 1);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        bool successfulEquip = false;

        foreach (var result in raycastResults)
        {
            if (((1 << result.gameObject.layer) & characterLayerMask) != 0)
            {
                CharacterCTRL character = result.gameObject.GetComponent<CharacterCTRL>();
                if (IsConsumableItem)
                {
                    ConsumableItem consumableItem = equipmentData as ConsumableItem;
                    if (consumableItem != null)
                    {
                        consumableItem.consumableEffect.ApplyEffect(character);
                        Destroy(ghostItem);
                        if (!consumableItem.consumableEffect.Permanent)
                        {
                            equipmentManager.RemoveEquipmentItem(equipmentData, gameObject);

                        }
                        BugReportLogger.Instance.UseConsumableOnCharacter(character.name, consumableItem.EquipmentName);
                        if (gridLayoutGroup != null)
                        {
                            gridLayoutGroup.enabled = true;
                        }
                        transform.SetParent(originalParent);
                        transform.SetSiblingIndex(0);
                        transform.localPosition = Vector3.zero;
                        isDragging = false;
                        return;
                    }
                }
                if (character != null)
                {
                    successfulEquip = character.EquipItem(equipmentData);
                    CustomLogger.Log(this, $"try equip {equipmentData.EquipmentName} on {character.name} success = {successfulEquip}");
                    if (successfulEquip)
                    {
                        equipmentManager.RemoveEquipmentItem(equipmentData, gameObject);
                        break;
                    }
                }
            }
        }
        Destroy(ghostItem);

        if (!successfulEquip)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalIndex);
            transform.localPosition = Vector3.zero;
        }

        // 恢復 GridLayoutGroup 排列功能
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.enabled = true;
        }

        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging)
        {
            EquipmentUIManager.Instance.ToggleUI(Detail);
        }
    }
}
