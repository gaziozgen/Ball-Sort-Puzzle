using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelGenerator generator = (LevelGenerator)target;

        LevelGenerator.Difficulty = EditorGUILayout.Slider("Difficulty", LevelGenerator.Difficulty, 0, 1);
        if (GUILayout.Button("Generate Level"))
        {
            generator.GenerateLevel();
            EditorUtility.SetDirty(generator);
        }
    }
}