using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    public List<Image> traitIcon = new();
    public Image AcademyIcon;
    public void Start()
    {
        
    }
    public void SetImagesNull()
    {
        foreach (Image image in traitIcon)
        {
            image.sprite = null;
            image.color = new Color(1, 1, 1, 0);
        }
        AcademyIcon.sprite = null;
        AcademyIcon.color = new Color(1, 1, 1, 0);
    }
}
