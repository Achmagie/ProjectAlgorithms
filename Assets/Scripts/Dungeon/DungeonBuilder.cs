using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    [SerializeField] private float timeBetweenOperations;
    [SerializeField] private GenerationType generationType;
    [SerializeField] private PurgeType purgeType; 
    [SerializeField] private int seed;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
    }

    public enum PurgeType {
        ROOMS,
        DOORS
    }

    private DungeonGenerator generator;
    private DungeonGraph graph;
    private DungeonPainter painter;

    private void Awake() {
        generator = new DungeonGenerator();
        graph = new DungeonGraph();
        painter = new DungeonPainter(generator, graph);
    }

    private void Update() {
        painter.PaintRooms(Color.red);
        painter.PaintDoors(Color.blue);

        painter.PaintGraph(Color.green, Color.white);
        painter.PaintPath(Color.blue, Color.blue);
    }

    public void StartDungeonGeneration() {
        SetGenType();
        StopAllCoroutines();
        StartCoroutine(generator.GenerateDungeon(dungeonSize, minRoomSize, graph, seed));
    }

    public void StartDungeonPurge() {
        StopAllCoroutines();
        StartCoroutine(generator.PurgeRooms(graph, purgeType));
    }

    public void StartDoorGeneration() {
        SetGenType();
        StopAllCoroutines();
        StartCoroutine(generator.GenerateDoors(graph));
    }

    public void StartGraphGeneration() {
        SetGenType();
        StopAllCoroutines();
        StartCoroutine(graph.GenerateGraph(generator.Rooms));
    }

    public void StartGraphSearch() {
        SetGenType();
        StopAllCoroutines();
        StartCoroutine(graph.SearchGraph());
    }

    private void SetGenType() {
        generator.SetGenType(generationType, timeBetweenOperations);
        graph.SetGenType(generationType, timeBetweenOperations);
    }
}
