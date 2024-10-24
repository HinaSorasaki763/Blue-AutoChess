#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyWave))]
public class EnemyWaveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyWave enemyWave = (EnemyWave)target;

        // ��� Grid Slots ���M��
        EditorGUILayout.LabelField("Grid Slots", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Grid Slot"))
        {
            enemyWave.gridSlots.Add(new EnemyWave.GridSlotData { GridIndex = 0, CharacterID = -1 });
        }

        for (int i = 0; i < enemyWave.gridSlots.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            enemyWave.gridSlots[i].GridIndex = EditorGUILayout.IntField("Grid Index", enemyWave.gridSlots[i].GridIndex);
            enemyWave.gridSlots[i].CharacterID = EditorGUILayout.IntField("Character ID", enemyWave.gridSlots[i].CharacterID);
            if (GUILayout.Button("Remove"))
            {
                enemyWave.gridSlots.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(); // �W�[���Z�A���϶������
        EditorGUILayout.Space(); // �W�[���Z�A���϶������
        // ��� Logistic Slots ���M��
        EditorGUILayout.LabelField("Logistic Slots", EditorStyles.boldLabel);
        enemyWave.logisticSlot1.GridIndex = EditorGUILayout.IntField("Logistic Slot 1 Grid Index", enemyWave.logisticSlot1.GridIndex);
        enemyWave.logisticSlot1.CharacterID = EditorGUILayout.IntField("Logistic Slot 1 Character ID", enemyWave.logisticSlot1.CharacterID);

        enemyWave.logisticSlot2.GridIndex = EditorGUILayout.IntField("Logistic Slot 2 Grid Index", enemyWave.logisticSlot2.GridIndex);
        enemyWave.logisticSlot2.CharacterID = EditorGUILayout.IntField("Logistic Slot 2 Character ID", enemyWave.logisticSlot2.CharacterID);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(enemyWave);
        }
    }
}
#endif
