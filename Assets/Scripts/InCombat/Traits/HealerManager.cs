using UnityEngine;

public class HealerManager : MonoBehaviour
{
    public static HealerManager instance;
    public int amount;
    public void Awake()
    {
        instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
