using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunksManager))]
public class ChunksManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Llama al Inspector predeterminado para mostrar las variables
        DrawDefaultInspector();

        // Referencia al script que estamos editando
        ChunksManager chunksManager = (ChunksManager)target;

        if (!Application.isPlaying) return;

        // Agregar un botón al Inspector
        if (GUILayout.Button("Initialize Chunks"))
        {
            // Llama al método del script cuando se presiona el botón
            chunksManager.StartChunkManager();
        }

        if (GUILayout.Button("Remove All Chunks"))
        {
            // Llama al método del script cuando se presiona el botón
            chunksManager.RemoveAllChunks();
        }
    }
}
