using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    [Header("Asset Params")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private List<GameObject> tilePrefabs;

    [SerializeField] private PathFinder pathFinder;

    private const float SPAWN_OFFSET = 1f;

    /// <summary>
    /// Places all the necessary tiles per room based on a given tilemap
    /// </summary>
    /// <param name="roomsToBuild">List of rooms to place tiles for</param>
    /// <param name="tileMap">Used to determine which tiles to place and where</param>
    /// <returns></returns>
    public IEnumerator SpawnTiles(List<Room> roomsToBuild, int[,] tileMap) {
        GameObject roomParent = new("Rooms");

        foreach (Room room in roomsToBuild) {
            GameObject tileParent = new("Room_" + room.Position + "_" + room.Size);
            tileParent.transform.parent = roomParent.transform;
            
            for (int y = room.Position.y; y < room.Position.y + room.Size.y - 1; y++) {
                for (int x = room.Position.x; x < room.Position.x + room.Size.x - 1; x++) {
                    int tileCase = CalculateTileCase(x, y, tileMap);

                    GameObject tile = tilePrefabs[tileCase];

                    tile = Instantiate(tile, new Vector3(x + SPAWN_OFFSET, 0, y + SPAWN_OFFSET), tile.transform.rotation, tileParent.transform); 
                    tile.name = "Tile_" + tile.transform.position;

                    if (DungeonProcessor.Instance.GenerationType != DungeonProcessor.ProcessingType.INSTANT) yield return DungeonProcessor.Instance.WaitForGeneration();
                }
            }
        }   
    }

    //BFS VERSION (works better but i needed to have a recursive one)

    // public IEnumerator SpawnFloor(Vector2Int startPoint, int[,] tileMap) {
    //     Graph<Vector3> traversalGraph = new();

    //     int oldTile = tileMap[startPoint.y, startPoint.x];

    //     if (oldTile == 1) yield break;

    //     int height = tileMap.GetLength(0);
    //     int width = tileMap.GetLength(1);

    //     Queue<Vector2Int> tileQ = new();
    //     tileQ.Enqueue(startPoint);
    //     tileMap[startPoint.y, startPoint.x] = 1;

    //     GameObject floorParent = new GameObject("Floors");

    //     Vector3 spawnPos = new(startPoint.x, 0, startPoint.y);
    //     Vector3 startNodePos = spawnPos + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);
    //     Instantiate(floorPrefab, spawnPos + new Vector3(SPAWN_OFFSET, 0, SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);
    //     traversalGraph.AddNode(startNodePos);

    //     int[] dx = { -1, 1, 0, 0 };
    //     int[] dy = { 0, 0, -1, 1 };

    //     while (tileQ.Count > 0)
    //     {
    //         Vector2Int tile = tileQ.Dequeue();

    //         for (int i = 0; i < 4; i++)
    //         {
    //             int nx = tile.x + dx[i];
    //             int ny = tile.y + dy[i];

    //             if (nx >= 0 && nx < height && ny >= 0 && ny < width && tileMap[nx, ny] == 0)
    //             {
    //                 tileMap[nx, ny] = 1;

    //                 spawnPos = new Vector3(ny, 0, nx);
    //                 Vector3 nodePos = spawnPos + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);

    //                 Instantiate(floorPrefab, spawnPos + new Vector3(SPAWN_OFFSET, 0, SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);
    //                 traversalGraph.AddNode(nodePos);

    //                 List<Vector3> currentNodes = traversalGraph.GetNodes();

    //                 foreach (var dir in new Vector2Int[] {
    //                     new(-1, 0),
    //                     new(1, 0),
    //                     new(0, -1),
    //                     new(0, 1),

    //                     new(-1, 1),
    //                     new(1, 1),
    //                     new(-1, -1),
    //                     new(1, -1)
    //                 }) {
    //                     int neighborX = nx + dir.x;
    //                     int neighborY = ny + dir.y;

    //                     Vector3 neighborPos = new Vector3(neighborY, 0, neighborX) + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);

    //                     if (currentNodes.Contains(neighborPos)) traversalGraph.AddEdge(nodePos, neighborPos);
    //                 }

    //                 if (DungeonProcessor.Instance.GenerationType != DungeonProcessor.ProcessingType.INSTANT) yield return DungeonProcessor.Instance.WaitForGeneration();

    //                 tileQ.Enqueue(new Vector2Int(nx, ny));
    //             }
    //         }
    //     }

    //     pathFinder.SetGraph(traversalGraph);
    // }


    // DFS VERSION (worse but its recursive)
    /// <summary>
    /// Starts the floor spawning process
    /// </summary>
    /// <param name="startPoint">Start of the floodfill algorithm</param>
    /// <param name="tileMap">Tilemap necessary for checking boundaries</param>
    public void SpawnFloor(Vector2Int startPoint, int[,] tileMap) {
        int height = tileMap.GetLength(0);
        int width = tileMap.GetLength(1);

        int oldTile = tileMap[startPoint.y, startPoint.x];
        if (oldTile == 1) return;

        GameObject floorParent = new("Floors");
        Graph<Vector3> traversalGraph = new();

        StartCoroutine(RecursiveFloodFill(startPoint, tileMap, floorParent, traversalGraph, height, width));

        pathFinder.SetGraph(traversalGraph);
    }

    /// <summary>
    /// Implements dfs based flood fill algorithm to recursively spawn floors,
    /// Connects all floor tiles together in an 8 way pattern
    /// </summary>
    /// <param name="tile">Position of current tile</param>
    /// <param name="tileMap">Tilemap necessary for checking boundaries</param>
    /// <param name="floorParent">Parent object of all generated floor tiles</param>
    /// <param name="traversalGraph">Used to connect all floor tiles together in an 8 way pattern for traversal later on</param>
    /// <param name="height">Dungeon height</param>
    /// <param name="width">Dungeon width</param>
    /// <returns></returns>
    private IEnumerator RecursiveFloodFill(Vector2Int tile, int[,] tileMap, GameObject floorParent, Graph<Vector3> traversalGraph, int height, int width)
    {
        tileMap[tile.y, tile.x] = 1;

        Vector3 spawnPos = new(tile.x, 0, tile.y);
        Vector3 nodePos = spawnPos + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);

        Instantiate(floorPrefab, spawnPos + new Vector3(SPAWN_OFFSET, 0, SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);
        traversalGraph.AddNode(nodePos);

        List<Vector3> currentNodes = traversalGraph.GetNodes();
        foreach (var dir in new Vector2Int[] {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1),
            new(-1, 1), new(1, 1), new(-1, -1), new(1, -1)
        })
        {
            int nx = tile.x + dir.x;
            int ny = tile.y + dir.y;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                Vector3 neighborPos = new Vector3(nx, 0, ny) + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);
                if (currentNodes.Contains(neighborPos)) traversalGraph.AddEdge(nodePos, neighborPos);
            }
        }

        if (DungeonProcessor.Instance.GenerationType != DungeonProcessor.ProcessingType.INSTANT) yield return DungeonProcessor.Instance.WaitForGeneration();

        Vector2Int[] directions = new Vector2Int[] {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1)
        };

        foreach (var dir in directions)
        {
            int nx = tile.x + dir.x;
            int ny = tile.y + dir.y;

            if (nx >= 0 && nx < width && ny >= 0 && ny < height && tileMap[ny, nx] == 0)
            {
                yield return RecursiveFloodFill(new Vector2Int(nx, ny), tileMap, floorParent, traversalGraph, height, width);
            }
        }
    }

    /// <summary>
    /// Calculates which prefab to use for which tile in the map
    /// </summary>
    /// <param name="x">current place in column</param>
    /// <param name="y">current place in row</param>
    /// <param name="tileMap"></param>
    /// <returns></returns>
    private int CalculateTileCase(int x, int y, int[,] tileMap) {
        var bottomLeft = tileMap[y, x];
        var bottomRight = tileMap[y, x + 1];
        var topLeft = tileMap[y + 1, x];
        var topRight = tileMap[y + 1, x + 1];

        return bottomRight + 2 * topRight + 4 * topLeft + 8 * bottomLeft;
    }
}
