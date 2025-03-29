using UnityEngine;
using TMPro;

public class DebugInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText; // �b Inspector ���j�w TextMeshPro ����
    public GameObject debugPanel; // �Ψӱ��� UI ���

    private bool isShowing = false;
    private float deltaTime = 0.0f;

    void Update()
    {
        if (isShowing)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            string cpuName = SystemInfo.processorType;
            int cpuCores = SystemInfo.processorCount;
            string gpuName = SystemInfo.graphicsDeviceName;
            string ramSize = SystemInfo.systemMemorySize + " MB";

            debugText.text = $"FPS: {fps:F1}\n" +
                             $"CPU: {cpuName} ({cpuCores} cores)\n" +
                             $"GPU: {gpuName}\n" +
                             $"RAM: {ramSize}";
        }
    }

    public void ToggleDebugInfo()
    {
        isShowing = !isShowing;
        debugPanel.SetActive(isShowing);
    }
}
