using UnityEngine;
using UnityEngine.UI;

public class TwoButtonsToggle : MonoBehaviour
{
    [SerializeField] private GameObject firstObject;
    [SerializeField] private GameObject secondObject;
    [SerializeField] private Button firstButton;
    [SerializeField] private Button secondButton;

    [Header("Button Colors")]
    // 依需求：未啟用(不顯示)的要是深色，啟用的要是較淺色
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private void Start()
    {
        firstButton.onClick.AddListener(() =>
        {
            // 切換物體顯示
            firstObject.SetActive(true);
            secondObject.SetActive(false);

            // 第一個按鈕(顯示中的)用較淺色
            SetButtonColors(firstButton, activeColor);
            // 第二個按鈕(隱藏中的)用深色
            SetButtonColors(secondButton, inactiveColor);

            CustomLogger.Log(this, "第一個物體被啟用");
        });

        secondButton.onClick.AddListener(() =>
        {
            // 切換物體顯示
            firstObject.SetActive(false);
            secondObject.SetActive(true);

            // 第二個按鈕(顯示中的)用較淺色
            SetButtonColors(secondButton, activeColor);
            // 第一個按鈕(隱藏中的)用深色
            SetButtonColors(firstButton, inactiveColor);

            CustomLogger.Log(this, "第二個物體被啟用");
        });

        // 預設啟用第一個
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
