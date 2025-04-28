using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameEnum;
using System.Linq;
using System.Text;

public class CharacterDetailPanel : MonoBehaviour
{
    public static CharacterDetailPanel Instance { get; private set; }

    public GameObject panelRoot;
    public Editor_HexTile currentTile;
    public Image Protrait;

    [Header("Star Selection")]
    public List<Button> starButtons;
    public List<Image> starButtonImages;
    private int selectedStar = 1;

    [Header("Equipment Slots")]
    public List<Image> equipmentSlots;
    public List<Image> equipmentBgImage;
    public List<Button> equipmentButtons;
    private int selectedEquipSlotIndex = -1;

    [Header("Equipment Selection Area")]
    public GameObject equipmentSelectionArea;
    public Transform equipmentButtonParent;
    public GameObject equipmentButtonPrefab;

    private void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
        equipmentSelectionArea.SetActive(true);
        for (int i = 0; i < starButtons.Count; i++)
        {
            int index = i;
            starButtons[i].onClick.AddListener(() => OnStarButtonClicked(index + 1));
        }
        for (int i = 0; i < equipmentButtons.Count; i++)
        {
            int index = i;
            equipmentButtons[i].onClick.AddListener(() => OnEquipmentSlotClicked(index));
        }

    }
    public void Init()
    {
        foreach (var equip in EnemyWaveRuntimeEditor.Instance.availableEquipments)
        {
            GameObject buttonObj = Instantiate(equipmentButtonPrefab, equipmentButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            Image image = buttonObj.GetComponent<Image>();

            if (equip.Icon != null)
            {
                image.sprite = equip.Icon;
                image.color = Color.white;
            }

            button.onClick.AddListener(() => { OnEquipmentSelected(equip); });
        }
    }
    public void ClearCurrentTile()
    {
        currentTile.ClearOccupant();
        EnemyWaveRuntimeEditor.Instance.OnHexTileClicked(currentTile);
    }
    public void Open(Editor_HexTile tile)
    {
        if (tile.occupant == null) return;
        if (currentTile == tile)
        {
            panelRoot.SetActive(!panelRoot.activeInHierarchy);
            return;
        }
        currentTile = tile;
        panelRoot.SetActive(true);
        Protrait.sprite = tile.occupant.Sprite;

        LoadCharacterSettings();

        CustomLogger.Log(this, $"打開角色詳細設定面板: {tile.occupantName}");
    }

    public void Close()
    {
        panelRoot.SetActive(false);
        currentTile = null;
    }

    private void LoadCharacterSettings()
    {
        selectedStar = currentTile.editedLevel > 0 ? currentTile.editedLevel : 1;
        UpdateStarButtonUI();

        for (int i = 0; i < 3; i++)
        {
            int equipID = currentTile.editedEquipmentIDs != null && i < currentTile.editedEquipmentIDs.Length
                ? currentTile.editedEquipmentIDs[i]
                : -1;
            if (equipID >= 0)
            {
                IEquipment equipment = GetEquipmentByID(equipID);
                equipmentSlots[i].sprite = equipment.Icon;
                currentTile.equipmentImages[i].sprite = equipment.Icon;
            }
            else
            {
                equipmentSlots[i].sprite = null;
            }
        }
    }
    public IEquipment GetEquipmentByID(int equipmentID)
    {
        IEquipment eq = EnemyWaveRuntimeEditor.Instance.availableEquipments
                          .FirstOrDefault(e => e.Id == equipmentID);
        return eq;
    }

    public void OnStarButtonClicked(int starLevel)
    {
        selectedStar = starLevel;
        UpdateStarButtonUI();

        if (currentTile != null)
        {
            currentTile.editedLevel = selectedStar;
            CustomLogger.Log(this, $"設定 {currentTile.occupantName} 星級為 {selectedStar} 星 (Tile上儲存)");
        }
    }

    private void UpdateStarButtonUI()
    {
        for (int i = 0; i < starButtonImages.Count; i++)
        {
            if (i == selectedStar - 1)
            {
                starButtonImages[i].color = Color.yellow;
            }
            else
            {
                starButtonImages[i].color = Color.white;
            }
        }
    }

    public void OnEquipmentSlotClicked(int slotIndex)
    {
        selectedEquipSlotIndex = slotIndex;
        for (int i = 0; i < 3; i++)
        { 
            if (i == slotIndex)
            {
                equipmentBgImage[slotIndex].color = Color.yellow;
                equipmentSlots[slotIndex].color = Color.clear;
            }
            else
            {
                equipmentBgImage[i].color = Color.white;
                equipmentSlots[i].color = Color.white;
            }
        }
    }

    private void OnEquipmentSelected(IEquipment selectedEquip)
    {
        if (selectedEquipSlotIndex < 0 || selectedEquipSlotIndex >= equipmentSlots.Count) return;

        equipmentSlots[selectedEquipSlotIndex].sprite = selectedEquip.Icon;
        equipmentSlots[selectedEquipSlotIndex].color = Color.white;

        if (currentTile != null)
        {
            currentTile.editedEquipmentIDs[selectedEquipSlotIndex] = selectedEquip.Id;
            currentTile.equipmentImages[selectedEquipSlotIndex].sprite = selectedEquip.Icon;
        }
    }
}
