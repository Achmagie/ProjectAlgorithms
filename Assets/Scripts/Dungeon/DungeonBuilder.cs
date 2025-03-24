using UnityEngine;

public class DungeonBuilder : MonoBehaviour
{
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    [SerializeField] private float timeBetweenOperations;
    [SerializeField] private GenerationType generationType;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
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
        StartCoroutine(generator.GenerateDungeon(dungeonSize, minRoomSize, graph));
    }

    public void StartDoorGeneration() {
        SetGenType();
        StartCoroutine(generator.GenerateDoors(graph));
    }

    public void StartGraphGeneration() {
        SetGenType();
        StartCoroutine(graph.GenerateGraph());
    }

    public void StartGraphSearch() {
        SetGenType();
        StartCoroutine(graph.SearchGraph());
    }

    private void SetGenType() {
        generator.SetGenType(generationType, timeBetweenOperations);
        graph.SetGenType(generationType, timeBetweenOperations);
    }
}
