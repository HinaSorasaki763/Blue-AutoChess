using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtsukoDroneCTRL : MonoBehaviour
{
    public GameObject droneObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EnableDrone()
    {
        droneObj.SetActive(true);
    }
    public void DisableDrone()
    {
        droneObj.SetActive(false);
    }
}
