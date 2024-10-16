using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public Transform parent;
    private Camera cam;
    public int amount;
    public TextMeshProUGUI dmgAmount;

    public float floatSpeed = 2f;        // 文字上升的速度
    public float fadeDuration = 1f;      // 文字淡出的總時長
    public float startFadeAfter = 1f;    // 開始淡出前的等待時間
    private void OnEnable()
    {
        cam = Camera.main;
        dmgAmount.text = amount.ToString();
        StartCoroutine(FloatAndFade());
    }

    private void OnDisable()
    {
        StopCoroutine(FloatAndFade()); // 停止協程以避免衝突
        ResetFloatingText(); // 重置狀態
    }

    private void ResetFloatingText()
    {
        amount = 0;
        dmgAmount.text = string.Empty;
        dmgAmount.color = Color.white; // 重置顏色為完全不透明
        transform.position = new Vector3(0f, 0f, 0f);
        parent = null;
    }
    public void SetTextsParent(Transform parent)
    {
        this.parent = parent;
        transform.position = cam.WorldToScreenPoint(parent.position); // 更新位置
    }

    public void SetAmount(int amount)
    {
        this.amount = amount;
        dmgAmount.text = amount.ToString();
    }

    public void Start()
    {

    }
    public void Update()
    {

    }

    private IEnumerator FloatAndFade()
    {
        yield return null;
        if (parent == null)
        {
            yield break;
        }
        float elapsedTime = 0;
        Color startColor = dmgAmount.color;

        // 等待開始淡出前的時間
        while (elapsedTime < startFadeAfter)
        {
            // 更新位置
            transform.position = cam.WorldToScreenPoint(parent.position) + Vector3.up * 100 * (floatSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 淡出
        while (elapsedTime < startFadeAfter + fadeDuration)
        {
            // 繼續更新位置
            transform.position = cam.WorldToScreenPoint(parent.position) + Vector3.up * 100 * (floatSpeed * elapsedTime);
            float fadeAmount = Mathf.Clamp01((elapsedTime - startFadeAfter) / fadeDuration);
            dmgAmount.color = new Color(startColor.r, startColor.g, startColor.b, 1 - fadeAmount);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 禁用浮動文字
        transform.position = new Vector3(0f, 0f, 0f);
        gameObject.SetActive(false);
    }
}
