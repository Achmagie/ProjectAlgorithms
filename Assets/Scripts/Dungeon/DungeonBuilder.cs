using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    [Header("Dungeon Params")]
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    [SerializeField] private int seed;

    [Header("Generation Params")]
    [SerializeField] private float timeBetweenOperations;
    [SerializeField] private GenerationType generationType;

    [Header("Spawn Assets")]
    [SerializeField] private GameObject roomParent;
    [SerializeField] private GameObject floorParent;
    [SerializeField] private List<GameObject> tilePrefabs;
    [SerializeField] private GameObject floorPrefab;

    [SerializeField] private NavMeshSurface navMeshSurface;

    private int [,] _tileMap;

    private const float SPAWN_OFFSET = 1f;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
    }

    private readonly DungeonGenerator generator = new();
    private readonly DungeonGraph graph = new();
    private readonly DungeonPainter painter = new();
    private readonly TileMapGenerator tileMapGenerator = new();

    private void Update() {
        painter.PaintRooms(generator.Rooms, Color.red);
        painter.PaintDoors(generator.Doors, Color.blue);

        painter.PaintGraph(graph, Color.green, Color.white);
        painter.PaintPath(graph, Color.blue, Color.blue);
    }

    public void StartSpawnWalls() {
        StartCoroutine(SpawnTiles());
    }

    public void StartSpawnFloor() {
        Room startRoom = generator.Rooms[UnityEngine.Random.Range(0, generator.Rooms.Count - 1)];
        StartCoroutine(SpawnFloor(new Vector2Int((int)startRoom.Bounds.center.x, (int)startRoom.Bounds.center.y)));
    }

    // private IEnumerator SpawnWalls() {
    //     HashSet<Vector3> doorPositions = generator.Doors.Select(d => new Vector3(d.position.x, 0, d.position.y)).ToHashSet();

    //     Vector3 wallBottomOffset = new(SPAWN_OFFSET, 0, SPAWN_OFFSET);
    //     Vector3 wallTopOffset = new(SPAWN_OFFSET, 0, -SPAWN_OFFSET);
    //     Vector3 wallLeftOffset = new(SPAWN_OFFSET, 0, SPAWN_OFFSET);
    //     Vector3 wallRightOffset = new(-SPAWN_OFFSET, 0, SPAWN_OFFSET);

    //     foreach (Room room in generator.Rooms) {        
    //         HashSet<Vector3> wallPositions = new();

    //         for (int i = 0; i < room.Size.x; i++) {
    //             Vector3 wallBottom = new(room.Position.x + i, 0, room.Position.y);
    //             Vector3 wallTop = new(room.Position.x + i, 0, room.Position.y + room.Size.y);

    //             wallPositions.Add(wallBottom + wallBottomOffset);
    //             wallPositions.Add(wallTop + wallTopOffset);
    //         }

    //         for (int j = 0; j < room.Size.y; j++) {
    //             Vector3 wallLeft = new(room.Position.x, 0, room.Position.y + j);
    //             Vector3 wallRight = new(room.Position.x + room.Size.x, 0, room.Position.y + j);

    //             wallPositions.Add(wallLeft + wallLeftOffset);
    //             wallPositions.Add(wallRight + wallRightOffset);
    //         }

    //         GameObject wallParent = new("Room_" + room.Position + "_" + room.Size);
    //         wallParent.transform.parent = roomParent.transform;

    //         GameObject floor = Instantiate(floorPrefab, new Vector3(room.Bounds.center.x, 0, room.Bounds.center.y), floorPrefab.transform.rotation, wallParent.transform);
    //         floor.transform.localScale = new Vector3(room.Size.x, room.Size.y, 1);

    //         foreach (Vector3 wallPos in wallPositions) {
    //             if (doorPositions.Contains(wallPos - wallBottomOffset) || doorPositions.Contains(wallPos - wallLeftOffset)) continue; 

    //             // GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, wallParent.transform);
    //             // wall.name = "Wall_" + wall.transform.position;  

    //             if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
    //         }
    //     }
    // }

    private IEnumerator SpawnTiles() {
        foreach (Room room in generator.Rooms) {
            GameObject tileParent = new("Room_" + room.Position + "_" + room.Size);
            tileParent.transform.parent = roomParent.transform;

            for (int y = room.Position.y; y < room.Position.y + room.Size.y - 1; y++) {
                for (int x = room.Position.x; x < room.Position.x + room.Size.x - 1; x++) {
                    int tileCase = CalculateTileCase(x, y);

                    GameObject tile = tilePrefabs[tileCase];

                    tile = Instantiate(tile, new Vector3(x + SPAWN_OFFSET, 0, y + SPAWN_OFFSET), tile.transform.rotation, tileParent.transform); 
                    tile.name = "Tile_" + tile.transform.position;

                    if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
                }
            }
        }   
    }

    // private IEnumerator SpawnFloor(int startY, int startX) {        
    //     int height = _tileMap.GetLength(0);
    //     int width = _tileMap.GetLength(1);
    //     bool[,] visited = new bool[height, width];
    //     Queue<Vector2Int> tileQ = new();

    //     tileQ.Enqueue(new Vector2Int(startX, startY));
    //     visited[startY, startX] = true;

    //     while (tileQ.Count > 0) {
    //         Vector2Int currentTile = tileQ.Dequeue();
    //         int x = currentTile.x;
    //         int y = currentTile.y;

    //         if (y < 0 || y >= height - 1 || x < 0 || x >= width - 1) continue;

            // GameObject tileType = tilePrefabs[CalculateTileCase(x, y)];

            // if (tileType.CompareTag("Filler")) Instantiate(floorPrefab, new Vector3(x + SPAWN_OFFSET, 0, y + SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);

    //         if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);

    //         Vector2Int[] dir = {
    //             new(x, y + 1),
    //             new(x, y - 1),
    //             new(x + 1, y),
    //             new(x - 1, y)
    //         };

    //         foreach (var next in dir) {
    //             int nx = next.x;
    //             int ny = next.y;

    //             if (ny >= 0 && ny < height - 1 && nx >= 0 && nx < width - 1 && !visited[ny, nx]) {
    //                 visited[ny, nx] = true;
    //                 tileQ.Enqueue(next);
    //             }
    //         }
    //     }
    // }
    
    private IEnumerator SpawnFloor(Vector2Int startPoint) {        
        int oldTile = _tileMap[startPoint.y, startPoint.x];

        if (oldTile == 1) yield break;

        int height = _tileMap.GetLength(0);
        int width = _tileMap.GetLength(1);

        Queue<Vector2Int> tileQ = new();

        tileQ.Enqueue(startPoint);

        _tileMap[startPoint.y, startPoint.x] = 1;

        Instantiate(floorPrefab, new Vector3(startPoint.x + SPAWN_OFFSET, 0, startPoint.y + SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1};

        while (tileQ.Count > 0) {
            Vector2Int tile = tileQ.Dequeue();

            for (int i = 0; i < 4; i++) {
                int nx = tile.x + dx[i];
                int ny = tile.y + dy[i];

                if (nx >= 0 && nx < height && ny >= 0 && ny < width && _tileMap[nx, ny] == 0) {
                    _tileMap[nx, ny] = 1;

                    Instantiate(floorPrefab, new Vector3(ny + SPAWN_OFFSET, 0, nx + SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);

                    if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);

                    tileQ.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }

    private int CalculateTileCase(int x, int y) {
        var bottomLeft = _tileMap[y, x];
        var bottomRight = _tileMap[y, x + 1];
        var topLeft = _tileMap[y + 1, x];
        var topRight = _tileMap[y + 1, x + 1];

        return bottomRight + 2 * topRight + 4 * topLeft + 8 * bottomLeft;
    }

    public void StartDungeonGeneration() {
        StartProcess(generator.GenerateDungeon(dungeonSize, minRoomSize, seed));
    }

    public void StartRoomPurge() {
        StartProcess(generator.PurgeRooms());
    }

    public void StartDoorPurge() {
        StartProcess(generator.PurgeDoors(graph));
    }

    public void StartDoorGeneration() {
        StartProcess(generator.GenerateDoors(seed));
    }

    public void StartGraphGeneration() {
        StartProcess(graph.GenerateGraph(generator.Rooms));
    }

    public void StartGraphSearch() {
        StartProcess(graph.SearchGraph());
    }

    public void GenerateTileMap() {
        _tileMap = tileMapGenerator.GenerateTileMap(dungeonSize, generator.Rooms, generator.Doors);
    }

    public void PrintTileMap() {
        tileMapGenerator.PrintTileMap();
    }

    public void BakeNavMesh() {
        generator.BakeNavMesh(navMeshSurface);
    }

    private void SetGenType() {
        generator.SetGenType(generationType, timeBetweenOperations);
        graph.SetGenType(generationType, timeBetweenOperations);
    }

    private void StartProcess(IEnumerator routine) {
        SetGenType();
        StopAllCoroutines();
        StartCoroutine(routine);
    }
}
