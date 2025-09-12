using System.Collections;
using UnityEngine;

public class Kasumi_Drill : MonoBehaviour
{
    [SerializeField] private Transform body;   // Inspector 指定
    [SerializeField] private GameObject child;
    [SerializeField] private float slowSpeed = 0.15f;
    [SerializeField] private float fastSpeed = 15f;

    public void StartDrop(Vector3 target)
    {
        child.SetActive(true);
        transform.position = new Vector3(-3f, 3f, -3f);
        transform.LookAt(target);
        StartCoroutine(DropRoutine(target));
    }

    IEnumerator DropRoutine(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;

        // 1. 快速前進直到接近 target
        while (Vector3.Distance(transform.position, target) > 0.5f)
        {
            transform.position += dir * fastSpeed * Time.deltaTime;
            yield return null;
        }
        transform.position = target; // 修正到精準位置

        // 2. 在目標位置慢速移動並小幅隨機旋轉
        float t = 0f;
        while (t < .75f)
        {
            transform.position += dir * slowSpeed * Time.deltaTime;

            Vector3 randomRot = new Vector3(
                Random.Range(-30f, 30f),
                Random.Range(-30f, 30f),
                Random.Range(-30f, 30f)
            ) * Time.deltaTime;
            body.Rotate(randomRot, Space.Self);

            t += Time.deltaTime;
            yield return null;
        }

        // 3. 再次加速往同方向移動直到消失
        float life = .5f;
        while (life > 0f)
        {
            transform.position += dir * fastSpeed * Time.deltaTime;
            life -= Time.deltaTime;
            yield return null;
        }
        child.SetActive(false);
    }
}
