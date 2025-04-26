using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTest_ : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResourcePool.Instance.GetGoldPrefab(Vector3.zero);
            ResourcePool.Instance.GetRandRewardPrefab(Vector3.zero);
        }
    }
}
