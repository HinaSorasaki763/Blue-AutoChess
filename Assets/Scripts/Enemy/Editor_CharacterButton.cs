using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Editor_CharacterButton : MonoBehaviour, IPointerClickHandler
{
    public Character characterData;  // 這個按鈕對應哪位角色
    [SerializeField] public Image characterImage;

    private void Start()
    {
        // 初始化顯示
        if (characterData != null && characterData.Sprite != null)
        {
            characterImage.sprite = characterData.Sprite;
            characterImage.color = Color.white;
        }
        else
        {
            characterImage.sprite = null;
            characterImage.color = Color.clear;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 告知管理器，我這邊被選了
        EnemyWaveRuntimeEditor.Instance.OnCharacterButtonClicked(characterData);
    }
}
