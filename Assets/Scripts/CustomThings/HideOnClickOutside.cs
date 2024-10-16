using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HideOnClickOutside : MonoBehaviour, IPointerDownHandler
{
    public GameObject obj;

    // ���I���o�ͮɡA�ˬd�O�_�I���b UI ����
    public void OnPointerDown(PointerEventData eventData)
    {
        if (obj.activeInHierarchy && !IsPointerOverUI(eventData))
        {
            obj.SetActive(false); // �I���b UI �~�A���� UI
        }
    }

    // �ˬd�ƹ���Ĳ�N�O�_�b�ؼ� UI ������
    private bool IsPointerOverUI(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // �ˬd��^�� Raycast ���G���O�_�]�t�ؼ� UI
        foreach (var result in results)
        {
            if (result.gameObject == obj)
            {
                return true; // �I���b�ؼ� UI �W
            }
        }

        return false; // �I���b UI �~
    }
}
