using UnityEngine;

#if UNITY_EDITOR

public partial class GameLevel
{
    public bool HasLevelObject(GameLevelObject o)
    {
        if (_levelObjects != null)
        {
            for (int i = 0; i < _levelObjects.Length; i++)
            {
                if (_levelObjects[i] == o)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasMissingLevelObjects
    {
        get
        {
            if (_levelObjects != null)
            {
                for (int i = 0; i < _levelObjects.Length; i++)
                {
                    if (_levelObjects[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }

    public void RegisterLevelObject(GameLevelObject o)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Do not invoke in play mode");
            return;
        }

        if (HasLevelObject(o))
        {
            return;
        }

        if (_levelObjects == null)
        {
            _levelObjects = new GameLevelObject[] { o };
        }
        else
        {
            System.Array.Resize(ref _levelObjects, _levelObjects.Length + 1);
            _levelObjects[_levelObjects.Length - 1] = o;
        }
    }
    
    public void RemoveMissingLevelObjects()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Do not invoke in  Play Mode");
            return;
        }
        int holes = 0;
        for (int i = 0; i < _levelObjects.Length - holes; i++)
        {
            if (_levelObjects[i] == null)
            {
                holes += 1;
                System.Array.Copy(
                    _levelObjects, i + 1, _levelObjects, i,
                    _levelObjects.Length - i - holes
                );
                i -= 1;
            }
        }
        System.Array.Resize(ref _levelObjects, _levelObjects.Length - holes);
    }
    
}

#endif
