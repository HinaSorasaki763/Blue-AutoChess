using Firebase;
using Firebase.Firestore;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AuthUIController : MonoBehaviour
{
    public TMP_InputField idInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;   // �s�W
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
            Debug.Log($"�۰ʵn�J�G{PlayerSession.Instance.Data.Name} ({PlayerSession.Instance.Data.Uid})");
            Panel.SetActive(false);
        }
        DontDestroyOnLoad(this);
    }
    private void OnEnable()
    {
        if (PlayerSession.Instance != null && PlayerSession.Instance.Data != null)
        {
            messageText.text = $"�w�n�J:{PlayerSession.Instance.Data.Name}";
        }
    }

    public async void OnLoginButtonClicked()
    {
        messageText.text = "�n�J��...";
        string fakeEmail = ToFakeEmail(idInput.text);
        string password = passwordInput.text.TrimEnd();
        await HandleAuthResult(authManager.Login(fakeEmail, password));
    }

    public async void OnRegisterButtonClicked()
    {
        messageText.text = "���U��...";
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

            userIdText.text = $"�W��: {playerName}";

            // �g�J���� Session
            PlayerSession.Instance.SetData(uid, playerName);
        }
        catch (System.Exception e)
        {
            userIdText.text = $"UID: {uid} (�W��Ū������)";
            Debug.LogError($"Ū���W�٥���: {e.Message}");
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
                // ���\
                ShowLoggedInUI(authManager.CurrentUser.UserId);
            }
            else
            {
                messageText.text = "�������~�A�L�k�n�J�C";
            }
        }
        catch (FirebaseException fe)
        {
            // Firebase ���ѰT��
            messageText.text = $"���~: {fe.Message}";
        }
        catch (System.Exception e)
        {
            // ��L���~
            messageText.text = $"�t�ο��~: {e.Message}";
        }
    }

    private void ShowLoginUI()
    {
        messageText.text = "";
    }
}
