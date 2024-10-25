using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    public HexNode TargetHex;
    private int Range;
    private int HealAmount;
    private CharacterCTRL characterCTRL;
    private float fallSpeed = 10.0f; // ¤U¼Y³t«×
    private bool isAlly;
    private bool stop;
    public void InitStats(HexNode targetHex, int range, int healAmount,CharacterCTRL character,bool Ally)
    {
        TargetHex = targetHex;
        Range = range;
        HealAmount = healAmount;
        characterCTRL = character;
        isAlly = Ally;
    }
    public void Start()
    {
        
    }
    public void Update()
    {
        if (!stop)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }

        if (transform.position.y<=0.05f)
        {
            stop= true;
            Heal();
        }
    }
    public void Heal()
    {
        CustomLogger.Log(this, $"Heal");
        List<CharacterCTRL> c = SpawnGrid.Instance.GetCharactersWithinRadius(TargetHex,isAlly,Range,false,characterCTRL);
        foreach (var item in c)
        {
            item.Heal(HealAmount,characterCTRL);
        }
        Return();
    }
    public void Return()
    {
        stop = false;
        characterCTRL = null;
        HealAmount = 0;
        Range = 0;
        gameObject.SetActive(false);
        
    }
}
