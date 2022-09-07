using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifecycleShapeBehavior : ShapeBehavior
{
    private float _adultDuration, _dyingDuration,  _dyingAge;

    public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Dying;

    public void Initialize(Shape shape, float growingDuration, float adultDuration, float dyingDuration)
    {
        _adultDuration = adultDuration;
        _dyingDuration = dyingDuration;
        _dyingAge = growingDuration + adultDuration;

        if (growingDuration > 0f)
        {
            shape.AddBehavior<GrowingShapeBehavior>().Initialize(shape, growingDuration);
        }
    }

    public override bool GameUpdate(Shape shape)
    {
        if (shape.Age >= _dyingAge)
        {
            if (_dyingDuration <= 0f)
            {
                shape.Die();
                return true;
            }
            if (!shape.IsMarkedAsDying)
            {
                shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, _dyingDuration + _dyingAge - shape.Age);
            }
            return false;
        }
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(_adultDuration);
        writer.Write(_dyingDuration);
        writer.Write(_dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        _adultDuration = reader.ReadFloat();
        _dyingDuration = reader.ReadFloat();
        _dyingAge = reader.ReadFloat();
    }

    public override void Recycle()
    {
        ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
    }
}
