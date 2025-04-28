using UnityEngine;

public class EnemyTraitRelated : MonoBehaviour
{
    public static EnemyTraitRelated Instance;
    private int PressureStack = 0;
    public void OnEnable()
    {
        Instance = this;
    }
    public int GetPressure()
    {
        return PressureStack;
    }
    public void AddPressure(int amount)
    {
        PressureStack += amount;
    }
    public void SetPressure(int stack)
    {
        PressureStack = stack;
    }
}
