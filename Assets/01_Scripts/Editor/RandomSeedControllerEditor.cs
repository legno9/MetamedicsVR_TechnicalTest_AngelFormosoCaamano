using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomSeedController))]
public class RandomSeedControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RandomSeedController controller = (RandomSeedController)target;

        // Begin horizontal layout for buttons
        GUILayout.BeginHorizontal();

        // Button to generate a new random seed
        if (GUILayout.Button("Randomize Seed"))
        {
            controller.GenerateRandomSeed();
        }

        // Button to copy the current seed to the clipboard
        if (GUILayout.Button("Copy Seed"))
        {
            controller.CopySeedToClipboard();
        }

        // End horizontal layout
        GUILayout.EndHorizontal();
    }
}
