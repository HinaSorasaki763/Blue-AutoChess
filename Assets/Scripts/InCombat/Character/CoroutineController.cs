using System.Collections;
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
    public void SetNextSkill(BarrageObserver observer, float duration, float interval, float angle, float fireAngle)
    {
        currentObserver = observer;
        skillDuration = duration;
        skillInterval = interval;
        bestAngle = angle;
        this.fireAngle = fireAngle;
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

    private IEnumerator ShootingCoroutine(BarrageObserver observer, float duration, float interval, int angle)
    {
        float elapsedTime = 0f;
        float scatterAngle = observer.GetAngle();
        int numBullets = (int)scatterAngle / angle;
        while (elapsedTime < duration * 2)
        {
            // 從最佳角度開始，逐步射出一整波子彈
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
