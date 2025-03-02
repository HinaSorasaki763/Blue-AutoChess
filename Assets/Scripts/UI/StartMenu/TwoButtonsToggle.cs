using UnityEngine;
using UnityEngine.UI;

public class TwoButtonsToggle : MonoBehaviour
{
    [SerializeField] private GameObject firstObject;
    [SerializeField] private GameObject secondObject;
    [SerializeField] private Button firstButton;
    [SerializeField] private Button secondButton;

    [Header("Button Colors")]
    // �̻ݨD�G���ҥ�(�����)���n�O�`��A�ҥΪ��n�O���L��
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void Start()
    {
        firstButton.onClick.AddListener(() =>
        {
            // �����������
            firstObject.SetActive(true);
            secondObject.SetActive(false);

            // �Ĥ@�ӫ��s(��ܤ���)�θ��L��
            SetButtonColors(firstButton, activeColor);
            // �ĤG�ӫ��s(���ä���)�β`��
            SetButtonColors(secondButton, inactiveColor);

            CustomLogger.Log(this, "�Ĥ@�Ӫ���Q�ҥ�");
        });

        secondButton.onClick.AddListener(() =>
        {
            // �����������
            firstObject.SetActive(false);
            secondObject.SetActive(true);

            // �ĤG�ӫ��s(��ܤ���)�θ��L��
            SetButtonColors(secondButton, activeColor);
            // �Ĥ@�ӫ��s(���ä���)�β`��
            SetButtonColors(firstButton, inactiveColor);

            CustomLogger.Log(this, "�ĤG�Ӫ���Q�ҥ�");
        });

        // �w�]�ҥβĤ@��
        firstButton.onClick.Invoke();
    }

    private void SetButtonColors(Button button, Color color)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        button.colors = cb;
    }
}
