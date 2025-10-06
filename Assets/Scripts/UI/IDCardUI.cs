// IDCardUI.cs
using Firebase.Firestore;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IDCardUI : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text idText;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image favoriteCharacterImage;

    [Header("Edit Popup")]
    [SerializeField] GameObject editPopup;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] Transform backgroundsContent;
    [SerializeField] Transform charactersContent;
    [SerializeField] ScrollRect backgroundsScroll;
    [SerializeField] ScrollRect charactersScroll;

    [Header("Catalogs")]
    [SerializeField] BackgroundCatalog backgroundCatalog;
    [SerializeField] CharacterCatalog characterCatalog;

    PlayerProfile profile;
    readonly List<Button> bgButtons = new();
    readonly List<Button> charButtons = new();
    string selectedBackgroundId;
    string selectedCharacterId;
    FirebaseFirestore db;
    bool isUploading = false; // 在 class 層級宣告
    [SerializeField] Button UploadNameButton;
    void Awake()
    {
        if (profile == null) profile = PlayerProfile.LoadOrCreate();
        SyncView();
        BuildSelectors();
    }
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        UploadNameButton.onClick.AddListener(OnUpdateNameButtonClicked);
    }
    void SyncView()
    {
        nameText.text = profile.PlayerName;
        idText.text = profile.PlayerId;
        backgroundImage.sprite = backgroundCatalog.GetSprite(profile.FavoriteBackgroundId);
        favoriteCharacterImage.sprite = characterCatalog.GetSprite(profile.FavoriteCharacterId);
    }

    void BuildSelectors()
    {
        foreach (Transform t in backgroundsContent) Destroy(t.gameObject);
        foreach (Transform t in charactersContent) Destroy(t.gameObject);
        bgButtons.Clear();
        charButtons.Clear();

        foreach (var e in backgroundCatalog.Entries)
        {
            Button b = null;
            b = CreateThumbButton(e.sprite, backgroundsContent, () =>
            {
                selectedBackgroundId = e.id;
                Highlight(bgButtons, b);
            });
            bgButtons.Add(b);
        }

        foreach (var e in characterCatalog.Entries)
        {
            Button b = null;
            b = CreateThumbButton(e.sprite, charactersContent, () =>
            {
                selectedCharacterId = e.id;
                Highlight(charButtons, b);
            });
            charButtons.Add(b);
        }

        selectedBackgroundId = profile.FavoriteBackgroundId;
        selectedCharacterId = profile.FavoriteCharacterId;
        Preselect(bgButtons, backgroundCatalog.IndexOf(selectedBackgroundId));
        Preselect(charButtons, characterCatalog.IndexOf(selectedCharacterId));
        Canvas.ForceUpdateCanvases();
        backgroundsScroll.verticalNormalizedPosition = 1f;
        charactersScroll.verticalNormalizedPosition = 1f;
    }
    public async void OnUpdateNameButtonClicked()
    {
        if (isUploading) return; // 防重複點擊
        isUploading = true;
        UploadNameButton.interactable = false;
        string newName = nameInput.text.Trim();

        if (!IsValidName(newName))
        {
            Debug.LogWarning("名稱包含非法字元或為空，操作取消。");
            UploadNameButton.interactable = true;
            isUploading = false;
            return;
        }

        var session = PlayerSession.Instance?.Data;
        if (session == null || string.IsNullOrEmpty(session.Uid))
        {
            Debug.LogError("未登入或 UID 無效，無法更新名稱。");
            UploadNameButton.interactable = true;
            isUploading = false;
            return;
        }

        string uid = session.Uid;
        DocumentReference docRef = db.Collection("users").Document(uid);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.LogError($"找不到 UID={uid} 的使用者資料。");
            UploadNameButton.interactable = true;
            isUploading = false;
            return;
        }

        try
        {
            await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "name", newName }
        });

            Debug.Log($"Firebase 名稱已更新: {newName}");

            PlayerSession.Instance.SetData(uid, newName);
            profile.PlayerName = newName;
            PlayerProfile.Save(profile);
            SyncView();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"名稱更新失敗: {e.Message}");
        }

        // 重新啟用按鈕
        UploadNameButton.interactable = true;
        isUploading = false;
    }
    // 名稱檢查
    bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 20) return false;
        var regex = new Regex(@"^[\u4e00-\u9fa5A-Za-z0-9_]+$");
        return regex.IsMatch(name);
    }
    Button CreateThumbButton(Sprite s, Transform parent, System.Action onClick)
    {
        var go = new GameObject("Thumb", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(96, 96);
        var img = go.GetComponent<Image>();
        img.sprite = s;
        img.preserveAspect = true;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        var colors = btn.colors;
        colors.selectedColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        return btn;
    }

    void Highlight(List<Button> list, Button chosen)
    {
        foreach (var b in list) b.interactable = true;
        chosen.interactable = false;
    }

    void Preselect(List<Button> list, int index)
    {
        if (index < 0 || index >= list.Count) return;
        Highlight(list, list[index]);
    }


    public void OpenEdit()
    {
        nameInput.text = profile.PlayerName;
        selectedBackgroundId = profile.FavoriteBackgroundId;
        selectedCharacterId = profile.FavoriteCharacterId;
        BuildSelectors();
        editPopup.SetActive(true);
    }

    public void ConfirmEdit()
    {
        profile.PlayerName = nameInput.text.Trim();
        if (!string.IsNullOrEmpty(selectedBackgroundId)) profile.FavoriteBackgroundId = selectedBackgroundId;
        if (!string.IsNullOrEmpty(selectedCharacterId)) profile.FavoriteCharacterId = selectedCharacterId;
        PlayerProfile.Save(profile);
        SyncView();
        editPopup.SetActive(false);
    }

    public void CancelEdit()
    {
        editPopup.SetActive(false);
    }

    public void CopyId()
    {
        GUIUtility.systemCopyBuffer = profile.PlayerId;
    }
}
