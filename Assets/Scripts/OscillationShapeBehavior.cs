using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillationShapeBehavior : ShapeBehavior
{
    private float _previousOscillation;

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Oscillation;

    public Vector3 Offset { get; set; }
    public float Frequency { get; set; }

    public override bool GameUpdate(Shape shape)
    {
        float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
        shape.transform.localPosition += (oscillation - _previousOscillation) * Offset;
        _previousOscillation = oscillation;
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Frequency);
        writer.Write(_previousOscillation);
    }

    public override void Load(GameDataReader reader)
    {
        Offset = reader.ReadVector3();
        Frequency = reader.ReadFloat();
        _previousOscillation = reader.ReadFloat();
    }

    public override void Recycle()
    {
        _previousOscillation = 0f;
        ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
    }
}
