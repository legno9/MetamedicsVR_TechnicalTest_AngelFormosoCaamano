using UnityEngine;
using System.Collections.Generic;

public class MeshesCombiner : MonoBehaviour
{
    private const int MaxVerticesPerMesh = 65000;
    private GameObject mainObject;

    public void CombineMeshesInChildren()
    {
        // Create a new GameObject to hold the combined meshes
        mainObject = new GameObject("Combined Meshes");
        mainObject.transform.SetParent(transform, false);

        MeshFilter[] childMeshFilters = GetComponentsInChildren<MeshFilter>();
        if (childMeshFilters.Length == 0) return;

        // Dictionary to map materials to their corresponding combine instances
        Dictionary<Material, List<CombineInstance>> materialToCombineInstancesMap = new();

        foreach (var meshFilter in childMeshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer == null) continue;

            // Create a CombineInstance for the current mesh
            CombineInstance combineInstance = new()
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix * transform.worldToLocalMatrix
            };

            Material sharedMaterial = meshRenderer.sharedMaterial;
            // Add the combine instance to the list for the corresponding material
            if (!materialToCombineInstancesMap.TryGetValue(sharedMaterial, out var combineList))
            {
                combineList = new List<CombineInstance>();
                materialToCombineInstancesMap[sharedMaterial] = combineList;
            }
            combineList.Add(combineInstance);

            meshRenderer.gameObject.SetActive(false);
        }

        // Check vertex count and create combined meshes for each material
        foreach (var kvp in materialToCombineInstancesMap)
        {
            CheckVertexCount(kvp.Value, kvp.Key);
        }
    }

    private void CheckVertexCount(List<CombineInstance> combineInstances, Material material)
    {
        // List to hold combine instances for the current combined mesh
        List<CombineInstance> currentCombinedInstance = new List<CombineInstance>(MaxVerticesPerMesh / 100); // Pre-allocate
        int currentVertexCount = 0; // Track the current vertex count

        foreach (var combineInstance in combineInstances)
        {
            int vertexCount = combineInstance.mesh.vertexCount;
            // If adding the current mesh exceeds the max vertex count, create a new combined mesh
            if (currentVertexCount + vertexCount > MaxVerticesPerMesh)
            {
                CreateMesh(currentCombinedInstance.ToArray(), material);
                currentCombinedInstance.Clear();
                currentVertexCount = 0;
            }

            currentCombinedInstance.Add(combineInstance);
            currentVertexCount += vertexCount;
        }

        // Create a mesh for any remaining combine instances
        if (currentCombinedInstance.Count > 0)
        {
            CreateMesh(currentCombinedInstance.ToArray(), material);
        }
    }

    private void CreateMesh(CombineInstance[] combineInstances, Material material)
    {
        if (combineInstances.Length == 0) return;

        // Create a new mesh and combine the provided instances
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances, true, true);

        // Create a new GameObject for the combined mesh
        GameObject combinedMeshObject = new ($"CombinedMesh of {material.name}");
        combinedMeshObject.transform.SetParent(mainObject.transform, false);

        // Add a MeshFilter and assign the combined mesh
        MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
        combinedMeshFilter.sharedMesh = combinedMesh;

        // Add a MeshRenderer and assign the material
        MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
        combinedMeshRenderer.material = material;
    }
}
