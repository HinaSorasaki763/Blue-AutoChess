using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public Transform parent;
    private Camera cam;
    public int amount;
    public TextMeshProUGUI dmgAmount;

    public float floatSpeed = 2f;        // ��r�W�ɪ��t��
    public float fadeDuration = 1f;      // ��r�H�X���`�ɪ�
    public float startFadeAfter = 1f;    // �}�l�H�X�e�����ݮɶ�
    private void OnEnable()
    {
        cam = Camera.main;
        dmgAmount.text = amount.ToString();
        StartCoroutine(FloatAndFade());
    }

    private void OnDisable()
    {
        StopCoroutine(FloatAndFade()); // �����{�H�קK�Ĭ�
        ResetFloatingText(); // ���m���A
    }

    private void ResetFloatingText()
    {
        amount = 0;
        dmgAmount.text = string.Empty;
        dmgAmount.color = Color.white; // ���m�C�⬰�������z��
        transform.position = new Vector3(0f, 0f, 0f);
        parent = null;
    }
    public void SetTextsParent(Transform parent)
    {
        this.parent = parent;
        transform.position = cam.WorldToScreenPoint(parent.position); // ��s��m
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

        // ���ݶ}�l�H�X�e���ɶ�
        while (elapsedTime < startFadeAfter)
        {
            // ��s��m
            transform.position = cam.WorldToScreenPoint(parent.position) + Vector3.up * 100 * (floatSpeed * elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �H�X
        while (elapsedTime < startFadeAfter + fadeDuration)
        {
            // �~���s��m
            transform.position = cam.WorldToScreenPoint(parent.position) + Vector3.up * 100 * (floatSpeed * elapsedTime);
            float fadeAmount = Mathf.Clamp01((elapsedTime - startFadeAfter) / fadeDuration);
            dmgAmount.color = new Color(startColor.r, startColor.g, startColor.b, 1 - fadeAmount);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �T�ίB�ʤ�r
        transform.position = new Vector3(0f, 0f, 0f);
        gameObject.SetActive(false);
    }
}
