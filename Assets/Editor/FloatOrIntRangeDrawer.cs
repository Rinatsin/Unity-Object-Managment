using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRange)), CustomPropertyDrawer(typeof(IntRange))]
public class FloatOrIntRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int origonalIndentLevel = EditorGUI.indentLevel;
        float originalLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        position.width = position.width / 2.0f;
        EditorGUIUtility.labelWidth = position.width / 2;
        EditorGUI.indentLevel = 1;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_min"));
        position.x += position.width;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_max"));
        EditorGUI.EndProperty();

        EditorGUI.indentLevel = origonalIndentLevel;
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }
}
