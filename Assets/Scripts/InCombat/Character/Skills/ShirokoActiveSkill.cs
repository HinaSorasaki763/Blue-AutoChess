using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ShirokoActiveSkill : MonoBehaviour
{
    public GameObject droneRef;
    public Shiroko_Terror_DroneCTRL droneCTRL;
    public GameObject Drone;
    public void Start()
    {
        
    }
    public GameObject GetDrone(SkillContext skillContext)
    {
        GameObject obj = Instantiate(Drone, skillContext.Parent.transform.position, Quaternion.identity);
        droneRef = obj;
        droneRef.transform.LookAt(skillContext.Parent.Target.transform);
        droneCTRL = obj.GetComponent<Shiroko_Terror_DroneCTRL>();
        droneCTRL.stack++;
        return obj;
    }
}
