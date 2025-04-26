using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Editor_CharacterButton : MonoBehaviour, IPointerClickHandler
{
    public Character characterData;  // �o�ӫ��s�������쨤��
    [SerializeField] public Image characterImage;

    private void Start()
    {
        // ��l�����
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
        // �i���޲z���A�ڳo��Q��F
        EnemyWaveRuntimeEditor.Instance.OnCharacterButtonClicked(characterData);
    }
}
