using System;
using UnityEngine;
using UnityEngine.Serialization;

public partial class GameLevel : PersistableObject
{
    [SerializeField]
    private SpawnZone _spawnZone;

    [SerializeField]
    private int _populationLimit;

    [SerializeField][FormerlySerializedAs("_persistentObjects")] GameLevelObject[] _levelObjects;

    public static GameLevel Current { get; private set; }
    public int PopulationLimit => _populationLimit;
    
    private void OnEnable()
    {
        Current = this;
        _levelObjects ??= Array.Empty<GameLevelObject>();
    }

    public void GameUpdate()
    {
        for (int i = 0; i < _levelObjects.Length; i++)
        {
            _levelObjects[i].GameUpdate();
        }
    }
    
    public override void Save(GameDataWriter writer)
    {
        writer.Write(_levelObjects.Length);
        for(int i = 0; i < _levelObjects.Length; i++)
        {
            _levelObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int savedCount = reader.ReadInt();
        for (int i = 0; i < savedCount; i++)
        {
            _levelObjects[i].Load(reader);
        }
    }

    public void SpawnShapes()
    {
        _spawnZone.SpawnShapes();
    }

}
