using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // �Y�n��IPointerClickHandler

public class Editor_HexTile : MonoBehaviour, IPointerClickHandler
{
    // �N���e��l�̬O���@�쨤�� (�i�H�O Character, �Φs CharacterID)
    public Character occupant;
    public string occupantName;

    // �Ψ���ܨ����Y���� UI (�Ҧp���b�P�@��GameObject�U)
    public Sprite HexagonSprite;
    public int index;
    [SerializeField] private Image occupantImage;
    public void Awake()
    {
        occupantImage = GetComponent<Image>();
        index = int.Parse(name)+32;
        
    }
    public void Start()
    {
        EnemyWaveRuntimeEditor.Instance.editor_HexTiles.Add(this);
    }
    // HexTile �Q�I����
    public void OnPointerClick(PointerEventData eventData)
    {
        // �Y�w������ �� �I���@���N�M��
        if (occupant != null)
        {
            ClearOccupant();
        }
        else
        {
            // �i���u�޲z���v����ثe�O�Ū��A���޲z���O���
            EnemyWaveRuntimeEditor.Instance.OnHexTileClicked(this);
        }
    }

    // �]�m�@�쨤��
    public void SetOccupant(Character newOccupant)
    {
        occupant = newOccupant;
        occupantName = newOccupant.CharacterName;
        occupantImage.rectTransform.localEulerAngles = new Vector3(0, 0, 0);
        if (occupantImage != null)
        {
            occupantImage.sprite = (newOccupant != null && newOccupant.Sprite != null)
                ? newOccupant.Sprite
                : null;
            occupantImage.color = (newOccupant != null) ? Color.white : Color.clear;
        }
    }
    public void ClearOccupant()
    {
        occupant = null;

        occupantName = "";
        if (occupantImage != null)
        {
            occupantImage.sprite = HexagonSprite;
            occupantImage.color = Color.white;
            occupantImage.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
        }
    }
}
