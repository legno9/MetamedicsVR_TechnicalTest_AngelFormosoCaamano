using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Chunk: MonoBehaviour
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
    [HideInInspector]public Vector2Int endEdge;
    [HideInInspector]public Vector2Int secondaryEndEdge;

    private MeshesCombiner meshesCombiner;
    private GameObject terrainPrefab;
    private GameObject pathPrefab;
    private List<PathPreviewData> pathPreview  = new();
    private HashSet<Vector2Int> pathPreviewHashset  = new();
    private HashSet<Vector2Int> forbiddenTiles = new();
    private Tile[] pathTiles;
    private Tile[] terrainTiles;
    private Vector2Int chunkSize;
    private Vector2Int chunkRelativePosition;
    private Vector2Int? startEdge;
    private Vector2Int pathStart;
    private Vector2Int? pathEnd;
    private Vector2Int? secondaryPathEnd;
    private bool lastPathReached = false;
    private bool canEnd = false;
    private bool secondaryPathEnded = false;
    private float pathExpansionFactor;
    private float pathIrregularityFactor;
    private float maxDistanceToCenterLine;
    private float startLine;
    private float maxDistanceToStartLine;
    private static readonly Vector2Int[] directions =
    {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

    private readonly int iterationLimit = 100000; // Límite de iteraciones
    private int iterationCount = 0;
    
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

    public Vector2Int Initialize( Vector2Int chunkDimensions, Vector2Int startPathTile, Vector2Int relativePosition, 
    GameObject terrainType, GameObject pathType, float pathExpansionFactor, float pathIrregularityFactor)
    {
        pathStart = startPathTile;
        chunkRelativePosition = relativePosition;
        this.pathExpansionFactor = pathExpansionFactor;
        this.pathIrregularityFactor = pathIrregularityFactor;

        SetChunkDimensionsValues(chunkDimensions);

        transform.position = new(
            relativePosition.x * chunkSize.x, 
            0, 
            relativePosition.y * chunkSize.y
        );

        terrainPrefab = terrainType;
        pathPrefab = pathType;
        
        GeneratePath();

        meshesCombiner = GetComponent<MeshesCombiner>();
        
        return pathEnd.Value;
    }

    private void SetChunkDimensionsValues(Vector2Int dimensions)
    {
        chunkSize = dimensions;
        Vector2Int halfChunk = dimensions / 2;

        // Set the chunk limits for X and Y based on the dimensions
        chunkLimitsX = new Range(
            dimensions.x % 2 == 0 ? -halfChunk.x + 1 : -halfChunk.x,
            halfChunk.x
        );

        chunkLimitsY = new Range(
            -halfChunk.y,
            dimensions.y % 2 == 0 ? halfChunk.y - 1 : halfChunk.y
        );

        // Determine the starting edge of the chunk
        startEdge = GetTileEdge(pathStart);
        
        if (startEdge != null)
        {
            if (startEdge.Value.x == 0)
            {
                float yValue = Mathf.Abs(startEdge.Value.y);
                startLine = yValue * pathStart.x;
                maxDistanceToCenterLine = yValue * chunkLimitsX.Max;
            }
            else
            {
                float xValue = Mathf.Abs(startEdge.Value.x);
                startLine = xValue * pathStart.y;
                maxDistanceToCenterLine = xValue * chunkLimitsY.Max;
            }
        }
        else
        {
            startLine = 0;
            maxDistanceToCenterLine = Mathf.Max(chunkLimitsX.Max, chunkLimitsY.Max);
        }

        maxDistanceToStartLine = maxDistanceToCenterLine + Mathf.Abs(startLine) -1;
    }

    private void GeneratePath()
    {
        PathPreviewData currentDataPath = new(pathStart);
        while (pathEnd == null)
        {
            if (iterationCount++ > iterationLimit)
                { throw new System.Exception("Infinite loop detected in path selection."); }

            pathPreview.Add(currentDataPath);
            pathPreviewHashset.Add(currentDataPath.position);
              
            if (lastPathReached)
            {
                pathEnd = currentDataPath.position;
                endEdge = GetTileEdge(pathEnd.Value).Value;
                return;
            }

            PathPreviewData? nextDataPath = GetNextDataPath(currentDataPath);

            // If no valid path is found, backtrack to the previous path data
            while (nextDataPath == null)
            {
                if (iterationCount++ > iterationLimit)
                { throw new System.Exception("Infinite loop detected in previous path calculation."); }

                if (pathPreview.Count == 1){throw new System.Exception($"Path generation failed: No remaining paths to explore in chunk {chunkRelativePosition}.");}
                
                BacktrackPath(currentDataPath);
                currentDataPath = pathPreview[^1];
                nextDataPath = GetNextDataPath(currentDataPath);
            }

            // Move to the next path data
            currentDataPath = nextDataPath.Value;
        }
    }

    private void BacktrackPath(PathPreviewData currentDataPath)
    {
        // Remove the current path data from the preview and hashset, and add it to forbidden tiles
        pathPreview.Remove(currentDataPath);
        pathPreviewHashset.Remove(currentDataPath.position);
        forbiddenTiles.Add(currentDataPath.position);
    }

    private PathPreviewData? GetNextDataPath(PathPreviewData currentPathData)
    {
        // Attempt to find a valid direction to continue the path
        while (currentPathData.availableDirections.Count > 0)
        {
            List<Vector2Int> weightedDirections = GetWeightedDirections(currentPathData);
            Vector2Int chosenDirection = weightedDirections[Random.Range(0, weightedDirections.Count)];

            // Create new path data based on the chosen direction
            PathPreviewData newPathData = new(currentPathData.position + chosenDirection);

            // Remove the chosen direction from available directions
            currentPathData.availableDirections.Remove(chosenDirection);

            if (IsPositionValid(newPathData.position)) 
            {
                return newPathData;
            }
        }

        // Return null if no valid path is found
        return null;
    }

    private List<Vector2Int> GetWeightedDirections(PathPreviewData currentPathData)
    {
        List<Vector2Int> availableDirections = currentPathData.availableDirections;
        int[] cumulativeWeights = new int[availableDirections.Count];
        int totalWeight = 0;

        // Calculate weights for each available direction
        for (int i = 0; i < availableDirections.Count; i++)
        {
            int weight = CalculateDirectionWeight(currentPathData.position + availableDirections[i]);
            totalWeight += weight;
            cumulativeWeights[i] = totalWeight;
        }

        if (totalWeight == 0) { throw new System.Exception("No weighted directions found."); }

        // Select a random weight from the total weight, and then select the direction associated with that weight
        int randomWeight = Random.Range(0, totalWeight);
        for (int i = 0; i < cumulativeWeights.Length; i++)
        {
            if (randomWeight < cumulativeWeights[i])
            {
                return new List<Vector2Int> { availableDirections[i] };
            }
        }

        throw new System.Exception("Failed to select a weighted direction.");
    }

    private int CalculateDirectionWeight(Vector2Int newPosition)
    {
        float newPositionLine = 0f;
        if (startEdge != null)
        {
            // Calculate the line position based on the start edge
            newPositionLine = startEdge.Value.x == 0
                ? Mathf.Abs(startEdge.Value.y) * newPosition.x
                : Mathf.Abs(startEdge.Value.x) * newPosition.y;
        }

        // Calculate weights based on expansion and alignment factors
        float expansionWeight = Mathf.Exp(-Mathf.Abs(newPositionLine)
        / maxDistanceToCenterLine * (1 - pathExpansionFactor) * 10);
        float alignmentWeight = Mathf.Exp(-Mathf.Abs(startLine - newPositionLine)
        / maxDistanceToStartLine * (1 - pathIrregularityFactor) * 10);

        // Ensure a minimum weight
        float finalWeight = Mathf.Max(expansionWeight + alignmentWeight, 0.1f);

        return Mathf.RoundToInt(finalWeight * 10);
    }

    private bool IsPositionValid(Vector2Int position)
    {
        if (TilePathIsInUse(position)){return false;}
        if (AreSurroundingTilesUsed(position)){return false;}
        
        if (CanEnd(position)){return lastPathReached = true;}
        if (IsInsideOfChunk(position)){return true;}

        return false;
    }

    private bool TilePathIsInUse(Vector2Int tilePosition, bool byForbidden = true)
    {
        // Check if the tile position is already in use or forbidden
        return pathPreviewHashset.Contains(tilePosition) || byForbidden && forbiddenTiles.Contains(tilePosition);
    }

    private bool AreSurroundingTilesUsed(Vector2Int tilePosition)
    {
        int tilesUsed = 0;

        foreach (Vector2Int direction in directions)
        {
            if (TilePathIsInUse(tilePosition + direction, false)){tilesUsed ++;}
            
        }

        return tilesUsed > 1; //The tile behind is always used.
    }

    private bool IsInsideOfChunk(Vector2Int tilePosition)
    {
        // Check if the tile position is within the chunk limits
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

    public Vector2Int? GenerateSecondaryPath()
    {
        int secondaryPathTries = 2; // The secondary path can't be the same as the last path.

        // Remove the end edge from available sides to prevent exit the same side.
        availableSides.Remove(endEdge);

        // Attempt to find a valid starting point for the secondary path
        PathPreviewData? startSecondaryPath = null;
        while (startSecondaryPath == null && secondaryPathTries < pathPreview.Count - 1)
        {
            if (iterationCount++ > iterationLimit)
            {
                throw new System.Exception("Infinite loop detected in secondary path start selection.");
            }

            startSecondaryPath = GetNextDataPath(pathPreview[^secondaryPathTries++]);
        }

        if (startSecondaryPath == null)
        {
            return null;
        }

        PathPreviewData currentSecondaryPath = startSecondaryPath.Value;
        int secondaryPathStartIndex = pathPreview.Count + 1; // +1 because the start is gonna be added to the list.

        // Generate the secondary path
        while (!secondaryPathEnded)
        {
            pathPreview.Add(currentSecondaryPath);
            pathPreviewHashset.Add(currentSecondaryPath.position);

            if (iterationCount++ > iterationLimit)
            {
                throw new System.Exception("Infinite loop detected in secondary path generation.");
            }

            if (CanEnd(currentSecondaryPath.position))
            {
                secondaryPathEnded = true;
                secondaryPathEnd = currentSecondaryPath.position;
                secondaryEndEdge = GetTileEdge(secondaryPathEnd.Value).Value;

                break;
            }

            PathPreviewData? nextSecondaryPath = GetNextDataPath(currentSecondaryPath);

            while (nextSecondaryPath == null)
            {
                if (iterationCount++ > iterationLimit)
                {
                    throw new System.Exception("Infinite loop detected in secondary path generation.");
                }

                if (pathPreview.Count == secondaryPathStartIndex){return null;}
                

                // Backtrack if no valid path is found
                BacktrackPath(currentSecondaryPath);

                currentSecondaryPath = pathPreview[^1];
                nextSecondaryPath = GetNextDataPath(currentSecondaryPath);
            }
            
            currentSecondaryPath = nextSecondaryPath.Value;
        }

        return secondaryPathEnd;
    }

    public void FillChunk()
    {
        // Initialize arrays for path and terrain tiles
        pathTiles = new Tile[pathPreview.Count];
        terrainTiles = new Tile[chunkSize.x * chunkSize.y - pathPreview.Count];

        int terrainTileIndex = 0, pathTileIndex = 0;

        // Iterate over each tile position within the chunk limits
        for (int x = chunkLimitsX.Min; x <= chunkLimitsX.Max; x++)
        {
            for (int y = chunkLimitsY.Min; y <= chunkLimitsY.Max; y++)
            {
                Vector2Int tilePosition = new(x, y);

                // Calculate the world position of the tile
                Vector3 tileWorldPosition = new
                (
                    tilePosition.x + chunkSize.x * chunkRelativePosition.x,
                    0,
                    tilePosition.y + chunkSize.y * chunkRelativePosition.y
                );

                // Determine if the tile is part of the path or terrain
                bool isPathTile = TilePathIsInUse(tilePosition, false);
                GameObject prefab = isPathTile ? pathPrefab : terrainPrefab;
                GameObject tileGO = InstanceTile(tilePosition, tileWorldPosition, prefab);

                Tile tile = new(tilePosition, tileWorldPosition, prefab, this, tileGO);

                if (isPathTile) {pathTiles[pathTileIndex++] = tile;}  
                else {terrainTiles[terrainTileIndex++] = tile;}
            }
        }

        meshesCombiner.CombineMeshesInChildren();
    }

    private GameObject InstanceTile(Vector2Int positionInChunk, Vector3 globalPosition, GameObject prefab)
    {
        GameObject tileGO = Instantiate(prefab, globalPosition, Quaternion.identity, this.transform);
        tileGO.name = $"{prefab.name} {positionInChunk}";

        return tileGO;
    }

    public Vector2Int? GetTileEdge (Vector2Int pathTile)
    {
        // Check if the tile is on the edge of the chunk and return the corresponding direction
        if (pathTile.x == chunkLimitsX.Max){return Vector2Int.right;}
        if (pathTile.x == chunkLimitsX.Min){return Vector2Int.left;}
        if (pathTile.y == chunkLimitsY.Max){return Vector2Int.up;}
        if (pathTile.y == chunkLimitsY.Min){return Vector2Int.down;}

        return null;
    }

    public Vector2Int RestartPathGenerated()
    {
        pathPreview.Clear();
        pathPreviewHashset.Clear();
        forbiddenTiles.Clear();
        pathEnd = null;
        lastPathReached = false;
        canEnd = false;

        GeneratePath();

        return pathEnd.Value;
    }
}
