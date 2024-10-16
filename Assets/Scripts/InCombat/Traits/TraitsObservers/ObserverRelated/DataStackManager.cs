// DataLayerManager.cs
using UnityEngine;

public class DataStackManager : MonoBehaviour
{
    public static DataStackManager Instance { get; private set; }
    public int CurrentDataStack { get; private set; }
    public GameObject MillienumIndicator;
    public TraitsText TraitText;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CurrentDataStack = 0;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Update()
    {
        UpdateIndicator();
    }
    public void IncreaseDataLayer(int amount)
    {
        CurrentDataStack += amount;
        // 可以在这里添加数据层数变化的逻辑，例如更新UI
    }
    public int GetData()
    {
        return CurrentDataStack;
    }
    public void ResetDataLayer()
    {
        CurrentDataStack = 0;
    }
    public void UpdateIndicator()
    {
        int data = TraitsEffectManager.Instance.GetTraitLevelForCharacter(GameEnum.Traits.Millennium, false);
        if (data > 0)
        {
            MillienumIndicator.SetActive(true);
            TraitText.gameObject.SetActive(true);
            TraitText.TextMesh.text = $"Data : {GetData()}";
        }
        else
        {
            TraitText.TextMesh.text = string.Empty;
            MillienumIndicator.SetActive(false);
        }
    }
}
