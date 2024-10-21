using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkUIManager : MonoBehaviour
{
    // 這是 InputField，用來輸入 IP 地址
    public TMP_InputField ipInputField;

    // 這是 UI Panel 用來顯示警告訊息
    public GameObject warningPanel;  // Panel 物件，從 Unity 編輯器中綁定

    // 這是警告訊息的 Text
    public TextMeshProUGUI warningText;  // 如果使用的是 TextMeshPro

    // 當按下按鈕時，調用這個方法來嘗試連接
    public void OnConnectButtonClicked()
    {

       
        // 從 InputField 中獲取輸入的 IP 地址
        string ipAddress = ipInputField.text;

        // 檢查是否輸入了 IP 地址
        if (string.IsNullOrEmpty(ipAddress))
        {
            // 顯示警告 Panel 並設置警告訊息
            warningPanel.SetActive(true);
            warningText.text = "Please enter a valid IP address!";
            return;
        }

        // 如果有輸入 IP，則隱藏警告 Panel
        warningPanel.SetActive(false);

        // 設定客戶端的 IP 地址，這裡假設使用 UnityTransport
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData(ipAddress, 7777); // 7777 是默認端口號，可以根據需要更改
        }

        // 開始作為客戶端進行連接
        NetworkManager.Singleton.StartClient();
    }

    // 當房主點擊開房按鈕時，啟動伺服器
    public void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnCloseButton()
    {
        // 回到StartMenu
        SceneManager.LoadScene("StartMenu");
    }
    public void OnCloseButtonClicked()
    {
        // 隱藏警告 Panel
        warningPanel.SetActive(false);
    }
}
