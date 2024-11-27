using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomSeedController))]
public class RandomSeedControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RandomSeedController controller = (RandomSeedController)target;

        // Inicia un grupo horizontal
        GUILayout.BeginHorizontal();

        // Botón para generar una nueva semilla
        if (GUILayout.Button("Randomize Seed"))
        {
            controller.GenerateRandomSeed();
        }

        // Botón para copiar la semilla al portapapeles
        if (GUILayout.Button("Copy Seed"))
        {
            controller.CopySeedToClipboard();
        }

        // Termina el grupo horizontal
        GUILayout.EndHorizontal();
    }
}

