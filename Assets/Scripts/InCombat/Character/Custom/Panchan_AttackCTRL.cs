using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panchan_AttackCTRL : MonoBehaviour
{
    public CharacterCTRL parent;
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Die()
    {
        parent.gameObject.SetActive(false);
    }
    public void Attack()
    {
        parent.Attack();
    }

}
