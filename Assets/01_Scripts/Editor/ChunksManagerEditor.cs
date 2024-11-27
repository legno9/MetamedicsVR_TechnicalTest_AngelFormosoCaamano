using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunksManager))]
public class ChunksManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector UI
        DrawDefaultInspector();

        // Get the target ChunksManager instance
        ChunksManager chunksManager = (ChunksManager)target;

        // Only show buttons if the application is playing
        if (!Application.isPlaying) return;

        // Button to initialize chunks
        if (GUILayout.Button("Initialize Chunks"))
        {
            chunksManager.StartChunkManager();
        }

        // Button to remove all chunks
        if (GUILayout.Button("Remove All Chunks"))
        {
            chunksManager.RemoveAllChunks();
        }
    }
}
