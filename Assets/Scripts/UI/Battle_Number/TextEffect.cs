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

        // �N�ù��y���ഫ�� UI �� RectTransform �y��
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        alpha = 1.0f;
        targetPosition = screenPosition + Vector3.up * 50; // �]�w�W�ɥؼЦ�m
        gameObject.SetActive(true);
    }
    private void SetNumber(int number, bool healing)
    {
        // �����éҦ��Ʀr�Ϥ�
        foreach (var digitImage in digitImages)
        {
            digitImage.gameObject.SetActive(false);
        }

        int index = digitImages.Length - 1;
        // �ܤַ|����@���A�Ʀr�� 0 �]�n���
        do
        {
            int digit = number % 10;
            number /= 10;

            // �Y���޶W�X�d��� digitSprites ��ƿ��~�A�h���L
            if (index < 0 || digit < 0 || digit >= numberSprites.Length)
                break;

            // �]�w�Ӧ�ƹϤ�
            digitImages[index].sprite = numberSprites[digit];
            // �ھ� healing ���A�]�w�C��G�v���ɺ��A�_�h�զ�
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

            // �q�� TextEffectPool �o�ӯS�Ĥw�g����
            OnEffectFinished?.Invoke();
        }
    }
}
