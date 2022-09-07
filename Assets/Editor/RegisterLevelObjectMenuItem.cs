using UnityEditor;
using UnityEngine;

public static class RegisterLevelObjectMenuItem
{
    private const string _menuItem = "GameObject/Register Level Object";

    [MenuItem(_menuItem, true)]
    private static bool ValidateRegisterLevelObject()
    {
        if (Selection.objects.Length == 0)
        {
            return false;
        }

        foreach (var o in Selection.objects)
        {
            if (!(o is GameObject))
            {
                return false;
            }
        }
        return true;
    }

    [MenuItem(_menuItem)]
    public static void RegisterLevelObject()
    {
        foreach (var o in Selection.objects)
        {
            Register(o as GameObject);
        }
    }

    private static void Register(GameObject o)
    {
        if (PrefabUtility.GetPrefabAssetType(o) == PrefabAssetType.Model)
        {
            Debug.LogWarning(o.name + " is a prefab asset.", o);
            return;
        }

        var levelObject = o.GetComponent<GameLevelObject>();
        if (levelObject == null)
        {
            Debug.LogWarning(o.name + " isn't a game level object.", o);
            return;
        }

        foreach (GameObject rootObject in o.scene.GetRootGameObjects())
        {
            var gameLevel = rootObject.GetComponent<GameLevel>();
            if (gameLevel != null)
            {
                if (gameLevel.HasLevelObject(levelObject))
                {
                    Debug.LogWarning((o.name + " is already registered.", o));
                    return;
                }

                Undo.RecordObject(gameLevel, "Register Level Object.");
                gameLevel.RegisterLevelObject(levelObject);
                Debug.Log(
                    o.name + " registered to game level " +
                    gameLevel.name + " in scene " + o.scene.name + ".", o
                );

                return;
            }
        }
        Debug.LogWarning((o.name + " isn't a part of gameLevel.", o));
    }
}
