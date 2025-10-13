using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseAuthManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseUser user;
    FirebaseFirestore db;

    public FirebaseUser CurrentUser => user;

    async void Start()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task Register(string email, string password, string playerName = null)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            // �W�ٳB�z
            if (string.IsNullOrEmpty(playerName))
            {
                // �w�]�W�� BlueAutoChessPlayer{n}
                int n = UnityEngine.Random.Range(1000, 9999);
                playerName = $"BlueAutoChessPlayer{n}";
            }

            // �s�i Firestore
            var userData = new Dictionary<string, object>
            {
                { "uid", user.UserId },
                { "name", playerName }
            };
            await db.Collection("users").Document(user.UserId).SetAsync(userData);

            Debug.Log($"���U���\: {user.UserId}, �W��={playerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("���U����: " + e.Message);
            throw;
        }
    }

    public async Task Login(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            Debug.Log("�n�J���\: " + CurrentUser.UserId);
        }
        catch (System.Exception e)
        {
            Debug.LogError("�n�J����: " + e.Message);
            throw;
        }
    }

    public void Logout()
    {
        auth.SignOut();
        PlayerSession.Instance.Clear();
        user = null;
        Debug.Log("�w�n�X");
    }

    public async Task<string> GetPlayerNameById(string uid)
    {
        try
        {
            DocumentSnapshot doc = await FirebaseFirestore.DefaultInstance
                .Collection("users")
                .Document(uid)
                .GetSnapshotAsync();

            if (doc.Exists && doc.ContainsField("name"))
            {
                return doc.GetValue<string>("name");
            }
            else
            {
                Debug.LogWarning($"No name found for UID={uid}");
                return "Unknown Player";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get name for UID={uid}: {e.Message}");
            return "Unknown Player";
        }
    }
}
