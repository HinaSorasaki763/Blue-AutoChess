using GameEnum;
using UnityEngine;

public class CharacterBars : MonoBehaviour
{
    public Transform Parent;
    private CharacterCTRL CharacterCTRL;
    private Camera cam;

    public SliderCTRL HealthSlider;
    public SliderCTRL ManaSlider;
    public TMPro.TextMeshProUGUI CurrentState;

    // �s�W�� UI ����
    public GameObject strongestMark;  // "�̱j"�лx
    public TMPro.TextMeshProUGUI starLevelText;  // ��ܬP�Ū��e���

    private void OnEnable()
    {
        cam = Camera.main;
    }

    private void OnDisable()
    {
       // ResetBars();
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
        strongestMark.SetActive(false);  // ���m������"�̱j"�лx
    }

    void InitBars()
    {
        HealthSlider.SetMaxValue(CharacterCTRL.GetStat(StatsType.Health));
        CustomLogger.Log(this,$"set character {CharacterCTRL.name} max health = {CharacterCTRL.GetStat(StatsType.Health)}, max = {HealthSlider.sliderComponent.maxValue}");
        HealthSlider.SetMinValue(0);
        ManaSlider.SetMaxValue(CharacterCTRL.GetStat(StatsType.MaxMana));
        ManaSlider.SetMinValue(0);

        UpdateStarLevel();
        strongestMark.SetActive(false);  // �q�{���p�U����
    }

    void UpdateUIs()
    {
        HealthSlider.UpdateValue(CharacterCTRL.GetStat(StatsType.currHealth));
        ManaSlider.UpdateValue(CharacterCTRL.GetStat(StatsType.Mana));
    }

    // ��s�P�żлx
    public void UpdateStarLevel()
    {
        starLevelText.text = $"star: {CharacterCTRL.star}";  // ��ܬP��
    }

    // ��s"�̱j"�лx
    public void SetStrongestMark(bool isStrongest)
    {
        strongestMark.SetActive(isStrongest);
    }
}
