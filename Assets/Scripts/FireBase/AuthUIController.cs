using Firebase;
using Firebase.Firestore;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AuthUIController : MonoBehaviour
{
    public TMP_InputField idInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;   // 新增
    public GameObject Panel;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI userIdText;

    FirebaseAuthManager authManager;

    void Start()
    {
        authManager = FindObjectOfType<FirebaseAuthManager>();
        ShowLoginUI();
        PlayerSession.Instance.LoadLocal();
        if (PlayerSession.Instance.Data != null)
        {
            Debug.Log($"自動登入：{PlayerSession.Instance.Data.Name} ({PlayerSession.Instance.Data.Uid})");
            Panel.SetActive(false);
        }
        DontDestroyOnLoad(this);
    }
    private void OnEnable()
    {
        if (PlayerSession.Instance != null && PlayerSession.Instance.Data != null)
        {
            messageText.text = $"已登入:{PlayerSession.Instance.Data.Name}";
        }
    }

    public async void OnLoginButtonClicked()
    {
        messageText.text = "登入中...";
        string fakeEmail = ToFakeEmail(idInput.text);
        string password = passwordInput.text.TrimEnd();
        await HandleAuthResult(authManager.Login(fakeEmail, password));
    }

    public async void OnRegisterButtonClicked()
    {
        messageText.text = "註冊中...";
        string fakeEmail = ToFakeEmail(idInput.text);
        string password = passwordInput.text.TrimEnd();
        string name = nameInput.text.Trim();

        await HandleAuthResult(authManager.Register(fakeEmail, password, name));
    }

    private async void ShowLoggedInUI(string uid)
    {
        try
        {
            var snapshot = await FirebaseFirestore.DefaultInstance
                .Collection("users")
                .Document(uid)
                .GetSnapshotAsync();

            string playerName = snapshot.Exists && snapshot.ContainsField("name")
                ? snapshot.GetValue<string>("name")
                : uid;

            userIdText.text = $"名稱: {playerName}";

            // 寫入全域 Session
            PlayerSession.Instance.SetData(uid, playerName);
        }
        catch (System.Exception e)
        {
            userIdText.text = $"UID: {uid} (名稱讀取失敗)";
            Debug.LogError($"讀取名稱失敗: {e.Message}");
        }
    }


    private string ToFakeEmail(string id)
    {
        return id.TrimEnd() + "@blueautochess.com";
    }

    public void OnLogoutButtonClicked()
    {
        authManager.Logout();
        ShowLoginUI();
    }

    private async Task HandleAuthResult(Task task)
    {
        try
        {
            await task;
            if (authManager.CurrentUser != null)
            {
                // 成功
                ShowLoggedInUI(authManager.CurrentUser.UserId);
            }
            else
            {
                messageText.text = "未知錯誤，無法登入。";
            }
        }
        catch (FirebaseException fe)
        {
            // Firebase 失敗訊息
            messageText.text = $"錯誤: {fe.Message}";
        }
        catch (System.Exception e)
        {
            // 其他錯誤
            messageText.text = $"系統錯誤: {e.Message}";
        }
    }

    private void ShowLoginUI()
    {
        messageText.text = "";
    }
}
