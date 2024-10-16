using UnityEngine;

public class WallFade : MonoBehaviour
{
    public Material wallMaterial;
    public float fadeHeight;

    private void Start()
    {
        // ³]¸m§÷½èªº FadeHeight
        wallMaterial.SetFloat("_FadeHeight", fadeHeight);
    }
}
