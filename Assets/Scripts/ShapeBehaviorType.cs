public enum ShapeBehaviorType
{
    Movement,
    Rotation,
    Oscillation,
    Satellite,
    Growing,
    Dying
}

public static class ShapeBehaviorTypeMethods
{
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
    {
        switch (type)
        {
            case ShapeBehaviorType.Movement:
                return ShapeBehaviorPool<MovementShapeBehavior>.Get();
            case ShapeBehaviorType.Rotation:
                return ShapeBehaviorPool<RotationShapeBehavior>.Get();
            case ShapeBehaviorType.Oscillation:
                return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            case ShapeBehaviorType.Satellite:
                return ShapeBehaviorPool<SatelliteShapeBehavior>.Get();
            case ShapeBehaviorType.Growing:
                return ShapeBehaviorPool<GrowingShapeBehavior>.Get();
            case ShapeBehaviorType.Dying:
                return ShapeBehaviorPool<DyingShapeBehavior>.Get();
        }
        UnityEngine.Debug.LogError("Forgot support to " + type);
        return null;
    }
}
