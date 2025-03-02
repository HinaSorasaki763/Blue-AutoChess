using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

public class HealPack : MonoBehaviour
{
    public HexNode TargetHex;
    private int Range;
    private int HealAmount;
    private CharacterCTRL parent;
    private float fallSpeed = 10.0f; // ¤U¼Y³t«×
    private bool isAlly;
    private bool stop;
    public void InitStats(HexNode targetHex, int range, int healAmount,CharacterCTRL parent,bool Ally)
    {
        TargetHex = targetHex;
        Range = range;
        HealAmount = healAmount;
        this.parent = parent;
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

        HashSet<CharacterCTRL> c = SpawnGrid.Instance.GetCharactersWithinRadius(TargetHex,isAlly,Range,false, parent).ToHashSet();
        StringBuilder sb = new StringBuilder();
        foreach (var item in c)
        {
            sb.AppendLine(item.name);
        }
        CustomLogger.Log(this, $"HealPack Heal {sb.ToString()}");
        foreach (var item in c)
        {
            CustomLogger.Log(this, $"{parent.name} Heal {item.name} for {HealAmount}");
            item.Heal(HealAmount, parent);
        }
        Return();
    }
    public void Return()
    {
        stop = false;
        parent = null;
        HealAmount = 0;
        Range = 0;
        gameObject.SetActive(false);
        
    }
}
