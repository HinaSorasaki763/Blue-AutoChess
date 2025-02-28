using System.Collections.Generic;
using UnityEngine;

public class CombinationRouteDisplay : MonoBehaviour
{
    [Header("資料來源")]
    public CombinationRouteSO combinationRouteSO;

    [Header("UI 設定")]
    public GameObject routeItemPrefab;   // 包含 CombinationRouteItem 腳本的預製物件
    public Transform routesContainer;    // 用於放置所有路徑項目的容器
    public EquipmentTooltip equipmentTooltip; // 結果裝備 Tooltip 的參照

    void Start()
    {
        PopulateRoutes();
    }

    void PopulateRoutes()
    {
        // 清空舊項目（如果有）
        foreach (Transform child in routesContainer)
        {
            Destroy(child.gameObject);
        }

        // 依序產生每筆合成路徑項目
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
