using System.Collections;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool IsAllyWall { get; private set; }
    public CharacterCTRL SourceCharacter { get; private set; }
    public float resistPercent;
    public int life;
    public void InitWall(CharacterCTRL parent, bool isAllyWall, int resistance)
    {
        life = 3;
        IsAllyWall = isAllyWall;
        SourceCharacter = parent;
        resistPercent = resistance;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material tileMaterial = renderer.material;
            tileMaterial.color = isAllyWall ? Color.blue : Color.red;
            renderer.material = tileMaterial;
        }
    }
    public void StartWallLifetime(string wallKey, bool isAllyWall, float duration = 5f)
    {
        StartCoroutine(WallLifetimeCoroutine(wallKey, isAllyWall, duration));
    }

    private IEnumerator WallLifetimeCoroutine(string wallKey, bool isAllyWall, float duration)
    {
        yield return new WaitForSeconds(duration);
        resistPercent = 0;
        life = 0;
        gameObject.SetActive(false);
        Debug.Log($"Wall with key {wallKey} has been destroyed after {duration} seconds.");
    }
    public void GetHit(CharacterCTRL source, int dmg, bool iscrit)
    {
        dmg = (int)(dmg * resistPercent * 0.01f);
        life--;
        SourceCharacter.GetHit(dmg, source, $"Wall block from {source}", iscrit);
        if (life < 0) gameObject.SetActive(false);
    }
    public float GetResist()
    {
        return resistPercent;
    }
}
