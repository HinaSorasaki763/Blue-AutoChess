using UnityEngine;
using UnityEditor;
using GameEnum;

[CustomPropertyDrawer(typeof(Stat))]
public class StatPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �}�l�@���ݩʮ�
        EditorGUI.BeginProperty(position, label, property);

        // �]�m�檺�j�p
        var halfWidth = position.width / 2;

        // ��� Stat ���ݩ�
        SerializedProperty statTypeProp = property.FindPropertyRelative("statType");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        // �b Inspector ��ø�s StatType �M Value�A�ä�����b���
        Rect statTypeRect = new Rect(position.x, position.y, halfWidth - 5, position.height);
        Rect valueRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height);

        // ��� StatType �U�Կ��
        EditorGUI.PropertyField(statTypeRect, statTypeProp, GUIContent.none);

        // ��� Value �Ʀr���
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

        // �����ݩʮ�
        EditorGUI.EndProperty();
    }

    // �]�m���סA�o���q�{���@�檺����
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
