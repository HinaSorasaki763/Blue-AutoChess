// PressureManager.cs
using UnityEngine;

public class PressureManager : MonoBehaviour
{
    public static PressureManager Instance { get; private set; }
    public int CurrentPressure { get; private set; }
    public GameObject PressureIndicator;
    public TraitsText TraitText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CurrentPressure = 0;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Update()
    {
        UpdateIndicater();
    }
    public int GetPressure()
    {
        return CurrentPressure;
    }
    public void IncreasePressure(int amount)
    {
        CurrentPressure += amount;
        Debug.Log($"[PressureManager] 威压层数增加 {amount}，当前总层数：{CurrentPressure}");
    }

    public void ResetPressure()
    {
        CurrentPressure = 0;
    }
    public void UpdateIndicater()
    {
        if (TraitsEffectManager.Instance.GetTraitLevelForCharacter(GameEnum.Traits.Gehenna, false)> 0)
        {
            PressureIndicator.SetActive(true);
            TraitText.gameObject.SetActive(true);
            TraitText.TextMesh.text = $"Pressure =  {GetPressure()}";
        }
        else
        {
            TraitText.TextMesh.text = string.Empty;
            PressureIndicator.SetActive(false);

        }
    }
}
