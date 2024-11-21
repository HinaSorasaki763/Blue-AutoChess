using System.Collections;
using UnityEngine;
using TMPro; // �p�G�ϥ� TextMeshPro
using UnityEngine.UI;
using Unity.VisualScripting;
using GameEnum; // �p�G�ϥ� UI.Text

public class MessagePopup : MonoBehaviour
{
    public TextMeshProUGUI messageText; // �Ω���ܤ�r���ե� (TextMeshPro)
    public float fadeDuration = 1f; // �H�X������ɶ�
    public Image BackPanel;
    void Awake()
    {

    }

    /// <summary>
    /// ��ܰT���ó]�w�۰����áC
    /// </summary>
    /// <param name="message">�n��ܪ���r</param>
    /// <param name="displayDuration">��ܪ��`�ɶ��]�]�t�H�X�^</param>
    public void ShowMessage(string message, float displayDuration)
    {
        messageText.text = message;
        AdjustSize();
        StartCoroutine(FadeAndDestroy(displayDuration));
    }

    // �ھڤ�r�۰ʽվ� RectTransform �j�p
    private void AdjustSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = messageText.GetPreferredValues(messageText.text);
        rectTransform.sizeDelta = new Vector2(size.x + 20, size.y + 20); // �K�[�B�~��Z
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
