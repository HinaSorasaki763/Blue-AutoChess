using UnityEngine;
using Unity.Netcode;

public class DedicatedServerManager : MonoBehaviour
{
    void Start()
    {
        // �Ұʦ��A��
        NetworkManager.Singleton.StartServer();
        CustomLogger.Log(this, "Dedicated Server started");
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            // �M�z�^�I
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
        }
    }

    // �Ȥ�ݳs���^�I
    private void OnClientConnectedCallback(ulong clientId)
    {
        CustomLogger.Log(this, $"Client {clientId} connected to the server.");
    }

    // �Ȥ���_�}�^�I
    private void OnClientDisconnectedCallback(ulong clientId)
    {
        CustomLogger.Log(this, $"Client {clientId} disconnected from the server.");
    }
}
