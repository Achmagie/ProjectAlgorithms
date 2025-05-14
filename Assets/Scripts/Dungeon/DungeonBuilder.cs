using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
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

    private Vector3 startNode;
    private Vector3 endNode;
    
    public List<Vector3> path = new();
    HashSet<Vector3> discovered = new();
    
    private int [,] _tileMap;
    private Graph<Vector3> _traversalGraph = new();

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

        foreach (var node in _traversalGraph.GetNodes()) {
            DebugExtension.DebugWireSphere(node, Color.cyan, .2f);

            foreach (var neighbor in _traversalGraph.GetNeighbors(node)) {
                Debug.DrawLine(node, neighbor, Color.red);
            }
        }
    }

    public void StartSpawnWalls() {
        StartCoroutine(SpawnTiles());
    }

    public void StartSpawnFloor() {
        Room startRoom = generator.Rooms[Random.Range(0, generator.Rooms.Count - 1)];
        StartCoroutine(SpawnFloor(new Vector2Int((int)startRoom.Bounds.center.x, (int)startRoom.Bounds.center.y)));
    }

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

    private IEnumerator SpawnFloor(Vector2Int startPoint) {
        int oldTile = _tileMap[startPoint.y, startPoint.x];

        if (oldTile == 1) yield break;

        int height = _tileMap.GetLength(0);
        int width = _tileMap.GetLength(1);

        Queue<Vector2Int> tileQ = new();
        tileQ.Enqueue(startPoint);
        _tileMap[startPoint.y, startPoint.x] = 1;

        Vector3 spawnPos = new(startPoint.x, 0, startPoint.y);
        Vector3 startNodePos = spawnPos + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);
        Instantiate(floorPrefab, spawnPos + new Vector3(SPAWN_OFFSET, 0, SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);
        _traversalGraph.AddNode(startNodePos);

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        while (tileQ.Count > 0)
        {
            Vector2Int tile = tileQ.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = tile.x + dx[i];
                int ny = tile.y + dy[i];

                if (nx >= 0 && nx < height && ny >= 0 && ny < width && _tileMap[nx, ny] == 0)
                {
                    _tileMap[nx, ny] = 1;

                    spawnPos = new Vector3(ny, 0, nx);
                    Vector3 nodePos = spawnPos + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);

                    Instantiate(floorPrefab, spawnPos + new Vector3(SPAWN_OFFSET, 0, SPAWN_OFFSET), floorPrefab.transform.rotation, floorParent.transform);
                    _traversalGraph.AddNode(nodePos);

                    List<Vector3> currentNodes = _traversalGraph.GetNodes();

                    foreach (var dir in new Vector2Int[] {
                        new(-1, 0),
                        new(1, 0),
                        new(0, -1),
                        new(0, 1),

                        new(-1, 1),
                        new(1, 1),
                        new(-1, -1),
                        new(1, -1)
                    }) {
                        int neighborX = nx + dir.x;
                        int neighborY = ny + dir.y;

                        Vector3 neighborPos = new Vector3(neighborY, 0, neighborX) + new Vector3(SPAWN_OFFSET / 2f, 0, SPAWN_OFFSET / 2f);

                        if (currentNodes.Contains(neighborPos)) _traversalGraph.AddEdge(nodePos, neighborPos);
                    }

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

    private Vector3 GetClosestNodeToPosition(Vector3 position) {
        Vector3 closestNode = Vector3.zero;
        float closestDistance = Mathf.Infinity;
        
        foreach(Vector3 node in _traversalGraph.GetNodes()) {
            if ((node - position).magnitude < closestDistance) {
                closestDistance = (node - position).magnitude;
                closestNode = node;
            }            
        }

        return closestNode;
    }

    public List<Vector3> CalculatePath(Vector3 from, Vector3 to) {
        Vector3 playerPosition = from;
        
        startNode = GetClosestNodeToPosition(playerPosition);
        endNode = GetClosestNodeToPosition(to);

        List<Vector3> shortestPath = AStar(startNode, endNode);
        path = shortestPath; //Used for drawing the path
        
        return shortestPath;
    }

    List<Vector3> AStar(Vector3 start, Vector3 end) {
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        discovered.Clear();
        
        List<(Vector3 node, float priority)> nodeQ = new();
        Dictionary<Vector3, float> cost = new();
        Dictionary<Vector3, Vector3> path = new();

        nodeQ.Add((start, 0));
        cost[start] = 0;

        while (nodeQ.Count > 0) {
            nodeQ = nodeQ.OrderByDescending(n => n.priority).ToList();
            Vector3 node = nodeQ[^1].node;
            nodeQ.RemoveAt(nodeQ.Count - 1);

            if (node == end) return ReconstructPath(path, start, end);

            foreach(Vector3 neighbor in _traversalGraph.GetNeighbors(node)) {
                float newCost = cost[node] + Cost(node, neighbor);
                if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor]) {
                    cost[neighbor] = newCost;
                    path[neighbor] = node;

                    nodeQ.Add((neighbor, newCost + Heuristic(neighbor, end)));
                }
            }
        }
        /* */
        return new List<Vector3>(); // No path found
    }

    public float Cost(Vector3 from, Vector3 to) {
        return Vector3.Distance(from, to);
    }
    
    public float Heuristic(Vector3 from, Vector3 to) {
        return Vector3.Distance(from, to);
    }
    
    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end) {
        List<Vector3> path = new();
        Vector3 currentNode = end;

        while (currentNode != start) {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Add(start);
        path.Reverse();

        return path;
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
