using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectCanvas : MonoBehaviour
{
    public static EffectCanvas Instance;

    // �����C�Ө���������S�� Image
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
        // �C�V�M���r��A��s�Ҧ�����S�Ī���m
        foreach (var kvp in WakamoEffectSprites)
        {
            CharacterCTRL ctrl = kvp.Key;
            Image effectImage = kvp.Value;

            if (ctrl == null || effectImage == null)
                continue; // �Y����� Image �w�Q�����A���L

            // �N����@�ɮy���ഫ�������UI�y�Шt�A�ð��ǳ\Y����
            PositionWakamoImage(ctrl, ctrl.transform.position, 80f);
        }
    }

    /// <summary>
    /// �N���w���⪺�S��Image�A�������b�@�ɮy�Ъ��ù���m�C
    /// offsetY: �B�~�V�W/�V�U�첾�C
    /// </summary>
    public void PositionWakamoImage(CharacterCTRL ctrl, Vector3 worldPos, float offsetY = 80f)
    {
        if (!WakamoEffectSprites.ContainsKey(ctrl))
        {
            CustomLogger.LogWarning(this, $"���� {ctrl.name} �S���S�� Image�A�L�k�w��C");
            return;
        }

        Image image = WakamoEffectSprites[ctrl];
        RectTransform imageRect = image.rectTransform;

        // �p��ؼпù��y��
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.y += offsetY;

        // ��� Canvas �� RectTransform
        RectTransform canvasRect = GetComponent<RectTransform>();

        // ��ù��y���ഫ�� Canvas �������y��
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, null, out localPoint))
        {
            // �YCanvas�� Screen Space - Overlay�A�i��null��camera
            // �YCanvas�� Screen Space - Camera�A�Ч������ camera �ǤJ
            imageRect.anchoredPosition = localPoint;
        }
    }
    /// <summary>
    /// �Ǧ^�]�Ϋإߡ^���w���⪺ Wakamo �S�ĥ� Image�C
    /// �Y�r�夤�|�L�����⪺ Image�A�h�b�� Canvas �U�إߡC
    /// </summary>
    public Image GetOrCreateWakamoImage(CharacterCTRL ctrl, Sprite sprite)
    {
        if (WakamoEffectSprites.ContainsKey(ctrl))
        {
            // �Y���e�w�g�إߤF�A������s sprite ��^��
            var existingImage = WakamoEffectSprites[ctrl];
            existingImage.sprite = sprite;
            existingImage.color = Color.white;  // �i�����p���m�C��
            existingImage.gameObject.SetActive(true);
            return existingImage;
        }

        // �|���إߴN�s�ؤ@�Ӫ���
        GameObject explosionObj = new GameObject("WakamoExplosion_" + ctrl.name);
        explosionObj.transform.SetParent(this.transform, false);
        Image explosionImage = explosionObj.AddComponent<Image>();
        explosionImage.sprite = sprite;

        // ��i�r��
        WakamoEffectSprites.Add(ctrl, explosionImage);
        return explosionImage;
    }

    /// <summary>
    /// �����r�夤��������� Image�A�þP�� GameObject�C
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
            CustomLogger.LogWarning(this, $"���� {ctrl.name} �S���S�� Image�A�L�k���溥�ܡC");
            return;
        }

        Image image = WakamoEffectSprites[ctrl];
        StartCoroutine(FadeColorCoroutine(image, Color.red, 5f));
    }

    /// <summary>
    /// Coroutine�G�b���w��Ƥ��A�N Image �C��q�u��e�C��v���ܨ� targetColor�C
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
        CustomLogger.Log(this, "�Ϥ��C�⺥�ܵ���");
    }

    /// <summary>
    /// �N����������S�ĹϤ����m���զ�A�ñN�������C
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
