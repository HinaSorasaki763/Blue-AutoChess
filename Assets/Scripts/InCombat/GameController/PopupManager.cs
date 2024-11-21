using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPrefab; // �w�s����]MessagePopup�^
    public static PopupManager Instance;
    public void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// �Ыب���ܤ@�Ӥ�r�u���C
    /// </summary>
    /// <param name="message">��ܪ��T��</param>
    /// <param name="displayDuration">��ܪ��`�ɶ�</param>
    public void CreatePopup(string message, float displayDuration)
    {
        GameObject popupInstance = Instantiate(popupPrefab, transform); // �b Canvas �U�ͦ�
        MessagePopup popup = popupInstance.GetComponent<MessagePopup>();
        popup.ShowMessage(message, displayDuration);
    }
}
