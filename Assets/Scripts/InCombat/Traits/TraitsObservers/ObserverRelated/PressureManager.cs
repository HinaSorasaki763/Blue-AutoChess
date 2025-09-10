using UnityEngine;

public class PressureManager : MonoBehaviour
{
    public static PressureManager Instance { get; private set; }
    public int CurrentPressure { get; private set; }
    public GameObject PressureIndicator;
    public TraitsText TraitText;
    public int SRT_GehennaStack;
    public bool Augment109Triggered = false;
    public bool Augment109CondMatch = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CurrentPressure = 100;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Update()
    {

    }
    public int GetPressure(bool isAlly)
    {
        if (isAlly) return CurrentPressure;
        else return EnemyTraitRelated.Instance.GetPressure();

    }
    public void AddPressure(int amount)
    {
        CurrentPressure += amount;
        SRT_GehennaStack += amount;
        if (SelectedAugments.Instance.CheckAugmetExist(111))
        {
            if (!SelectedAugments.Instance.CheckIfConditionMatch(111))
            {
                DataStackManager.Instance.AddDataStack(CurrentPressure);
                return;
            }
        }
        if (SelectedAugments.Instance.CheckAugmetExist(112) && SRT_GehennaStack >= 100)
        {
            SRT_GehennaStack -= 100;
            int rand = Random.Range(0, 5);
            SRTManager.instance.AddSRT_GehennaStat(rand);
        }
        UpdateIndicater();
    }

    public void ResetPressure()
    {
        CurrentPressure = 0;
    }
    public void UpdateIndicater()
    {
        if (GameEnum.Utility.GetInBattleCharactersWithTrait(GameEnum.Traits.Gehenna, true).Count > 0)
        {
            PressureIndicator.SetActive(true);
            TraitText.gameObject.SetActive(true);
            TraitText.TextMesh.text = $"Pressure =  {GetPressure(true)}";
        }
        else
        {
            TraitText.TextMesh.text = string.Empty;
            PressureIndicator.SetActive(false);

        }
    }
    public void TriggerAugment109Negative()
    {

    }
    public void HandleAugment109()
    {
        if (!SelectedAugments.Instance.CheckAugmetExist(109)) return;
        if (!Augment109CondMatch && !Augment109Triggered)
        {
            Augment109Triggered = true;
            SelectedAugments.Instance.TriggerAugment(109);
        }
    }
}
