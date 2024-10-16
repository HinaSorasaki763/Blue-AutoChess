using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloMovement : MonoBehaviour
{
    public float maxHeight = 0.25f; // �����b���a�Y�����̤j����
    public float minHeight = 0f; // �̤p����
    public float speed = 1.0f; // ���ܰ��ת��t��

    private float targetHeight; // �ؼа���

    private void Start()
    {
        // ��l�H���]�m�ؼа���
        targetHeight = Random.Range(minHeight, maxHeight);
    }

    private void Update()
    {
        // ���ƹL���ؼа���
        float yPosition = Mathf.Lerp(transform.localPosition.y, targetHeight, speed * Time.deltaTime);
        transform.localPosition = new Vector3(transform.localPosition.x, yPosition, transform.localPosition.z);

        // �p�G����ؼа��סA�h��ܤ@�ӷs������
        if (Mathf.Abs(transform.localPosition.y - targetHeight) < 0.01f)
        {
            targetHeight = Random.Range(minHeight, maxHeight);
        }
    }
}