using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectCanvas : MonoBehaviour
{
    public static EffectCanvas Instance;

    // 紀錄每個角色對應的特效 Image
    public Dictionary<CharacterCTRL, Image> WakamoEffectSprites = new Dictionary<CharacterCTRL, Image>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        // 每幀遍歷字典，更新所有角色特效的位置
        foreach (var kvp in WakamoEffectSprites)
        {
            CharacterCTRL ctrl = kvp.Key;
            Image effectImage = kvp.Value;

            if (ctrl == null || effectImage == null)
                continue; // 若角色或 Image 已被移除，跳過

            // 將角色世界座標轉換到對應的UI座標系，並做些許Y偏移
            PositionWakamoImage(ctrl, ctrl.transform.position, 80f);
        }
    }

    /// <summary>
    /// 將指定角色的特效Image，對齊角色在世界座標的螢幕位置。
    /// offsetY: 額外向上/向下位移。
    /// </summary>
    public void PositionWakamoImage(CharacterCTRL ctrl, Vector3 worldPos, float offsetY = 80f)
    {
        if (!WakamoEffectSprites.ContainsKey(ctrl))
        {
            CustomLogger.LogWarning(this, $"角色 {ctrl.name} 沒有特效 Image，無法定位。");
            return;
        }

        Image image = WakamoEffectSprites[ctrl];
        RectTransform imageRect = image.rectTransform;

        // 計算目標螢幕座標
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.y += offsetY;

        // 找到 Canvas 的 RectTransform
        RectTransform canvasRect = GetComponent<RectTransform>();

        // 把螢幕座標轉換成 Canvas 的局部座標
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, null, out localPoint))
        {
            // 若Canvas為 Screen Space - Overlay，可用null當camera
            // 若Canvas為 Screen Space - Camera，請把對應的 camera 傳入
            imageRect.anchoredPosition = localPoint;
        }
    }
    /// <summary>
    /// 傳回（或建立）指定角色的 Wakamo 特效用 Image。
    /// 若字典中尚無此角色的 Image，則在本 Canvas 下建立。
    /// </summary>
    public Image GetOrCreateWakamoImage(CharacterCTRL ctrl, Sprite sprite)
    {
        if (WakamoEffectSprites.ContainsKey(ctrl))
        {
            // 若之前已經建立了，直接更新 sprite 後回傳
            var existingImage = WakamoEffectSprites[ctrl];
            existingImage.sprite = sprite;
            existingImage.color = Color.white;  // 可視情況重置顏色
            existingImage.gameObject.SetActive(true);
            return existingImage;
        }

        // 尚未建立就新建一個物件
        GameObject explosionObj = new GameObject("WakamoExplosion_" + ctrl.name);
        explosionObj.transform.SetParent(this.transform, false);
        Image explosionImage = explosionObj.AddComponent<Image>();
        explosionImage.sprite = sprite;

        // 放進字典
        WakamoEffectSprites.Add(ctrl, explosionImage);
        return explosionImage;
    }

    /// <summary>
    /// 移除字典中角色對應的 Image，並銷毀 GameObject。
    /// </summary>
    public void RemoveWakamoImage(CharacterCTRL ctrl)
    {
        if (WakamoEffectSprites.ContainsKey(ctrl))
        {
            Image img = WakamoEffectSprites[ctrl];
            if (img != null && img.gameObject != null)
            {
                Destroy(img.gameObject);
            }
            WakamoEffectSprites.Remove(ctrl);
        }
    }
    public void FadeToRedOverFiveSeconds(CharacterCTRL ctrl)
    {
        if (!WakamoEffectSprites.ContainsKey(ctrl))
        {
            CustomLogger.LogWarning(this, $"角色 {ctrl.name} 沒有特效 Image，無法執行漸變。");
            return;
        }

        Image image = WakamoEffectSprites[ctrl];
        StartCoroutine(FadeColorCoroutine(image, Color.red, 5f));
    }

    /// <summary>
    /// Coroutine：在指定秒數內，將 Image 顏色從「當前顏色」漸變到 targetColor。
    /// </summary>
    private IEnumerator FadeColorCoroutine(Image image, Color targetColor, float duration)
    {
        Color startColor = image.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        image.color = targetColor;
        CustomLogger.Log(this, "圖片顏色漸變結束");
    }

    /// <summary>
    /// 將角色對應的特效圖片重置為白色，並將其關閉。
    /// </summary>
    public void ResetImage(CharacterCTRL ctrl,bool disable)
    {
        if (!WakamoEffectSprites.ContainsKey(ctrl)) return;

        Image image = WakamoEffectSprites[ctrl];
        image.color = Color.white;
        if (disable)
        {
            image.gameObject.SetActive(false);
        }
    }
}
