using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float _min, _max;

    public float RandomValueInRange => Random.Range(_min, _max);
}
