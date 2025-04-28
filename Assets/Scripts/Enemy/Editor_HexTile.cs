using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Editor_HexTile : MonoBehaviour, IPointerClickHandler
{
    public Character occupant;
    public string occupantName;
    public Sprite HexagonSprite;
    public int index;
    [SerializeField] private Image occupantImage;
    public int editedLevel = 1;
    public int[] editedEquipmentIDs = new int[3] { -1, -1, -1 };
    public Image[] equipmentImages = new Image[3];

    public void Awake()
    {
        occupantImage = GetComponent<Image>();
        index = int.Parse(name) + 32;

        // 找到名字以 "equipments" 開頭的子物件
        Transform equipmentRoot = null;
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("equipments"))
            {
                equipmentRoot = child;
                break;
            }
        }

        if (equipmentRoot != null)
        {
            Transform panel = equipmentRoot.Find("Panel");
            if (panel != null)
            {
                int count = 0;
                foreach (Transform imgChild in panel)
                {
                    if (count >= 3) break;
                    Image img = imgChild.GetComponent<Image>();
                    if (img != null)
                    {
                        equipmentImages[count] = img;
                        count++;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Editor_HexTile] 找到equipments但裡面沒有Panel");
            }
        }
        else
        {
            Debug.LogWarning($"[Editor_HexTile] 找不到以'equipments'開頭的子物件！");
        }
    }


    public void Start()
    {
        EnemyWaveRuntimeEditor.Instance.editor_HexTiles.Add(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (occupant != null)
        {
            CharacterDetailPanel.Instance.Open(this);
            EnemyWaveRuntimeEditor.Instance.OnHexTileClicked(this);
        }
        else
        {
            EnemyWaveRuntimeEditor.Instance.OnHexTileClicked(this);
        }
    }

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

        editedLevel = 1;
        editedEquipmentIDs = new int[3] { -1, -1, -1 };
    }

    public void ClearOccupant()
    {
        occupant = null;
        occupantName = "";
        if (occupantImage != null)
        {
            occupantImage.sprite = HexagonSprite;
            occupantImage.color = Color.white;
        }

        editedLevel = 1;
        editedEquipmentIDs = new int[3] { -1, -1, -1 };
        foreach (var item in equipmentImages)
        {
            item.sprite = null;
        }
    }
}
