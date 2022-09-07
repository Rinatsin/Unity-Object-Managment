using UnityEngine;

public abstract class SpawnZone : GameLevelObject
{
	[System.Serializable]
	public struct SpawnConfiguration
    {
		public enum MovementDirection
		{
			Forward,
			Upward,
			Outward,
			Random
		}

		public ShapeFactory[] _factories;
		public MovementDirection movementDirection;
		public FloatRange spawnSpeed;
		public FloatRange angularSpeed;
		public FloatRange scale;
		public ColorRangeHSV color;
		public bool uniformColor;
		public MovementDirection oscillationDirection;
		public FloatRange oscillationAmplitude;
		public FloatRange oscillationFrequency;

		[System.Serializable]
		public struct SatelliteConfiguration
        {
			[FloatRangeSlider(0.1f, 1.0f)]
			public FloatRange relativeScale;

			public IntRange amount;
			public FloatRange orbitRadius;
			public FloatRange orbitFrequency;
			public bool uniformLifecycles;

		}

		public SatelliteConfiguration satellite;

		[System.Serializable]
		public struct LifecycleConfiguration
        {
			[FloatRangeSlider(0f, 2f)]
			public FloatRange growingDuration;

			[FloatRangeSlider(0f, 100f)]
			public FloatRange adultDuration;

			[FloatRangeSlider(0f, 2f)]
			public FloatRange dyingDuration;

			public Vector3 RandomDurations 
				=> new Vector3(growingDuration.RandomValueInRange, adultDuration.RandomValueInRange ,dyingDuration.RandomValueInRange);
		}

		public LifecycleConfiguration lifecycle;

	}

	[SerializeField, Range(0f, 50f)] private float _spawnSpeed;
	[SerializeField] private SpawnConfiguration _spawnConfig;

	private float _spawnProgress;

	public abstract Vector3 SpawnPoint { get; }

    public virtual void SpawnShapes()
    {
		int factoryIndex = Random.Range(0, _spawnConfig._factories.Length);
		Shape shape = _spawnConfig._factories[factoryIndex].GetRandom();
		shape.gameObject.layer = gameObject.layer;
		Transform t = shape.transform;
		t.localPosition = SpawnPoint;
		t.localRotation = Random.rotation;
		t.localScale = Vector3.one * _spawnConfig.scale.RandomValueInRange;
		SetupColor(shape);
		float angularSpeed = _spawnConfig.angularSpeed.RandomValueInRange;
		if (angularSpeed != 0)
		{
			var rotation = shape.AddBehavior<RotationShapeBehavior>();
			rotation.AngularVelocity = Random.onUnitSphere * _spawnConfig.angularSpeed.RandomValueInRange;
		}

		float speed = _spawnConfig.spawnSpeed.RandomValueInRange;
		if (speed != 0)
        {
			var movement = shape.AddBehavior<MovementShapeBehavior>();
			movement.Velocity = GetDirectionVector(_spawnConfig.movementDirection, t) * speed;
		}
		SetupOscillation(shape);

		//Создание спутников

		Vector3 lifecycleDurations = _spawnConfig.lifecycle.RandomDurations;
		int satelliteCount = _spawnConfig.satellite.amount.RandomValueInRange;
		for (int i = 0; i < satelliteCount; i++)
		{
			CreateSatelliteFor(shape, 
				_spawnConfig.satellite.uniformLifecycles ? lifecycleDurations : _spawnConfig.lifecycle.RandomDurations);
		}
		SetupLifecycle(shape, lifecycleDurations);
	}

    public override void Save(GameDataWriter writer)
    {
		writer.Write(_spawnProgress);
    }

    public override void Load(GameDataReader reader)
    {
		_spawnProgress = reader.ReadFloat();
    }

    public override void GameUpdate()
    {
		_spawnProgress += Time.deltaTime * _spawnSpeed;
		while(_spawnProgress >= 1f)
        {
			_spawnProgress -= 1f;
			SpawnShapes();
        }
	}

    private void SetupOscillation(Shape shape)
    {
		float amplitude = _spawnConfig.oscillationAmplitude.RandomValueInRange;
		float frequency = _spawnConfig.oscillationFrequency.RandomValueInRange;

		if (amplitude == 0f || frequency == 0f) return;

		var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
		oscillation.Offset = GetDirectionVector(_spawnConfig.oscillationDirection, shape.transform) * amplitude;
		oscillation.Frequency = frequency;
    }

    private Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t)
    {
        switch (direction)
        {
			case SpawnConfiguration.MovementDirection.Upward:
				return transform.up;
			case SpawnConfiguration.MovementDirection.Outward:
				return (t.localPosition - transform.position).normalized;
			case SpawnConfiguration.MovementDirection.Random:
				return Random.onUnitSphere;
			default:
				return transform.forward;
		}
    }

	private void CreateSatelliteFor(Shape focalShape, Vector2 lifecycleDurations)
    {
		int factoryIndex = Random.Range(0, _spawnConfig._factories.Length);
		Shape shape = _spawnConfig._factories[factoryIndex].GetRandom();
		shape.gameObject.layer = gameObject.layer;
		Transform t = shape.transform;
		t.localRotation = Random.rotation;
		t.localScale = focalShape.transform.localScale * _spawnConfig.satellite.relativeScale.RandomValueInRange;
		SetupColor(shape);
		shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
			shape, focalShape,
			_spawnConfig.satellite.orbitRadius.RandomValueInRange,
			_spawnConfig.satellite.orbitFrequency.RandomValueInRange
			);
		SetupLifecycle(shape, lifecycleDurations);
    }

	private void SetupColor(Shape shape)
    {
		if (_spawnConfig.uniformColor)
		{
			shape.SetColor(_spawnConfig.color.RandomInRange);
		}
		else
		{
			for (int i = 0; i < shape.ColorCount; i++)
			{
				shape.SetColor(_spawnConfig.color.RandomInRange, i);
			}
		}
	}

	private void SetupLifecycle(Shape shape, Vector3 durations)
    {
		if (durations.x > 0f)
        {
			if (durations.y > 0f || durations.z > 0f)
            {
				shape.AddBehavior<LifecycleShapeBehavior>().Initialize(shape, durations.x, durations.y, durations.z);
            }
            else
			{
				shape.AddBehavior<GrowingShapeBehavior>().Initialize(shape, durations.x);
			}
        } 
		else if (durations.y > 0f)
        {
			shape.AddBehavior<LifecycleShapeBehavior>().Initialize(shape, durations.x, durations.y, durations.z);
        }
		else if (durations.z > 0f)
        {
			shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, durations.z);
        }
    }
}
