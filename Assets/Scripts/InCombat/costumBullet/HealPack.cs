using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    public HexNode TargetHex;
    private int Range;
    private int HealAmount;
    public void InitStats(HexNode targetHex, int range, int healAmount)
    {
        TargetHex = targetHex;
        Range = range;
        HealAmount = healAmount;
    }
    public void Start()
    {
        
    }
    public void Update()
    {
        if ((transform.position-TargetHex.Position).magnitude<0.5)
        {
            StartCoroutine(Heal());
        }
    }
    public IEnumerator Heal()
    {
        Debug.Log($"Heal");
        yield return new WaitForSeconds(1f);
        List<HexNode> nodes = Utility.GetHexInRange(TargetHex,Range);
        foreach (var item in nodes)
        {
            if (item.OccupyingCharacter!= null)
            {
                item.OccupyingCharacter.Heal(HealAmount);
                Debug.Log($"{item.OccupyingCharacter.name} heal {HealAmount}");
            }
        }
        gameObject.SetActive(false);
    }
}
