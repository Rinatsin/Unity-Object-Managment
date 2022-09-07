using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntRange
{
    public int _min, _max;

    public int RandomValueInRange => Random.Range(_min, _max + 1);
}
