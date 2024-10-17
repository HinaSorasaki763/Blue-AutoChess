using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    public HexNode TargetHex;
    private int Range;
    private int HealAmount;
    private CharacterCTRL characterCTRL;
    public void InitStats(HexNode targetHex, int range, int healAmount,CharacterCTRL character)
    {
        TargetHex = targetHex;
        Range = range;
        HealAmount = healAmount;
        characterCTRL = character;
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
                item.OccupyingCharacter.Heal(HealAmount,characterCTRL);
            }
        }

    }
    public void Return()
    {
        characterCTRL = null;
        HealAmount = 0;
        Range = 0;
        gameObject.SetActive(false);
    }
}
