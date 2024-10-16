using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HideOnClickOutside : MonoBehaviour, IPointerDownHandler
{
    public GameObject obj;

    // 當點擊發生時，檢查是否點擊在 UI 內部
    public void OnPointerDown(PointerEventData eventData)
    {
        if (obj.activeInHierarchy && !IsPointerOverUI(eventData))
        {
            obj.SetActive(false); // 點擊在 UI 外，隱藏 UI
        }
    }

    // 檢查滑鼠或觸摸是否在目標 UI 元素內
    private bool IsPointerOverUI(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 檢查返回的 Raycast 結果中是否包含目標 UI
        foreach (var result in results)
        {
            if (result.gameObject == obj)
            {
                return true; // 點擊在目標 UI 上
            }
        }

        return false; // 點擊在 UI 外
    }
}
