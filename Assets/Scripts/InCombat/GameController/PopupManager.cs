using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPrefab; // 預製物件（MessagePopup）
    public static PopupManager Instance;
    public void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// 創建並顯示一個文字彈窗。
    /// </summary>
    /// <param name="message">顯示的訊息</param>
    /// <param name="displayDuration">顯示的總時間</param>
    public void CreatePopup(string message, float displayDuration)
    {
        GameObject popupInstance = Instantiate(popupPrefab, transform); // 在 Canvas 下生成
        MessagePopup popup = popupInstance.GetComponent<MessagePopup>();
        popup.ShowMessage(message, displayDuration);
    }
}
