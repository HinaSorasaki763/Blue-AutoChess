using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HideOnClickOutside : MonoBehaviour, IPointerDownHandler
{
    public GameObject obj;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (obj.activeInHierarchy && !IsPointerOverUI(eventData))
        {
            obj.SetActive(false);
        }
    }
    private bool IsPointerOverUI(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
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
