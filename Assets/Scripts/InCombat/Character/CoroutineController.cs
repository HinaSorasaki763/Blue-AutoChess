using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineController : MonoBehaviour
{
    private CharacterCTRL parent;
    private BarrageObserver currentObserver;
    private float skillDuration;
    private float skillInterval;
    private float bestAngle;
    private float fireAngle;
    private Coroutine shootingCoroutine;
    private bool penetrate; 
    public void OnEnable()
    {
        parent = gameObject.GetComponent<CharacterCTRL>();
    }
    public void StartShootingCoroutine(BarrageObserver observer, float duration, float interval, float angle, int intervalAngle)
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
        }
        bestAngle = angle;
        shootingCoroutine = StartCoroutine(ShootingCoroutine(observer, duration, interval, intervalAngle));
    }
    public void StopShootingCoroutine()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }
    public void SetNextSkill(BarrageObserver observer,
                              float duration,
                              float interval,
                              float angle,
                              int fireAngle,
                              bool penetrate)
    {
        currentObserver = observer;
        skillDuration = duration;
        skillInterval = interval;
        bestAngle = angle;
        this.fireAngle = fireAngle;
        this.penetrate = penetrate;

        PrewarmBullets(observer, duration, interval, observer.IntervalAngle, penetrate);
    }
    private void PrewarmBullets(BarrageObserver observer,
                             float duration,
                             float interval,
                             int angle,
                             bool penetrateBullet)
    {
        int scatter = (int)observer.GetAngle();
        int bulletsPerWave = scatter / angle;
        int waveCount = Mathf.CeilToInt(duration * 2f / interval);
        int totalBullets = bulletsPerWave * waveCount;

        SkillPrefab bulletType = penetrateBullet
            ? SkillPrefab.PenetrateTrailedBullet
            : SkillPrefab.NormalTrailedBullet;

        ResourcePool.Instance.Prewarm(bulletType, totalBullets);
    }

    // 透過動畫事件觸發的公開方法
    public void TriggerSkillShooting()
    {
        CustomLogger.Log(this, $"TriggerSkillShooting()");
        float interval = parent.ActiveSkill.GetCharacterLevel()[parent.star].Data5;
        if (currentObserver != null)
        {
            currentObserver.Character.transform.rotation = Quaternion.Euler(0, bestAngle, 0);
            StartShootingCoroutine(currentObserver, skillDuration, interval, bestAngle, currentObserver.IntervalAngle);
        }
        else
        {
            Debug.LogWarning("No skill set for shooting. Please call SetNextSkill first.");
        }
    }
    public void StartHiyoriShooting(CharacterCTRL parent, int dmg, CharacterCTRL target)
    {
        StartCoroutine(HiyoriShooting(parent, parent.ActiveSkill.GetCharacterLevel()[parent.star].Data1, target));
    }
    private IEnumerator HiyoriShooting(CharacterCTRL parent,int dmg,CharacterCTRL target)
    {

        for (int i = 0; i < 5; i++)
        {
            float interval = 0.2f;
            var bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, parent.FirePoint.position, Quaternion.identity);
            Vector3 v = target.transform.position + (target.transform.position - parent.transform.position).normalized * 10;
            var bulletComponent = bullet.GetComponent<NormalBullet>();
            (bool iscrit, int dmg1) = parent.CalculateCrit(dmg);
            List<HitEffect> hitEffects = new List<HitEffect>() { new HiyoriSkillEffect()};
            bulletComponent.Initialize(dmg1, parent.GetTargetLayer(), parent, 20, target.gameObject, true, iscrit, hitEffects, 20, true, v,false,"HiyoriSkill");
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ShootingCoroutine(BarrageObserver observer, float duration, float interval, int angle)
    {
        float elapsedTime = 0f;
        float scatterAngle = observer.GetAngle();
        int numBullets = (int)scatterAngle / angle;
        while (elapsedTime < duration * 2)
        {
            for (int i = 0; i < numBullets; i++)
            {
                float bulletAngle = bestAngle - (scatterAngle / 2) + i * angle;
                observer.ScatterBulletAtAngle(bulletAngle);
            }
            yield return new WaitForSeconds(interval);

            elapsedTime += interval;
        }
    }
}
