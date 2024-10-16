using UnityEngine;
using UnityEditor;
using GameEnum;

[CustomPropertyDrawer(typeof(Stat))]
public class StatPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 開始一個屬性框
        EditorGUI.BeginProperty(position, label, property);

        // 設置行的大小
        var halfWidth = position.width / 2;

        // 獲取 Stat 的屬性
        SerializedProperty statTypeProp = property.FindPropertyRelative("statType");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        // 在 Inspector 中繪製 StatType 和 Value，並分成兩半顯示
        Rect statTypeRect = new Rect(position.x, position.y, halfWidth - 5, position.height);
        Rect valueRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, position.height);

        // 顯示 StatType 下拉選單
        EditorGUI.PropertyField(statTypeRect, statTypeProp, GUIContent.none);

        // 顯示 Value 數字欄位
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

        // 結束屬性框
        EditorGUI.EndProperty();
    }

    // 設置高度，這裡默認為一行的高度
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
