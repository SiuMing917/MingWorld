using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]

public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChanceInGrass = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        GUILayout.Label($"�`�J���v = {totalChanceInGrass}", style);

        if (totalChanceInGrass != 100 && totalChanceInGrass != -1)
            EditorGUILayout.HelpBox($"�b��a���`�J���v�O {totalChanceInGrass} ,����100%", MessageType.Error);

        if (totalChanceInWater != 100 && totalChanceInWater != -1)
            EditorGUILayout.HelpBox($"�b�������`�J���v�O {totalChanceInWater} ,����100%", MessageType.Error);
    }

}
