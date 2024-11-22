
using UnityEngine;

public class Tile
{
    public Vector2Int RelativePosition { get; private set; }
    public Vector3 WorldPosition { get; private set; } 
    public Chunk ChunkParent { get; private set; }
    public GameObject TileGO { get; private set; }
    public GameObject Type { get; private set; }

    public Tile(Vector2Int positionInChunk, Vector3 globalPosition, GameObject prefab, Chunk parent, GameObject tileObject)
    {
        RelativePosition = positionInChunk;
        WorldPosition = globalPosition;
        Type = prefab;
        ChunkParent = parent;
        TileGO = tileObject;
    }

}
