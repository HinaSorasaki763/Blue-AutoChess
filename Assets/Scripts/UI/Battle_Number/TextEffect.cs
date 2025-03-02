using GameEnum;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TextEffect : MonoBehaviour
{
    public Image effectImage;
    public Image[] digitImages;
    public Sprite[] numberSprites;
    private float alpha = 1.0f;
    private Vector3 targetPosition;
    public event System.Action OnEffectFinished;
    public void Initialize(BattleDisplayEffect effect,Sprite effectSprite, int number, Vector3 screenPosition,bool empty,bool healing)
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
        if (effect == BattleDisplayEffect.None)
        {
            effectImage.gameObject.SetActive(false);
        }
        SetNumber(number,healing);

        // 將螢幕座標轉換為 UI 的 RectTransform 座標
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        alpha = 1.0f;
        targetPosition = screenPosition + Vector3.up * 50; // 設定上升目標位置
        gameObject.SetActive(true);
    }
    private void SetNumber(int number, bool healing)
    {
        // 先隱藏所有數字圖片
        foreach (var digitImage in digitImages)
        {
            digitImage.gameObject.SetActive(false);
        }

        int index = digitImages.Length - 1;
        // 至少會執行一次，數字為 0 也要顯示
        do
        {
            int digit = number % 10;
            number /= 10;

            // 若索引超出範圍或 digitSprites 資料錯誤，則跳過
            if (index < 0 || digit < 0 || digit >= numberSprites.Length)
                break;

            // 設定該位數圖片
            digitImages[index].sprite = numberSprites[digit];
            // 根據 healing 狀態設定顏色：治療時綠色，否則白色
            digitImages[index].color = healing ? Color.green : Color.white;
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

            // 通知 TextEffectPool 這個特效已經結束
            OnEffectFinished?.Invoke();
        }
    }
}
