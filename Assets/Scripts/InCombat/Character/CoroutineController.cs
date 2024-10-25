using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CoroutineController : MonoBehaviour
{
    private BarrageObserver currentObserver;
    private float skillDuration;
    private float skillInterval;
    private float bestAngle;
    private float fireAngle;
    private Coroutine shootingCoroutine;
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
    public void SetNextSkill(BarrageObserver observer, float duration, float interval, float angle,float fireAngle)
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
        CustomLogger.Log(this,$"TriggerSkillShooting()");
        if (currentObserver != null)
        {
            currentObserver.Character.transform.rotation = Quaternion.Euler(0, bestAngle, 0);
            StartShootingCoroutine(currentObserver, skillDuration, skillInterval, bestAngle,currentObserver.IntervalAngle);
        }
        else
        {
            Debug.LogWarning("No skill set for shooting. Please call SetNextSkill first.");
        }
    }

    private IEnumerator ShootingCoroutine(BarrageObserver observer, float duration, float interval,int angle)
    {
        float elapsedTime = 0f;
        float scatterAngle = observer.GetAngle();
        int numBullets = (int)scatterAngle/ angle;
        while (elapsedTime < duration)
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
    public void StartRemoveCenterPointCoroutine(HexNode centerNode, float delay)
    {
        StartCoroutine(RemoveCenterPointAfterDelay(centerNode, delay));
    }
    public IEnumerator RemoveCenterPointAfterDelay(HexNode centerNode, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveCenterPoint(centerNode);
    }
    public void RemoveCenterPoint(HexNode node)
    {
        if (node.EnemyBlockingZonecenter)
        {
            node.TargetedEnemyzone =false;
            foreach (var neighbor in node.Neighbors)
            {
                neighbor.TargetedEnemyzone = false;
            }
        }
        if (node.AllyBlockingZonecenter)
        {
            node.TargetedAllyZone = false;
            foreach (var neighbor in node.Neighbors)
            {
                neighbor.TargetedAllyZone = false;
            }
        }
        node.AllyBlockingZonecenter = false;
        node.EnemyBlockingZonecenter = false;
        CustomLogger.Log(this, $"removing {node.name} as center , pos int = {node.Position}");
    }
}
