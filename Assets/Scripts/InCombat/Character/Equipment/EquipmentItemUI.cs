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
    public GameObject Detail;
    public Button Btn;
    public LayerMask characterLayerMask;
    private bool isDragging;

    // 新增 GridLayoutGroup 和原始索引變數
    private GridLayoutGroup gridLayoutGroup;
    private int originalIndex;

    // 虛影用變數
    private GameObject ghostItem;

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
        // 虛影跟隨鼠標移動
        ghostItem.transform.position = Input.mousePosition;
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

        // 刪除虛影
        Destroy(ghostItem);

        if (!successfulEquip)
        {
            // 如果未成功裝備，將原物品恢復到其原來的父級位置
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalIndex); // 將物品放回原來的索引位置
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