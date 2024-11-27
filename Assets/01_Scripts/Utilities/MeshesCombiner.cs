using UnityEngine;
using System.Collections.Generic;

public class MeshesCombiner : MonoBehaviour
{
    private const int MaxVerticesPerMesh = 65000;
    private GameObject mainObject;

    public void CombineMeshesInChildren()
    {
        mainObject = new GameObject("Combined Meshes");
        mainObject.transform.SetParent(transform, false);

        MeshFilter[] childMeshFilters = GetComponentsInChildren<MeshFilter>();
        if (childMeshFilters.Length == 0) return;

        Dictionary<Material, List<CombineInstance>> materialToCombineInstancesMap = new();

        foreach (var meshFilter in childMeshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer == null) continue;

            CombineInstance combineInstance = new()
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix * transform.worldToLocalMatrix
            };

            Material sharedMaterial = meshRenderer.sharedMaterial;
            if (!materialToCombineInstancesMap.TryGetValue(sharedMaterial, out var combineList))
            {
                combineList = new List<CombineInstance>();
                materialToCombineInstancesMap[sharedMaterial] = combineList;
            }
            combineList.Add(combineInstance);

            meshRenderer.gameObject.SetActive(false);
        }

        foreach (var kvp in materialToCombineInstancesMap)
        {
            CheckVertexCount(kvp.Value, kvp.Key);
        }
    }

    private void CheckVertexCount(List<CombineInstance> combineInstances, Material material)
    {
        List<CombineInstance> currentCombinedInstance = new List<CombineInstance>(MaxVerticesPerMesh / 100); // Pre-allocate
        int currentVertexCount = 0;

        foreach (var combineInstance in combineInstances)
        {
            int vertexCount = combineInstance.mesh.vertexCount;
            if (currentVertexCount + vertexCount > MaxVerticesPerMesh)
            {
                CreateMesh(currentCombinedInstance.ToArray(), material);
                currentCombinedInstance.Clear();
                currentVertexCount = 0;
            }

            currentCombinedInstance.Add(combineInstance);
            currentVertexCount += vertexCount;
        }

        if (currentCombinedInstance.Count > 0)
        {
            CreateMesh(currentCombinedInstance.ToArray(), material);
        }
    }

    private void CreateMesh(CombineInstance[] combineInstances, Material material)
    {
        if (combineInstances.Length == 0) return;

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances, true, true);

        GameObject combinedMeshObject = new ($"CombinedMesh of {material.name}");
        combinedMeshObject.transform.SetParent(mainObject.transform, false);

        MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
        combinedMeshFilter.sharedMesh = combinedMesh;

        MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
        combinedMeshRenderer.material = material;
    }
}
