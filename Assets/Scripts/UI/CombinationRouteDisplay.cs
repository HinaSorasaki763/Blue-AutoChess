using System.Collections.Generic;
using UnityEngine;

public class CombinationRouteDisplay : MonoBehaviour
{
    [Header("��ƨӷ�")]
    public CombinationRouteSO combinationRouteSO;

    [Header("UI �]�w")]
    public GameObject routeItemPrefab;   // �]�t CombinationRouteItem �}�����w�s����
    public Transform routesContainer;    // �Ω��m�Ҧ����|���ت��e��
    public EquipmentTooltip equipmentTooltip; // ���G�˳� Tooltip ���ѷ�

    void Start()
    {
        PopulateRoutes();
    }

    void PopulateRoutes()
    {
        // �M���¶��ء]�p�G���^
        foreach (Transform child in routesContainer)
        {
            Destroy(child.gameObject);
        }

        // �̧ǲ��ͨC���X�����|����
        foreach (var entry in combinationRouteSO.combinationEntries)
        {
            GameObject itemObj = Instantiate(routeItemPrefab, routesContainer);
            CombinationRouteItem item = itemObj.GetComponent<CombinationRouteItem>();
            if (item != null)
            {
                item.Setup(entry, equipmentTooltip);
            }
        }
    }
}
