using System;
using UnityEngine;

public class DyingShapeBehavior : ShapeBehavior
{
    private Vector3 _originalscale;
    private float _duration, _dyingAge;

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Dying;

    public void Initialize(Shape shape, float duration)
    {
        _originalscale = shape.transform.localScale;
        _duration = duration;
        _dyingAge = shape.Age;
        shape.MarkAsDying();
    }

    public override bool GameUpdate(Shape shape)
    {
        float dyingDuration = shape.Age - _dyingAge;
        if (dyingDuration < _duration)
        {
            float s = 1f - dyingDuration / _duration;
            s = (3f -2f * s) * s * s;
            shape.transform.localScale = _originalscale * s;
            return true;
        }
        shape.Die();
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(_originalscale);
        writer.Write(_duration);
        writer.Write(_dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        _originalscale = reader.ReadVector3();
        _duration = reader.ReadFloat();
        _dyingAge = reader.ReadFloat();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
    }
}
