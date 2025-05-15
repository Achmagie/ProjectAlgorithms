using Unity.AI.Navigation;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [Header("Dungeon Params")]
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minimumRoomSize;
    [SerializeField] private int seed;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private int[,] tileMap;
    private DungeonGraph graph;
    private DungeonGenerator generator;
    private DungeonPainter painter;
    private TileMapGenerator tileMapGenerator;
    private DungeonBuilder builder;

    private void Awake() {
        graph = new DungeonGraph();
        generator = new DungeonGenerator(graph);
        painter = new DungeonPainter(graph);
        tileMapGenerator = new TileMapGenerator();

        builder = GetComponent<DungeonBuilder>();
    }

    private void Update() {
        painter.PaintRooms(generator.Rooms, Color.red);
        painter.PaintDoors(generator.Doors, Color.blue);

        painter.PaintGraph(Color.green, Color.white);
        painter.PaintPath(Color.blue, Color.blue);
    }
    
    public void StartDungeonGeneration() {
        StartCoroutine(generator.GenerateDungeon(dungeonSize, minimumRoomSize, seed));
    }

    public void StartDoorGeneration() {
        StartCoroutine(generator.GenerateDoors(seed));
    }

    public void StartRoomPurge() {
        StartCoroutine(generator.PurgeRooms());
    }

    public void StartDoorPurge() {
        StartCoroutine(generator.PurgeDoors());
    }

    public void StartGraphGeneration() {
        StartCoroutine(graph.GenerateGraph(generator.Rooms));
    }

    public void StartGraphSearch() {
        StartCoroutine(graph.SearchGraph());
    }

    public void GenerateTileMap() {
        tileMap = tileMapGenerator.GenerateTileMap(dungeonSize, generator.Rooms, generator.Doors);
    }

    public void PrintTileMap() {
        tileMapGenerator.PrintTileMap();
    }

    public void SpawnTiles() {
        StartCoroutine(builder.SpawnTiles(generator.Rooms, tileMap));
    }

    public void SpawnFloor() {
        Room startRoom = generator.Rooms[Random.Range(0, generator.Rooms.Count - 1)];
        StartCoroutine(builder.SpawnFloor(new Vector2Int((int)startRoom.Bounds.center.x, (int)startRoom.Bounds.center.y), tileMap));
    }

    public void BakeNavMesh() {
        navMeshSurface.BuildNavMesh();
    }
}
