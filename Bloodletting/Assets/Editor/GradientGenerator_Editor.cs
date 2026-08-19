using UnityEngine;
using UnityEditor;

// reference: LKHGames Shader Graph Exposed Gradient https://www.youtube.com/watch?v=K3dPvA2MtTY

[CustomEditor(typeof(GradientGenerator))]
public class GradientGenerator_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GradientGenerator gradientGenerator = (GradientGenerator)target;

        if (GUILayout.Button("Generate PNG Gradient Texture"))
        {
            gradientGenerator.BakeGradientTexture();
            AssetDatabase.Refresh();
        }
    }
}
