using System.Collections;
using UnityEngine;
using TMPro; // 如果使用 TextMeshPro
using UnityEngine.UI;
using Unity.VisualScripting;
using GameEnum; // 如果使用 UI.Text

public class MessagePopup : MonoBehaviour
{
    public TextMeshProUGUI messageText; // 用於顯示文字的組件 (TextMeshPro)
    public float fadeDuration = 1f; // 淡出的持續時間
    public Image BackPanel;
    void Awake()
    {

    }

    /// <summary>
    /// 顯示訊息並設定自動隱藏。
    /// </summary>
    /// <param name="message">要顯示的文字</param>
    /// <param name="displayDuration">顯示的總時間（包含淡出）</param>
    public void ShowMessage(string message, float displayDuration)
    {
        messageText.text = message;
        AdjustSize();
        StartCoroutine(FadeAndDestroy(displayDuration));
    }

    // 根據文字自動調整 RectTransform 大小
    private void AdjustSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = messageText.GetPreferredValues(messageText.text);
        rectTransform.sizeDelta = new Vector2(size.x + 20, size.y + 20); // 添加額外邊距
    }
    private IEnumerator FadeAndDestroy(float displayDuration)
    {
        messageText.alpha = 1f;
        Utility.ChangeImageAlpha(BackPanel,1);
        yield return new WaitForSeconds(displayDuration - fadeDuration);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            messageText.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            Utility.ChangeImageAlpha(BackPanel, Mathf.Lerp(1f, 0f, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        Utility.ChangeImageAlpha(BackPanel, 0f);
        messageText.alpha = 0f;
        Destroy(gameObject);
    }
}
