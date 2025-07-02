using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shiroko_Canva : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public static Shiroko_Canva Instance = new Shiroko_Canva();
    public GameObject parent;
    private CharacterCTRL reserving;
    public void Awake()
    {
        Instance = this;
    }
    public void Reserving(CharacterCTRL c)
    {
        if (parent != null) return;
        reserving = c;
    }
    public void Release()
    {
        reserving = null;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
