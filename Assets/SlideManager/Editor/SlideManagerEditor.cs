using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SlideManager))]
public class SlideManagerEditor : Editor
{
    private SlideManager slideManager;

    private void OnEnable()
    {
        slideManager = target as SlideManager;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Load slide"))
        {
            slideManager.LoadSlideImmediately(slideManager.currentSlideIndex);
        }
    }
}
