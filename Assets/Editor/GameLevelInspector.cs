using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevel))]
public class GameLevelInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gameLevel = (GameLevel)target;
        if (gameLevel.HasMissingLevelObjects)
        {
            EditorGUILayout.HelpBox("Missing Level Objects", MessageType.Error);
            if (GUILayout.Button("Remove Missing Objects"))
            {
                Undo.RecordObject(gameLevel, "Remove Missing Level Objects.");
                gameLevel.RemoveMissingLevelObjects();
            }
        }
    }
}
