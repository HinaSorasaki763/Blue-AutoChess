using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kasumi_DrillCTRL : MonoBehaviour
{
    public GameObject DrillPrefab;
    public GameObject DrillRef;
    public void Start()
    {
        DrillRef = Instantiate(DrillPrefab);
        DrillRef.transform.position = new Vector3(50, 50, 50);
    }
    public void GetDrill(Vector3 drillPos)
    {
        DrillRef.GetComponent<Kasumi_Drill>().StartDrop(drillPos);
    }
}
