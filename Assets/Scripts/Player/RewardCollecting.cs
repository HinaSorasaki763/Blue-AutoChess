using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCollecting : MonoBehaviour
{
    public static RewardCollecting instance;
    public void Awake()
    {
        instance = this;
    }

}
