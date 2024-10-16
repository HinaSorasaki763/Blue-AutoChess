using UnityEngine;

public class MineSkillExplosion : MonoBehaviour
{
    public GameObject ExplosionEffectPrefab;
    public GameObject reference;
    public CharacterCTRL parentCTRL;
    public void Start()
    {
        parentCTRL = GetComponent<CharacterCTRL>();
    }
    public void Update()
    {
        
    }
    public void SpawnEffect()
    {

        reference = Instantiate(ExplosionEffectPrefab,parentCTRL.transform.position,Quaternion.identity);
        reference.SetActive(true);
    }
    public void DisableEffect()
    {
        reference.SetActive(false);
        reference = null;   
    }
}
