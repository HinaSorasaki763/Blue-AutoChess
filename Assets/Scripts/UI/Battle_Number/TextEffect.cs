using GameEnum;
using UnityEngine;
using UnityEngine.UI;

public class TextEffect : MonoBehaviour
{
    public Image effectImage;
    public Image[] digitImages;
    public Sprite[] numberSprites;
    private float alpha = 1.0f;
    private Vector3 targetPosition;

    public void Initialize(Sprite effectSprite, int number, Vector3 screenPosition,bool empty)
    {
        effectImage.sprite = effectSprite;
        if (empty)
        {
            effectImage.gameObject.SetActive(false);
        }
        else
        {
            effectImage.gameObject.SetActive(true);
        }
        SetNumber(number);

        // 將螢幕座標轉換為 UI 的 RectTransform 座標
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        alpha = 1.0f;
        targetPosition = screenPosition + Vector3.up * 50; // 設定上升目標位置
        gameObject.SetActive(true);
    }

    private void SetNumber(int number)
    {
        for (int i = 0; i < digitImages.Length; i++)
        {
            digitImages[i].gameObject.SetActive(false);
        }

        int index = digitImages.Length - 1;
        do
        {
            int digit = number % 10;
            number /= 10;
            digitImages[index].sprite = numberSprites[digit];
            digitImages[index].gameObject.SetActive(true);
            index--;
        } while (number > 0 && index >= 0);
    }

    private void Update()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * 2.0f);
        alpha -= Time.deltaTime * 0.5f;
        effectImage.color = new Color(effectImage.color.r, effectImage.color.g, effectImage.color.b, alpha);

        foreach (var digitImage in digitImages)
        {
            digitImage.color = new Color(digitImage.color.r, digitImage.color.g, digitImage.color.b, alpha);
        }

        if (alpha <= 0)
        {
            Utility.ChangeImageAlpha(effectImage, 1);
            gameObject.SetActive(false);
        }
    }
}
