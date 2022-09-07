using System;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    private MeshRenderer[] _meshRenderers;

    private int _shapeId = int.MinValue;

    public int ShapeId
    {
        get
        {
            return _shapeId;
        }
        set
        {
            if (_shapeId == int.MinValue && value != int.MinValue)
            {
                _shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change ShapeId.");
            }
        }
    }

    private ShapeFactory _originFactory;
    public ShapeFactory OriginFactory
    {
        get
        {
            return _originFactory;
        }
        set
        {
            if (_originFactory == null)
            {
                _originFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change origin factory.");
            }
        }
    }

    public int ColorCount => _colors.Length;

    public int MaterialId { get; private set; }
    public float Age { get; private set; }
    public int InstanceId { get; private set; }
    public int SaveIndex { get; set; }
    public bool IsMarkedAsDying => Game.Instance.IsMarkedAsDying(this);

    private Color[] _colors;
    private static int _colorPropertyId = Shader.PropertyToID("_Color");
    private static MaterialPropertyBlock _sharedPropertyBlock;
    private List<ShapeBehavior> _behaviorList = new List<ShapeBehavior>();


    private void Awake()
    {
        _colors = new Color[_meshRenderers.Length];
    }

    public T AddBehavior<T>() where T : ShapeBehavior, new()
    {
        T behavior = ShapeBehaviorPool<T>.Get();
        _behaviorList.Add(behavior);
        return behavior;
    }

    public void Recycle()
    {
        Age = 0f;
        InstanceId += 1;
        for (int i = 0; i < _behaviorList.Count; i++)
        {
            _behaviorList[i].Recycle();
        }
        _behaviorList.Clear();
        OriginFactory.Reclaim(this);
    }

    public void Die()
    {
        Game.Instance.Kill(this);
    }

    public void GameUpdate()
    {
        Age += Time.deltaTime;
        for (int i = 0; i < _behaviorList.Count; i++)
        {
            if (!_behaviorList[i].GameUpdate(this))
            {
                _behaviorList[i].Recycle();
                _behaviorList.RemoveAt(i--);
            }
        }
    }

    public void MarkAsDying()
    {
        Game.Instance.MarkAsDying(this);
    }

    public void SetMaterial(Material material, int materialId)
    {
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material = material;
        }
        MaterialId = materialId;
    }

    public void SetColor(Color color)
    {
        if (_sharedPropertyBlock == null)
        {
            _sharedPropertyBlock = new MaterialPropertyBlock();
        }
        _sharedPropertyBlock.SetColor(_colorPropertyId, color);
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _colors[i] = color;
            _meshRenderers[i].SetPropertyBlock(_sharedPropertyBlock);
        }
    }

    public void SetColor(Color color, int index)
    {
        if (_sharedPropertyBlock == null)
        {
            _sharedPropertyBlock = new MaterialPropertyBlock();
        }
        _sharedPropertyBlock.SetColor(_colorPropertyId, color);
        _colors[index] = color;
        _meshRenderers[index].SetPropertyBlock(_sharedPropertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(_colors.Length);
        for (int i = 0; i < _colors.Length; i++)
        {
            writer.Write(_colors[i]);
        }
        writer.Write(Age);
        writer.Write(_behaviorList.Count);
        for (int i = 0; i < _behaviorList.Count; i++)
        {
            writer.Write((int)_behaviorList[i].BehaviorType);
            _behaviorList[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if (reader.Version >= 5)
        {
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        if (reader.Version >= 6)
        {
            Age = reader.ReadFloat();
            int behaviorCount = reader.ReadInt();
            for (int i = 0; i < behaviorCount; i++)
            {
                ShapeBehavior behavior = ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
                _behaviorList.Add(behavior);
                behavior.Load(reader);
            }
        } else if (reader.Version >= 4)
        {
            AddBehavior<RotationShapeBehavior>().AngularVelocity = reader.ReadVector3();
            AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
        }
    }

    private void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= _colors.Length ? count : _colors.Length;
        int i = 0;
        for (; i < max; i++)
        {
            SetColor(reader.ReadColor(), i);
        }

        if (count > _colors.Length)
        {
            for (; i < count; i++)
            {
                reader.ReadColor();
            }
        }
        else if (count < _colors.Length)
        {
            for (; i < count; i++)
            {
                SetColor(Color.white, i);
            }
        }

    }

    public void ResolveShapeInstances()
    {
        for (int i = 0; i < _behaviorList.Count; i++)
        {
            _behaviorList[i].ResolveShapeInstances();
        }
    }
}

public struct ShapeInstance
{
    public ShapeInstance(Shape shape)
    {
        Shape = shape;
        instanceIdOrSaveIndex = shape.InstanceId;
    }

    public ShapeInstance(int saveIndex)
    {
        Shape = null;
        instanceIdOrSaveIndex = saveIndex;
    }

    public bool IsValid => (Shape && instanceIdOrSaveIndex == Shape.InstanceId);
    public Shape Shape { get; private set; }
    private int instanceIdOrSaveIndex;

    public static implicit operator ShapeInstance(Shape shape)
    {
        return new ShapeInstance(shape);
    }

    public void Resolve()
    {
        Shape = Game.Instance.GetShape(instanceIdOrSaveIndex);
        instanceIdOrSaveIndex = Shape.InstanceId;
    }
}
