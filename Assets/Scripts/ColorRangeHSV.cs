using UnityEngine;

[System.Serializable]
public struct ColorRangeHSV
{
    [FloatRangeSlider(0f, 1f)]
    public FloatRange hue, saturation, value;

    public Color RandomInRange => Random.ColorHSV(hue._min, hue._max,
                                                saturation._min, saturation._max,
                                                value._min, value._max,
                                                1f, 1f);
}
