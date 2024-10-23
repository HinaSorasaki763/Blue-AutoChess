using UnityEngine;
using Unity.Netcode;

public class DedicatedServerManager : MonoBehaviour
{
    void Start()
    {
        // 啟動伺服器
        NetworkManager.Singleton.StartServer();
        CustomLogger.Log(this, "Dedicated Server started");
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            // 清理回呼
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
    }

    // 客戶端連接回呼
    private void OnClientConnectedCallback(ulong clientId)
    {
        CustomLogger.Log(this, $"Client {clientId} connected to the server.");
    }

    // 客戶端斷開回呼
    private void OnClientDisconnectedCallback(ulong clientId)
    {
        CustomLogger.Log(this, $"Client {clientId} disconnected from the server.");
    }
}
