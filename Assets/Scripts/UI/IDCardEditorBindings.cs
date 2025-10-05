// IDCardEditorBindings.cs
using UnityEngine;

public class IDCardEditorBindings : MonoBehaviour
{
    [SerializeField] IDCardUI ui;
    public void OnOpenEdit() => ui.OpenEdit();
    public void OnConfirmEdit() => ui.ConfirmEdit();
    public void OnCancelEdit() => ui.CancelEdit();
    public void OnCopyId() => ui.CopyId();
}
