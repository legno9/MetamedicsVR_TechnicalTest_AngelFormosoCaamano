using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshesCombiner))]
[RequireComponent(typeof(RandomSeedController))]

public class ChunksManager : MonoBehaviour
{
    [SerializeField] private RandomSeedController randomSeedController;
    [SerializeField] private MeshesCombiner meshesCombiner;
    [Space]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject pathPrefab;
    [Space]
    [SerializeField] private Vector2Int chunkSize = new(13, 13);
    [SerializeField, Min(0)] private int numberOfChunks = 4;
    [Space]
    [SerializeField] private bool CombineAllChunks = true;
    [Space]
    [Range(0f, 100f)]
    [SerializeField] private float SecondPathChance = 10f;
    [Range(0f, 1f)]
    [SerializeField] private float ExpansionFactor = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float IrregularityFactor = 0.5f;

    private Chunk[] chunks;
    private HashSet<Vector2Int> chunksPositions;
    private HashSet<Vector2Int> forbiddenPositions;
    private List<Vector2Int> endTilePositions;
    private List<Vector2Int> currentChunkPositions;
    private static readonly Vector2Int[] directions =
    {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
    private GameObject chunksParent;

    private readonly int iterationLimit = 100000; // Límite de iteraciones
    private int iterationCount = 0;

    public void StartChunkManager()
    {
        // Ensure the chunk size is at least (3,3) to avoid errors in generation
        if (Mathf.Min(chunkSize.x, chunkSize.y) < 3)
        {throw new System.Exception ("Chunk size has to be at least (3,3).");}

        if (randomSeedController == null || terrainPrefab == null || pathPrefab == null)
            throw new System.Exception("Please ensure all serialized fields are assigned in the inspector.");

        // Deactivate all child objects of the current transform
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        chunksParent = new GameObject("Chunks");
        chunksParent.transform.SetParent(transform, false);

        chunks = new Chunk[numberOfChunks];

        chunksPositions = new();
        endTilePositions = new();
        currentChunkPositions = new();
        forbiddenPositions = new();
        randomSeedController.SetSeed();

        InitializeChunks();
    }

    private void InitializeChunks()
    {
        // Start at the origin for the first chunk
        Vector2Int startTilePosition = Vector2Int.zero;
        HashSet<Vector2Int> availableSides = new(directions);

        int index = 0;
        int endTileCounter = 1;
        currentChunkPositions.Add(Vector2Int.zero);

        // Loop through and create each chunk
        for (int c = 0; c < numberOfChunks; c++)
        {
            // Create a new GameObject for the chunk and set its parent
            GameObject chunkGO = new GameObject($"Chunk {currentChunkPositions[index]}");
            chunkGO.transform.parent = chunksParent.transform;
            Chunk chunk = chunkGO.AddComponent<Chunk>();
            chunk.AddComponent<MeshesCombiner>();

            chunksPositions.Add(currentChunkPositions[index]);
            chunks[c] = chunk;

            // Assign available sides to the chunk
            chunk.availableSides = availableSides;
            
            Vector2Int endTilePosition = chunk.Initialize(chunkSize, startTilePosition,
            currentChunkPositions[index], terrainPrefab, pathPrefab, ExpansionFactor, IrregularityFactor);

            if (endTilePositions.Count == 0){endTilePositions.Add(endTilePosition);}
            else {endTilePositions[index] = endTilePosition;}

            // Determine if a secondary path should be generated
            if (Random.Range(0f, 100f) < SecondPathChance)
            {
                Vector2Int? secondaryPathEnd = chunk.GenerateSecondaryPath();
                if (secondaryPathEnd.HasValue)
                {
                    endTilePositions.Add(secondaryPathEnd.Value);
                    currentChunkPositions.Add(currentChunkPositions[index]);
                }
            }

            index++;

            bool validChunkFound = false;
            Vector2Int nextChunkDirection;

            while (!validChunkFound)
            {
                if (index >= endTileCounter)
                {
                    endTileCounter = endTilePositions.Count;
                    index = 0;
                }  

                // Get the direction for the next chunk based on the current end tile
                nextChunkDirection = chunk.GetTileEdge(endTilePositions[index]).Value;
                currentChunkPositions[index] += nextChunkDirection;
                
                availableSides = GetAvailableSides(currentChunkPositions[index]);

                if (availableSides.Count > 0)
                {   
                    validChunkFound = true;
                    startTilePosition = GetStartTilePosition(endTilePositions[index], nextChunkDirection);
                }
                else if (endTilePositions.Count == 1)
                {
                    // Handle the case where only one end tile is available
                    while (availableSides.Count == 0)
                    {
                        if (iterationCount++ > iterationLimit)
                        { throw new System.Exception("Infinite loop detected in chunk generation."); }

                        if (GetPreviousChunk(ref c, index, ref chunk, ref currentChunkPositions, ref nextChunkDirection,
                        ref endTilePositions, ref availableSides))
                        {
                            throw new System.Exception("Chunk generation failed: No available sides.");
                        }
                    }
                    validChunkFound = true;
                    startTilePosition = GetStartTilePosition(endTilePositions[index], nextChunkDirection);
                }  
                else
                {
                    // Remove the current position and end tile if no valid sides are found
                    currentChunkPositions.RemoveAt(index);
                    endTilePositions.RemoveAt(index);
                    endTileCounter--;
                }
            }
        }
        FillAllChunks();
    }

    private bool GetPreviousChunk(ref int c, int index, ref Chunk chunk, ref List<Vector2Int> currentChunkPositions, 
        ref Vector2Int nextChunkDirection, ref List<Vector2Int> endTilePositions, ref HashSet<Vector2Int> availableSides)
    {
        if (c == 0) return true;

        forbiddenPositions.Add(currentChunkPositions[index]);
        currentChunkPositions[index] -= nextChunkDirection;
        chunk.availableSides.Remove(nextChunkDirection);

        // If there are available sides, restart the path and update positions
        if (chunk.availableSides.Count > 0)
        {
            endTilePositions[index] = chunk.RestartPathGenerated();
            nextChunkDirection = chunk.GetTileEdge(endTilePositions[index]).Value;
            currentChunkPositions[index] += nextChunkDirection;
            availableSides = GetAvailableSides(currentChunkPositions[index]);

            return false;
        }

        // If no available sides, destroy the chunk and move back to the previous chunk
        Destroy(chunk.gameObject);
        chunksPositions.Remove(currentChunkPositions[index]);
        
        c--;
        chunk = chunks[c];
        nextChunkDirection = chunk.endEdge;

        return false;
    }

    private HashSet<Vector2Int> GetAvailableSides(Vector2Int chunkPosition)
    {
        HashSet<Vector2Int> availableSides = new();

        // If the current chunk position is already occupied, return an empty set
        if (chunksPositions.Contains(chunkPosition))
        {
            return availableSides;
        }

        // Iterate over all possible directions to find available sides
        foreach (Vector2Int direction in directions)
        {
            Vector2Int potentialPosition = chunkPosition + direction;
            if (!chunksPositions.Contains(potentialPosition) && !forbiddenPositions.Contains(potentialPosition))
            {
                availableSides.Add(direction);
            }
        }

        return new HashSet<Vector2Int>(availableSides);
    }

    private Vector2Int GetStartTilePosition(Vector2Int endTilePosition, Vector2Int? nextChunkDirection)
    {
        // Determine the start tile position based on the end tile position and the direction of the next chunk
        if (nextChunkDirection.Value.x != 0 && chunkSize.x % 2 == 0 )
        {
            // Adjust position for even-sized chunks in the x-direction
            return new (-(endTilePosition.x -1), endTilePosition.y);
        }
        else if (nextChunkDirection.Value.y != 0 && chunkSize.y % 2 == 0)
        {
            return new (endTilePosition.x, -(endTilePosition.y + 1));
        }
        else
        {
            // Default adjustment for odd-sized chunks
            return nextChunkDirection.Value.x != 0 ?
                    new (-endTilePosition.x, endTilePosition.y) :
                    new (endTilePosition.x, -endTilePosition.y);
        }   
    }

    private void FillAllChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.FillChunk();
        }

        if (CombineAllChunks)
        {
            meshesCombiner.CombineMeshesInChildren();
        }   
    }

    public void RemoveAllChunks()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}