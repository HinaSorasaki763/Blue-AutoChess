using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Text;

public class NetworkUIManager : MonoBehaviour
{
    // 這是 InputField，用來輸入 IP 地址
    public InputField ipInputField;

    // 當按下按鈕時，調用這個方法來嘗試連接
    public void OnConnectButtonClicked()
    {
        // 從 InputField 中獲取輸入的 IP 地址
        string ipAddress = ipInputField.text;

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
}
