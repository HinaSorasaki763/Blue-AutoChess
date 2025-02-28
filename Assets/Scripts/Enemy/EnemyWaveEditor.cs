#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyWave))]
public class EnemyWaveEditor : Editor
{
    private bool showGridSlots = true;
    private bool showLogisticSlots = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyWave enemyWave = (EnemyWave)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemy Wave Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Grid Slots 設定
        showGridSlots = EditorGUILayout.Foldout(showGridSlots, "Grid Slots");
        if (showGridSlots)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("Add Grid Slot"))
            {
                enemyWave.gridSlots.Add(new EnemyWave.GridSlotData { GridIndex = 0, CharacterID = -1 });
            }

            for (int i = 0; i < enemyWave.gridSlots.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Slot {i + 1}", EditorStyles.boldLabel);

                enemyWave.gridSlots[i].GridIndex = EditorGUILayout.IntField("Grid Index", enemyWave.gridSlots[i].GridIndex);
                enemyWave.gridSlots[i].CharacterID = EditorGUILayout.IntField("Character ID", enemyWave.gridSlots[i].CharacterID);

                // 編輯裝備
                EditorGUILayout.LabelField("Equipment IDs", EditorStyles.boldLabel);
                for (int j = 0; j < enemyWave.gridSlots[i].EquipmentIDs.Length; j++)
                {
                    enemyWave.gridSlots[i].EquipmentIDs[j] = EditorGUILayout.IntField($"Equipment {j + 1}", enemyWave.gridSlots[i].EquipmentIDs[j]);
                }

                if (GUILayout.Button("Remove"))
                {
                    enemyWave.gridSlots.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Logistic Slots 設定
        showLogisticSlots = EditorGUILayout.Foldout(showLogisticSlots, "Logistic Slots");
        if (showLogisticSlots)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Logistic Slot 1", EditorStyles.boldLabel);
            enemyWave.logisticSlot1.GridIndex = EditorGUILayout.IntField("Grid Index", enemyWave.logisticSlot1.GridIndex);
            enemyWave.logisticSlot1.CharacterID = EditorGUILayout.IntField("Character ID", enemyWave.logisticSlot1.CharacterID);

            EditorGUILayout.LabelField("Logistic Slot 2", EditorStyles.boldLabel);
            enemyWave.logisticSlot2.GridIndex = EditorGUILayout.IntField("Grid Index", enemyWave.logisticSlot2.GridIndex);
            enemyWave.logisticSlot2.CharacterID = EditorGUILayout.IntField("Character ID", enemyWave.logisticSlot2.CharacterID);

            EditorGUI.indentLevel--;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(enemyWave);
        }
    }
}
#endif
