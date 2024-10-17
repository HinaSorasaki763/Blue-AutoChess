using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShizukoActiveSkill : MonoBehaviour
{
    public GameObject TruckPrefab;
    public GameObject Reference;
    public void Start()
    {
        Debug.Log("");
    }
    public void Update()
    {
        
    }
    public void SpawnTruck(Vector3 v)
    {
        Reference = Instantiate(TruckPrefab,v, Quaternion.identity);
    }
}
