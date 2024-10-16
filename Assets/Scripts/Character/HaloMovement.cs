using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloMovement : MonoBehaviour
{
    public float maxHeight = 0.25f; // 光環在玩家頭頂的最大高度
    public float minHeight = 0f; // 最小高度
    public float speed = 1.0f; // 改變高度的速度

    private float targetHeight; // 目標高度

    private void Start()
    {
        // 初始隨機設置目標高度
        targetHeight = Random.Range(minHeight, maxHeight);
    }

    private void Update()
    {
        // 平滑過渡到目標高度
        float yPosition = Mathf.Lerp(transform.localPosition.y, targetHeight, speed * Time.deltaTime);
        transform.localPosition = new Vector3(transform.localPosition.x, yPosition, transform.localPosition.z);

        // 如果接近目標高度，則選擇一個新的高度
        if (Mathf.Abs(transform.localPosition.y - targetHeight) < 0.01f)
        {
            targetHeight = Random.Range(minHeight, maxHeight);
        }
    }
}