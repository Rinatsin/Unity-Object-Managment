using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int originIndentLevel = EditorGUI.indentLevel;

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        EditorGUI.indentLevel = 0;

        SerializedProperty minProperty = property.FindPropertyRelative("_min");
        SerializedProperty maxProperty = property.FindPropertyRelative("_max");
        float minValue = minProperty.floatValue;
        float maxValue = maxProperty.floatValue;

        //Левое поле ввода
        float fieldWidth = position.width / 4f - 4f;
        float sliderWidth = position.width / 2f;
        position.width = fieldWidth;
        minValue = EditorGUI.FloatField(position, minValue);
        position.x += fieldWidth + 4f;

        //Слайдер
        position.width = sliderWidth;
        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;
        EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);

        //Правое поле ввода
        position.x += sliderWidth + 4f;
        position.width = fieldWidth;
        maxValue = EditorGUI.FloatField(position, maxValue);

        //Значения не выходят за границы диапазона
        if (minValue < limit.Min)
        {
            minValue = limit.Min;
        } else if (minValue > limit.Max)
        {
            minValue = limit.Max;
        }

        if (maxValue < minValue)
        {
            maxValue = minValue;
        } else if (maxValue > limit.Max)
        {
            maxValue = limit.Max;
        }


        minProperty.floatValue = minValue;
        maxProperty.floatValue = maxValue;
        EditorGUI.EndProperty();

        EditorGUI.indentLevel = originIndentLevel;
    }
}
