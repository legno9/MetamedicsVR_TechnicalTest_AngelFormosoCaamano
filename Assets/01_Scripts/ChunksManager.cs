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

    private void Start()
    {
        chunks = new Chunk[numberOfChunks];
        InitializeChunks();
    }

    private void InitializeChunks() //Problemas con chunks de tamano par y si es demasiado pequeno
    {
        Vector2Int currentTilePosition = Vector2Int.zero;
        Vector2Int currentChunkPosition = Vector2Int.zero;
        
        for (int c = 0; c< numberOfChunks; c++)
        {
            GameObject chunkGO = new($"Chunk {currentChunkPosition}");
            Chunk chunk = chunkGO.AddComponent<Chunk>();
            chunks[c] = chunk;

            currentTilePosition = chunk.Initialize(
                chunkSize, 
                currentTilePosition, 
                currentChunkPosition, 
                terrainPrefab, 
                pathPrefab
            );//Singleton?


            Vector2Int? nextChunkDirection = chunk.GetTileEdge(currentTilePosition);
            if (nextChunkDirection == null) {return;}

            currentChunkPosition += nextChunkDirection.Value;

            if (nextChunkDirection.Value.x != 0) {currentTilePosition.x *= -1;}
            else {currentTilePosition.y *= -1;}
        }                
    }

    public bool IsValidNextChunk()
    {
        return true;
    }

}
