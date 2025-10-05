// IDCardUI.cs
using System.Collections.Generic;
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

    void Awake()
    {
        if (profile == null) profile = PlayerProfile.LoadOrCreate();
        SyncView();
        BuildSelectors();
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
