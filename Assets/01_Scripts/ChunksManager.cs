using System.Collections.Generic;
using UnityEngine;

//Comentar
//Encapsular

public class ChunksManager : MonoBehaviour
{
    [SerializeField] private RandomSeedController randomSeedController;
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private Vector2Int chunkSize = new(13, 13);
    [SerializeField] private int numberOfChunks = 4;
    

    private Chunk[] chunks;
    private HashSet<Vector2Int> chunksPositions = new();
    private HashSet<Vector2Int> forbiddenPositions = new();
    private static readonly Vector2Int[] directions =
    {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

    private readonly int iterationLimit = 100000; // Límite de iteraciones
    private int iterationCount = 0;

    private void Start()
    {
        if (Mathf.Min(chunkSize.x, chunkSize.y) < 3){throw new System.Exception ("Chunk size has to be at least (3,3).");}

        if (randomSeedController == null || terrainPrefab == null || pathPrefab == null)
            throw new System.Exception("Please ensure all serialized fields are assigned in the inspector.");

        chunks = new Chunk[numberOfChunks];
        randomSeedController.SetSeed();
        InitializeChunks();
    }

    private void InitializeChunks() //Problemas con chunks de tamano par
    {
        Vector2Int startTilePosition = Vector2Int.zero;
        Vector2Int currentChunkPosition = Vector2Int.zero;
        HashSet<Vector2Int> availableSides = new(directions);

        for (int c = 0; c < numberOfChunks; c++)
        {
            GameObject chunkGO = new GameObject($"Chunk {currentChunkPosition}");
            Chunk chunk = chunkGO.AddComponent<Chunk>();

            chunksPositions.Add(currentChunkPosition);
            chunks[c] = chunk;
            
            chunk.availableSides = availableSides;
            Vector2Int endTilePosition = chunk.Initialize(chunkSize, startTilePosition, currentChunkPosition, terrainPrefab, pathPrefab);
            
            Vector2Int? nextChunkDirection = chunk.GetTileEdge(endTilePosition);
            currentChunkPosition += nextChunkDirection.Value;

            availableSides = GetAvailableSides(currentChunkPosition);

            while (availableSides.Count == 0)
            {
                if (iterationCount++ > iterationLimit){throw new System.Exception("Bucle infinito detectado en la generación de chunks.");}

                if (GetPreviousChunk(ref c, ref chunk, ref currentChunkPosition, ref nextChunkDirection, ref endTilePosition, ref availableSides))
                {
                    throw new System.Exception("Chunk generation failed: No available sides.");
                }

            }
            startTilePosition = nextChunkDirection.Value.x != 0 ? 
            new Vector2Int(-startTilePosition.x, startTilePosition.y) : 
            new Vector2Int(startTilePosition.x, -startTilePosition.y);
        }

        FillAllChunks();
    }

    private bool GetPreviousChunk(ref int c, ref Chunk chunk, ref Vector2Int currentChunkPosition, 
        ref Vector2Int? nextChunkDirection, ref Vector2Int endTilePosition, ref HashSet<Vector2Int> availableSides)
    {
        if (c == 0) return true;

        forbiddenPositions.Add(currentChunkPosition);
        currentChunkPosition -= nextChunkDirection.Value;
        chunk.availableSides.Remove(nextChunkDirection.Value);

        // If there are available sides, restart the path and update positions
        if (chunk.availableSides.Count > 0)
        {
            endTilePosition = chunk.RestartPathGenerated();
            nextChunkDirection = chunk.GetTileEdge(endTilePosition);
            currentChunkPosition += nextChunkDirection.Value;
            availableSides = GetAvailableSides(currentChunkPosition);
            return false;
        }

        // If no available sides, destroy the chunk and move back to the previous chunk
        Destroy(chunk.gameObject);
        chunksPositions.Remove(currentChunkPosition);
        
        c--;
        chunk = chunks[c];
        nextChunkDirection = chunk.GetEndEdge();

        return false;
    }

    private HashSet<Vector2Int> GetAvailableSides(Vector2Int chunkPosition)
    {
        HashSet<Vector2Int> availableSides = new();

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

    private void FillAllChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            chunk.FillChunk();
        }
    }
}