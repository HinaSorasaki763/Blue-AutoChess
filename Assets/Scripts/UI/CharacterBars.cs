using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterBars : MonoBehaviour
{
    public Transform Parent;
    private CharacterCTRL CharacterCTRL;
    private Camera cam;

    public SliderCTRL HealthSlider;
    public SliderCTRL ManaSlider;
    public TMPro.TextMeshProUGUI CurrentState;

    // 新增的 UI 元素
    public GameObject strongestMark;  // "最強"標誌
    public TMPro.TextMeshProUGUI starLevelText;  // 顯示星級的占位符

    public Transform equipmentDisplayArea; // 用于显示装备图标的区域
    public GameObject equipmentIconPrefab; // 装备图标的预制体
    public List<Image> equipmentImages = new List<Image>(); 
    private void OnEnable()
    {
        cam = Camera.main;
    }

    private void OnDisable()
    {

    }

    public void UpdateText( string currentState)
    {
        CurrentState.text = currentState;
    }


    void Update()
    {
        if (Parent == null || !Parent.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }
        Vector3 screenPos = cam.WorldToScreenPoint(Parent.position);
        transform.position = screenPos + new Vector3(0, 120, 0);
        UpdateUIs();
    }
    public void UpdateEquipmentDisplay(List<IEquipment> equippedItems)
    {

        for (int i = 0; i < equippedItems.Count; i++)
        {
            Image iconImage = equipmentImages[i];
            iconImage.sprite = equippedItems[i].Icon;
        }
        for (int i = equippedItems.Count; i < 3; i++)
        {
            Image iconImage = equipmentImages[i];
            iconImage.sprite = null;
        }
    }
    public void SetBarsParent(Transform parent)
    {
        CustomLogger.Log(this, $"set bar to {parent.name}");
        Parent = parent;
        CharacterCTRL = Parent.GetComponent<CharacterCTRL>();
        CharacterCTRL.SetBarChild(this);
        InitBars();
    }

    void ResetBars()
    {
        HealthSlider.SetMaxValue(1);
        HealthSlider.SetMinValue(0);
        ManaSlider.SetMaxValue(1);
        ManaSlider.SetMinValue(0);
        strongestMark.SetActive(false);  // 重置時隱藏"最強"標誌
    }

    public void InitBars()
    {
        HealthSlider.SetMaxValue(CharacterCTRL.GetStat(StatsType.Health));
        CustomLogger.Log(this,$"set character {CharacterCTRL.name} max health = {CharacterCTRL.GetStat(StatsType.Health)}, max = {HealthSlider.sliderComponent.maxValue}");
        HealthSlider.SetMinValue(0);
        ManaSlider.SetMaxValue(CharacterCTRL.GetStat(StatsType.MaxMana));
        ManaSlider.SetMinValue(0);

        UpdateStarLevel();
        strongestMark.SetActive(false);  // 默認情況下隱藏
    }

    void UpdateUIs()
    {
        HealthSlider.UpdateValue(CharacterCTRL.GetStat(StatsType.currHealth));
        ManaSlider.UpdateValue(CharacterCTRL.GetStat(StatsType.Mana));
    }

    // 更新星級標誌
    public void UpdateStarLevel()
    {
        starLevelText.text = $"star: {CharacterCTRL.star}";  // 顯示星級
    }

    // 更新"最強"標誌
    public void SetStrongestMark(bool isStrongest)
    {
        strongestMark.SetActive(isStrongest);
    }
}
