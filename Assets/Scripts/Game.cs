using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Game : PersistableObject
{
	private const int SAVE_VERSION = 7;

    [SerializeField] private ShapeFactory[] _shapeFactories;
	[SerializeField] private PersistentStorage _storage;
	[SerializeField] private Slider _creationSpeedSlider;
	[SerializeField] private Slider _destructionSpeedSlider;
	[SerializeField] private KeyCode _createKey = KeyCode.C;
	[SerializeField] private KeyCode _newGameKey = KeyCode.N;
	[SerializeField] private KeyCode _saveKey = KeyCode.S;
	[SerializeField] private KeyCode _loadKey = KeyCode.L;
	[SerializeField] private KeyCode _deestroyKey = KeyCode.X;
	[SerializeField] private int _levelCount;
	[SerializeField] private bool _reseedOnLoad;
	[SerializeField] private float _destroyDuration;


	private List<Shape> _shapes;
	private List<ShapeInstance> _killList, _markAsDyingList;
	private float _creationProgress, _destructionProgress;
	private int _loadedLevelBuildIndex;
	private Random.State _mainRandomState;
	private bool _inGameUpdateLoop;
	private int _dyingShapeCount;

	public float CreationSpeed { get; set; }
	public float DestructionSpeed { get; set; }
	public static Game Instance { get; private set; }

	public void AddShape(Shape shape)
    {
		shape.SaveIndex = _shapes.Count;
		_shapes.Add(shape);
    }

	public Shape GetShape(int index)
    {
		return _shapes[index];
    }

	public void Kill(Shape shape)
    {
		if (_inGameUpdateLoop)
        {
			_killList.Add(shape);
        }
        else
        {
			KillImmediately(shape);
        }
    }

	public void MarkAsDying(Shape shape)
    {
		if (_inGameUpdateLoop)
        {
			_markAsDyingList.Add(shape);
        }
        else
        {
			MarkAsDyingImmediately(shape);
        }
    }

	public bool IsMarkedAsDying(Shape shape)
    {
		return shape.SaveIndex < _dyingShapeCount;
    }

	private void KillImmediately(Shape shape)
    {
		int index = shape.SaveIndex;
		shape.Recycle();

		if (index < _dyingShapeCount && index < --_dyingShapeCount)
        {
			_shapes[_dyingShapeCount].SaveIndex = index;
			_shapes[index] = _shapes[_dyingShapeCount];
			index = _dyingShapeCount;
        }

		int lastIndex = _shapes.Count - 1;
		if (index < lastIndex)
		{
			_shapes[lastIndex].SaveIndex = index;
			_shapes[index] = _shapes[lastIndex];
		}
		_shapes.RemoveAt(lastIndex);
	}

    private void OnEnable()
    {
		Instance = this;
		if (_shapeFactories[0].FactoryId != 0)
        {
			for (int i = 0; i < _shapeFactories.Length; i++)
			{
				_shapeFactories[i].FactoryId = i;
			}
		}
    }

    private void Start()
    {
		_mainRandomState = Random.state;
		_shapes = new List<Shape>();
		_killList = new List<ShapeInstance>();
		_markAsDyingList = new List<ShapeInstance>();

		if (Application.isEditor)
        {
			for (int i = 0; i < SceneManager.sceneCount; i++)
            {
				Scene loadedScene = SceneManager.GetSceneAt(i);
				if (loadedScene.name.Contains("Level "))
                {
					SceneManager.SetActiveScene(loadedScene);
					_loadedLevelBuildIndex = loadedScene.buildIndex;
					return;
                }
            }
		}

		BeginNewGame();
		StartCoroutine(LoadLevel(1));
    }

    private void Update()
	{
		if (Input.GetKeyDown(_createKey))
		{
			GameLevel.Current.SpawnShapes();
		}
		else if (Input.GetKeyDown(_newGameKey))
		{
			BeginNewGame();
			StartCoroutine(LoadLevel(_loadedLevelBuildIndex));
		}
		else if (Input.GetKeyDown(_saveKey))
		{
			_storage.Save(this, SAVE_VERSION);
		}
		else if (Input.GetKeyDown(_loadKey))
		{
			_storage.Load(this);
		}
		else if (Input.GetKeyDown(_deestroyKey))
        {
			DestroyShape();
        }
        else
        {
			for(int i = 1; i <= _levelCount; i++)
            {
				if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
					BeginNewGame();
					StartCoroutine(LoadLevel(i));
					return;
                }
            }
        }
	}

    private void FixedUpdate()
    {
		_inGameUpdateLoop = true;
		for (int i = 0; i < _shapes.Count; i++)
        {
			_shapes[i].GameUpdate();
        }
		GameLevel.Current.GameUpdate();
		_inGameUpdateLoop = false;

		_creationProgress += Time.deltaTime * CreationSpeed;
		while (_creationProgress >= 1f)
		{
			_creationProgress -= 1f;
			GameLevel.Current.SpawnShapes();
		}

		_destructionProgress += Time.deltaTime * DestructionSpeed;
		while (_destructionProgress >= 1f)
		{
			_destructionProgress -= 1f;
			DestroyShape();
		}

		int limit = GameLevel.Current.PopulationLimit;
		if (limit > 0)
        {
			while(_shapes.Count - _dyingShapeCount > limit)
            {
				DestroyShape();
            }
        }

		if (_killList.Count > 0)
        {
			for (int i = 0; i < _killList.Count; i++)
            {
				if (_killList[i].IsValid)
				{
					KillImmediately(_killList[i].Shape);
				}
            }
			_killList.Clear();
        }

		if (_markAsDyingList.Count > 0)
        {
			for (int i = 0; i < _markAsDyingList.Count; i++)
            {
				if (_markAsDyingList[i].IsValid)
                {
					MarkAsDyingImmediately(_markAsDyingList[i].Shape);
                }
            }
			_markAsDyingList.Clear();
        }
	}

    private IEnumerator LoadLevel(int levelBuildIndex)
    {
		enabled = false;
		if (_loadedLevelBuildIndex > 0)
        {
			yield return SceneManager.UnloadSceneAsync(_loadedLevelBuildIndex);
        }

		yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
		_loadedLevelBuildIndex = levelBuildIndex;
		enabled = true;
    }

    private void BeginNewGame()
    {
		Random.state = _mainRandomState;
		int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
		_mainRandomState = Random.state;
		Random.InitState(seed);
		_creationSpeedSlider.value = CreationSpeed = 0;
		_destructionSpeedSlider.value = DestructionSpeed = 0;

		foreach (var shape in _shapes)
        {
			shape.Recycle();
        }
		_shapes.Clear();
		_dyingShapeCount = 0;
    }

	private void DestroyShape()
    {
		if (_shapes.Count - _dyingShapeCount > 0)
		{
			Shape shape = _shapes[Random.Range(_dyingShapeCount, _shapes.Count)];
			if (_destroyDuration <= 0f)
			{
				KillImmediately(shape);
            }
            else
            {
				shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, _destroyDuration);
            }
		}
    }

	public override void Save(GameDataWriter writer)
    {
		writer.Write(_shapes.Count);
		writer.Write(Random.state);
		writer.Write(CreationSpeed);
		writer.Write(_creationProgress);
		writer.Write(DestructionSpeed);
		writer.Write(_destructionProgress);
		writer.Write(_loadedLevelBuildIndex);
		GameLevel.Current.Save(writer);
		for (int i = 0; i < _shapes.Count; i++)
		{
			writer.Write(_shapes[i].OriginFactory.FactoryId);
			writer.Write(_shapes[i].ShapeId);
			writer.Write(_shapes[i].MaterialId);
			_shapes[i].Save(writer);
        }
    }

	public override void Load (GameDataReader reader)
    {
		int version = reader.Version;
		if (version > SAVE_VERSION)
        {
			Debug.LogError("Unsupported future save version " + version);
			return;
		}

		StartCoroutine(LoadGame(reader));
	}

    private IEnumerator LoadGame(GameDataReader reader)
    {
		int version = reader.Version;
		int count = version <= 0 ? -version : reader.ReadInt();

		if (version >= 3)
		{
			Random.State state = reader.ReadRandomState();
			if (!_reseedOnLoad)
			{
				Random.state = state;
			}
			_creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
			_creationProgress = reader.ReadFloat();
			_destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
			_destructionProgress = reader.ReadFloat();
		}

		yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
		if (version >= 3)
        {
			GameLevel.Current.Load(reader);
        }
		for (int i = 0; i < count; i++)
		{
			int factoryId = version >= 5 ? reader.ReadInt() : 0;
			int shapeId = version > 0 ? reader.ReadInt() : 0;
			int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = _shapeFactories[factoryId].Get(shapeId, materialId);
			instance.Load(reader);
		}

		for (int i = 0; i < _shapes.Count; i++)
        {
			_shapes[i].ResolveShapeInstances();
        }
	}

	private void MarkAsDyingImmediately(Shape shape)
    {
		int index = shape.SaveIndex;

		if (index < _dyingShapeCount) return;

		_shapes[_dyingShapeCount].SaveIndex = index;
		_shapes[index] = _shapes[_dyingShapeCount];
		shape.SaveIndex = _dyingShapeCount;
		_shapes[_dyingShapeCount++] = shape;
    }
}
