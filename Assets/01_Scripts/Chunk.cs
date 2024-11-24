using UnityEngine;
using System.Collections.Generic;

public class Chunk: MonoBehaviour //Que se pueda modificar el tamano del chunk en runtime? boton
{
    public struct PathPreviewData
    {
        public Vector2Int position;
        public List<Vector2Int> availableDirections;

        public PathPreviewData (Vector2Int position)
        {
            this.position = position;
            availableDirections = new (directions);
        }
    }
    public HashSet<Vector2Int> availableSides;

    private GameObject terrainPrefab;
    private GameObject pathPrefab;
    private List<PathPreviewData> pathPreview  = new();
    private HashSet<Vector2Int> pathPreviewHashset  = new();
    private Tile[] pathTiles;
    private Tile[] terrainTiles;
    private Vector2Int chunkSize;
    private Vector2Int chunkRelativePosition;
    private Vector2Int? startEdge;
    private Vector2Int pathStart;
    private Vector2Int? pathEnd;
    private bool lastPathReached = false;
    private bool canEnd = false;
    private static readonly Vector2Int[] directions =
    {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
    
    private Range chunkLimitsX;
    private Range chunkLimitsY;

    private struct Range
    {
        public int Min { get; }
        public int Max { get; }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    public Vector2Int Initialize( Vector2Int chunkDimensions, 
    Vector2Int startPathTile, Vector2Int relativePosition, GameObject terrainType, GameObject pathType)
    {
        pathStart = startPathTile;
        chunkRelativePosition = relativePosition;

        SetChunkSize(chunkDimensions);

        transform.position = new(
            relativePosition.x * chunkSize.x, 
            0, 
            relativePosition.y * chunkSize.y
        );

        terrainPrefab = terrainType;
        pathPrefab = pathType;

        startEdge = GetTileEdge(pathStart);
        
        GeneratePath();
        
        return pathEnd.Value;
    }

    private void SetChunkSize(Vector2Int dimensions)
    {
        chunkSize = dimensions;
        Vector2Int halfChunk = dimensions / 2;
        chunkLimitsX = new Range(-halfChunk.x, halfChunk.x);
        chunkLimitsY = new Range(-halfChunk.y, halfChunk.y);
    }

    private void GeneratePath()
    {
        PathPreviewData currentDataPath = new(pathStart);
        while (pathEnd == null)
        {
            pathPreview.Add(currentDataPath);
            pathPreviewHashset.Add(currentDataPath.position);
              
            if (lastPathReached){pathEnd = currentDataPath.position; return;}

            PathPreviewData? nextDataPath = GetNextDataPath(currentDataPath);

            while (nextDataPath == null)
            {
                if (pathPreview.Count == 1){throw new System.Exception("Path generation failed: No remaining paths to explore.");}

                pathPreview.Remove(currentDataPath);
                pathPreviewHashset.Remove(currentDataPath.position);

                currentDataPath = pathPreview[^1];
                nextDataPath = GetNextDataPath(currentDataPath);
            }
            
            currentDataPath = nextDataPath.Value;
        }
    }

    private PathPreviewData? GetNextDataPath(PathPreviewData currentPathData)
    {
        while (currentPathData.availableDirections.Count > 0)
        {
            Vector2Int chosenDirection = 
            currentPathData.availableDirections[Random.Range(0, currentPathData.availableDirections.Count)];
            PathPreviewData newPathData = new(currentPathData.position + chosenDirection);
            
            currentPathData.availableDirections.Remove(chosenDirection);

            if (IsPositionValid(newPathData.position)) 
            {
                return newPathData;
            }
        }

        return null;
    }

    private bool IsPositionValid(Vector2Int position)
    {
        if (TilePathIsInUse(position)){return false;}
        if (AreSurroundingTilesUsed(position)){return false;}
        
        if (CanEnd(position)){return lastPathReached = true;}
        if (IsInsideOfChunk(position)){return true;}

        return false;
    }

    private bool TilePathIsInUse(Vector2Int tilePosition)
    {
        return pathPreviewHashset.Contains(tilePosition);
    }

    private bool AreSurroundingTilesUsed(Vector2Int tilePosition)
    {
        int tilesUsed = 0;

        foreach (Vector2Int direction in directions)
        {
            if (TilePathIsInUse(tilePosition + direction)){tilesUsed ++;}
        }

        return tilesUsed > 1; //The tile behind is always used.
    }

    private bool IsInsideOfChunk(Vector2Int tilePosition)
    {   
        return tilePosition.x > chunkLimitsX.Min && tilePosition.x < chunkLimitsX.Max && 
        tilePosition.y > chunkLimitsY.Min && tilePosition.y < chunkLimitsY.Max;
    }

    private bool CanEnd(Vector2Int tilePosition)
    {   
        if (startEdge != null && !canEnd)
        {
            if (tilePosition.x * startEdge.Value.x > 0 || tilePosition.y * startEdge.Value.y > 0) 
            {                
                return false;
            }
            canEnd = true;    
        }
        
        return IsValidEdge(tilePosition);
    }

    private bool IsValidEdge(Vector2Int tilePosition)
    {
        Vector2Int? edge = GetTileEdge(tilePosition);

        if (edge == null) { return false; }
        if (edge == startEdge) { return false; }
        if (!availableSides.Contains(edge.Value)) { return false; }

        return true;
    }

    public void FillChunk()
    {
        pathTiles = new Tile[pathPreview.Count];
        terrainTiles = new Tile[chunkSize.x * chunkSize.y - pathPreview.Count];
        int terrainTileIndex = 0, pathTileIndex = 0;

        for (int x = chunkLimitsX.Min; x <= chunkLimitsX.Max; x++)
        {
            for (int y = chunkLimitsY.Min; y <= chunkLimitsY.Max; y++)
            {
                Vector2Int tilePosition = new(x, y);

                Vector3 tileWorldPosition = new 
                (
                    tilePosition.x + chunkSize.x * chunkRelativePosition.x, 
                    0, 
                    tilePosition.y + chunkSize.y * chunkRelativePosition.y
                );

                bool isPathTile = TilePathIsInUse(tilePosition);
                GameObject prefab = isPathTile ? pathPrefab : terrainPrefab;
                GameObject tileGO = InstanceTile(tilePosition, tileWorldPosition, prefab);

                Tile tile = new(tilePosition, tileWorldPosition, prefab, this, tileGO);

                if (isPathTile) {pathTiles[pathTileIndex++] = tile;}  
                else {terrainTiles[terrainTileIndex++] = tile;}
            }
        }
    }

    private GameObject InstanceTile(Vector2Int positionInChunk, Vector3 globalPosition, GameObject prefab)
    {
        GameObject tileGO = Instantiate(prefab, globalPosition, Quaternion.identity, this.transform);
        tileGO.name = $"{prefab.name} {positionInChunk}";

        return tileGO;
    }

    public Vector2Int? GetTileEdge (Vector2Int pathTile)
    {
        if (pathTile.x == chunkLimitsX.Max){return Vector2Int.right;}
        if (pathTile.x == chunkLimitsX.Min){return Vector2Int.left;}
        if (pathTile.y == chunkLimitsY.Max){return Vector2Int.up;}
        if (pathTile.y == chunkLimitsY.Min){return Vector2Int.down;}

        return null;
    }

    public Vector2Int GetEndEdge()
    {
        return GetTileEdge(pathEnd.Value).Value;
    }

    public Vector2Int RestartPathGenerated()
    {
        pathPreview.Clear();
        pathPreviewHashset.Clear();
        pathEnd = null;
        lastPathReached = false;
        canEnd = false;

        GeneratePath();

        return pathEnd.Value;
    }
}
