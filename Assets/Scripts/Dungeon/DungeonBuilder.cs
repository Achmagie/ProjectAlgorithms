using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject wallPrefab;

    private const float SPAWN_OFFSET = 0.5f;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
    }

    private readonly DungeonGenerator generator = new();
    private readonly DungeonGraph graph = new();
    private readonly DungeonPainter painter = new();

    private void Update() {
        painter.PaintRooms(generator.Rooms, Color.red);
        painter.PaintDoors(generator.Doors, Color.blue);

        painter.PaintGraph(graph, Color.green, Color.white);
        painter.PaintPath(graph, Color.blue, Color.blue);
    }

    public void StartSpawnWalls() {
        StartCoroutine(SpawnWalls());
    }

    private IEnumerator SpawnWalls() {
        HashSet<Vector3> wallPositions = new();

        foreach (Room room in generator.Rooms) {
            for (int i = 0; i < room.Size.x; i++) {
                wallPositions.Add(new Vector3(room.Position.x + SPAWN_OFFSET + i, 0, room.Position.y + SPAWN_OFFSET)); 
                wallPositions.Add(new Vector3(room.Position.x + SPAWN_OFFSET + i, 0, room.Position.y - SPAWN_OFFSET + room.Size.y)); 
            }

            for (int j = 0; j < room.Size.y; j++) {
                wallPositions.Add(new Vector3(room.Position.x + SPAWN_OFFSET, 0, room.Position.y + SPAWN_OFFSET + j)); 
                wallPositions.Add(new Vector3(room.Position.x - SPAWN_OFFSET + room.Size.x, 0, room.Position.y + SPAWN_OFFSET + j));
            }
        }

        foreach (Vector3 wallPos in wallPositions) {
            Instantiate(wallPrefab, wallPos, Quaternion.identity);

            if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
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
