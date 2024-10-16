using GameEnum;
using System.Collections;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool IsAllyWall { get; private set; }
    public CharacterCTRL SourceCharacter { get; private set; }
    public void SetWallType(bool isAllyWall)
    {
        IsAllyWall = isAllyWall;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material tileMaterial = renderer.material;
            tileMaterial.color = isAllyWall ? Color.blue : Color.red;
            renderer.material = tileMaterial; 
        }
    }
    public void SetSourceCharacter(CharacterCTRL sourceCharacter)
    {
        SourceCharacter = sourceCharacter;
    }
    public void StartWallLifetime(string wallKey, bool isAllyWall, float duration = 5f)
    {
        StartCoroutine(WallLifetimeCoroutine(wallKey, isAllyWall, duration));
    }

    private IEnumerator WallLifetimeCoroutine(string wallKey, bool isAllyWall, float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        Debug.Log($"Wall with key {wallKey} has been destroyed after {duration} seconds.");
    }
}
