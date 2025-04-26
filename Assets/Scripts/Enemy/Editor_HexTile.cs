using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 若要用IPointerClickHandler

public class Editor_HexTile : MonoBehaviour, IPointerClickHandler
{
    // 代表當前格子裡是哪一位角色 (可以是 Character, 或存 CharacterID)
    public Character occupant;
    public string occupantName;

    // 用來顯示角色頭像的 UI (例如掛在同一個GameObject下)
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
    // HexTile 被點擊時
    public void OnPointerClick(PointerEventData eventData)
    {
        // 若已有角色 → 點擊一次就清空
        if (occupant != null)
        {
            ClearOccupant();
        }
        else
        {
            // 告知「管理器」此格目前是空的，讓管理器記住我
            EnemyWaveRuntimeEditor.Instance.OnHexTileClicked(this);
        }
    }

    // 設置一位角色
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
