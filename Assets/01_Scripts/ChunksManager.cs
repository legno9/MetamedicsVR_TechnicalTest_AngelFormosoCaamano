using System.Collections.Generic;
using UnityEngine;

//Comentar
//Encapsular

public class ChunksManager : MonoBehaviour
{
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private Vector2Int chunkSize = new (13, 13);
    [SerializeField] private int numberOfChunks = 4;

    private Chunk[] chunks;
    private HashSet<Vector2Int> chunksPositions = new();

    public static ChunksManager Instance { get; private set; }

    private void Awake() 
    {
        if (Instance != null && Instance != this) {Destroy(this);}
        else {Instance = this;} 
    }

    private void Start()
    {
        chunks = new Chunk[numberOfChunks];
        GetComponent<RandomSeedController>().SetSeed();
        InitializeChunks();
    }

    private void InitializeChunks() //Problemas con chunks de tamano par y si es demasiado pequeno
    {
        Vector2Int startTilePosition = Vector2Int.zero;
        Vector2Int endTilePosition;
        Vector2Int currentChunkPosition = Vector2Int.zero;
        
        for (int c = 0; c< numberOfChunks; c++)
        {
            GameObject chunkGO = new($"Chunk {currentChunkPosition}");
            Chunk chunk = chunkGO.AddComponent<Chunk>();

            chunksPositions.Add(currentChunkPosition);
            chunks[c] = chunk;

            endTilePosition = chunk.Initialize(
                chunkSize, 
                startTilePosition, 
                currentChunkPosition, 
                terrainPrefab, 
                pathPrefab
            );

            Vector2Int? nextChunkDirection = chunk.GetTileEdge(endTilePosition);
            if (nextChunkDirection == null) {return;}

            currentChunkPosition += nextChunkDirection.Value;

            if (nextChunkDirection.Value.x != 0) {endTilePosition.x *= -1;}
            else {endTilePosition.y *= -1;}

            startTilePosition = endTilePosition;
        }                
    }

    public bool IsChunkAtPosition (Vector2Int position) //Se puede quedar bloqueado en una espiral.Tengo que hacer lo mismo que para las tiles.
    {
        return chunksPositions.Contains(position);
    }

}
