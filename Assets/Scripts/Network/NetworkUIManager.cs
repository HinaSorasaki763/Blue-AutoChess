using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;

public class NetworkUIManager : NetworkBehaviour
{
    public TextMeshProUGUI playerListDisplay;  // 用於顯示玩家列表
    public TMP_InputField roomCodeInputField;  // 客戶端輸入房間號碼的輸入框
    public TMP_InputField playerNameInputField; // 客戶端輸入玩家名稱的輸入框
    public TextMeshProUGUI roomCodeDisplay;    // 主機顯示房間號碼
    private string currentRoomCode;            // 當前房間號碼
    private static Dictionary<string, ulong> openRooms = new Dictionary<string, ulong>(); // 房間號碼字典
    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
        }
    }

    // 主機按下按鈕來創建房間
    public void OnCreateServerButtonClicked()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            GenerateRoomCode();
            NetworkManager.Singleton.StartHost();
            CustomLogger.Log(this, "Local server started by host.");
            string hostName = $"Host_{NetworkManager.Singleton.LocalClientId}";
            if (!playerNames.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                playerNames.Add(NetworkManager.Singleton.LocalClientId, hostName);
                CustomLogger.Log(this, $"Added host {hostName} with ID {NetworkManager.Singleton.LocalClientId} to playerNames.");
            }
            UpdateClientPlayerList();
        }
    }
    private void OnClientDisconnectedCallback(ulong clientId)
    {
        // 移除斷開的客戶端
        if (playerNames.ContainsKey(clientId))
        {
            CustomLogger.Log(this, $"Removing player with ID {clientId} from playerNames.");
            playerNames.Remove(clientId);
        }
       // UpdateClientPlayerList();
    }
    public void OnJoinServerButtonClicked()
    {
        string roomCode = roomCodeInputField.text;  // 获取用户输入的房间号
        string playerName = playerNameInputField.text; // 获取用户输入的玩家名称

        if (string.IsNullOrEmpty(roomCode))
        {
            CustomLogger.LogError(this, "Room code is empty. Please try again.");
            return;
        }

        // 如果玩家名称为空，使用默认名称
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Client_{NetworkManager.Singleton.LocalClientId}";
        }

        // 将房间号和玩家名称一起传递
        string connectionData = $"{roomCode}|{playerName}";
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData("127.0.0.1", 7777);  // 本地服务器地址
            CustomLogger.Log(this, "Transport connection data set to 127.0.0.1:7777.");
        }

        NetworkManager.Singleton.StartClient();  // 启动客户端
        CustomLogger.Log(this, $"Client started and attempting to connect with room code: {roomCode} and player name: {playerName}.");
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        CustomLogger.Log(this, $"Client {clientId} connected.");

        if (NetworkManager.Singleton.IsServer)  // 仅在伺服器端處理
        {
            UpdateClientPlayerList();  // 更新所有客戶端的玩家列表
        }
        else
        {
            RequestPlayerListServerRpc();
        }
    }
    // ServerRpc: 客戶端請求主機發送玩家列表
    [ServerRpc(RequireOwnership = false)]
    void RequestPlayerListServerRpc()
    {
        CustomLogger.Log(this, $"Client {NetworkManager.Singleton.LocalClientId} requested player list from server.");
        UpdateClientPlayerList();
    }

    // 通知所有客户端更新玩家列表
    private void UpdateClientPlayerList()
    {
        string playerList = "Players in the game:\n";

        // 構建玩家列表
        foreach (var player in playerNames)
        {
            playerList += $"{player.Value} (ID: {player.Key})\n";
        }

        CustomLogger.Log(this, $"Server is updating player list: {playerList}");
        UpdatePlayerListClientRpc(playerList);
    }
    [ClientRpc]
    private void UpdatePlayerListClientRpc(string playerList)
    {
        CustomLogger.Log(this, $"Client {NetworkManager.Singleton.LocalClientId} received updated player list: {playerList}");
        playerListDisplay.text = playerList;
        CustomLogger.Log(this, $"Updated player list on client {NetworkManager.Singleton.LocalClientId}");
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string rawData = System.Text.Encoding.ASCII.GetString(request.Payload);  // 获取连接数据
        string[] splitData = rawData.Split('|');
        string roomCode = splitData[0];
        string playerName = splitData.Length > 1 ? splitData[1] : $"Client_{request.ClientNetworkId}";

        CustomLogger.Log(this, $"Client attempting to connect with room code: {roomCode} and player name: {playerName}");

        if (openRooms.ContainsKey(roomCode))  // 检查房间号是否存在
        {
            CustomLogger.Log(this, $"Room code {roomCode} is valid. Connection approved.");
            response.Approved = true;
            response.CreatePlayerObject = true;

            // 確保主機正確添加新連接的客戶端到 playerNames 字典中
            if (!playerNames.ContainsKey(request.ClientNetworkId))
            {
                playerNames.Add(request.ClientNetworkId, playerName);
                CustomLogger.Log(this, $"Added player {playerName} with ID {request.ClientNetworkId} to playerNames dictionary.");
            }

            // 打印更新后的玩家列表
            foreach (var player in playerNames)
            {
                CustomLogger.Log(this, $"Player in server: {player.Value} (ID: {player.Key})");
            }
            UpdateClientPlayerList();
        }
        else
        {
            CustomLogger.LogError(this, $"Room code {roomCode} is invalid. Connection denied.");
            response.Approved = false;
            response.CreatePlayerObject = false;
        }
    }


    private void UpdatePlayerListOnHost(string playerList)
    {
        playerListDisplay.text = playerList;
        CustomLogger.Log(this, $"Host updated player list on client {NetworkManager.Singleton.LocalClientId}.");
    }

    // 生成隨機的四位數房間號碼
    private void GenerateRoomCode()
    {
        currentRoomCode = Random.Range(1000, 9999).ToString();
        openRooms[currentRoomCode] = NetworkManager.Singleton.LocalClientId;  // 將房間號碼與伺服器綁定
        roomCodeDisplay.text = $"Room Code: {currentRoomCode}";  // 顯示給主機玩家
        CustomLogger.Log(this, $"Room {currentRoomCode} created and mapped to Host ID {NetworkManager.Singleton.LocalClientId}.");
    }
}