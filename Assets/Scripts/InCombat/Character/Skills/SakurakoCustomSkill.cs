using GameEnum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SakurakoCustomSkill : MonoBehaviour
{
    private CharacterCTRL character;
    
    public void Awake()
    {
        character = GetComponent<CharacterCTRL>();
    }
    public void Start()
    {
        
    }
    public void StartShoot()
    {
        StartCoroutine(ShootingBurst());
    }
    public IEnumerator ShootingBurst()
    {
        character.ManaLock = true;
        int totalBullets = 12;
        int totalFrames = 90;
        float interval = (totalFrames / (float)totalBullets) / 30f; 

        for (int i = 0; i < totalBullets; i++)
        {
            character.HandleAttacking(false);
            GameObject bullet = ResourcePool.Instance.SpawnObject(
                SkillPrefab.NormalTrailedBullet,
                character.FirePoint.position,
                Quaternion.identity
            );

            int dmg = character.ActiveSkill.GetAttackCoefficient(character.GetSkillContext());
            (bool iscrit, int dmg1) = character.CalculateCrit(dmg);
            CharacterCTRL enemy = Utility.GetNearestEnemy(character);
            if (!enemy) yield return null;
            character.transform.LookAt(enemy.transform.position);
            bullet.GetComponent<NormalBullet>().Initialize(
                dmg1,
                character.GetTargetLayer(),
                character,
                15f,
                enemy.gameObject,
                false,
                iscrit,
                null,
                20,
                true,
                enemy.transform.position,
                false
            );

            yield return new WaitForSeconds(interval);
        }
    }

}
