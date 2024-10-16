using UnityEngine;

public class WallFade : MonoBehaviour
{
    public Material wallMaterial;
    public float fadeHeight;

    private void Start()
    {
        // �]�m���誺 FadeHeight
        wallMaterial.SetFloat("_FadeHeight", fadeHeight);
    }
}
