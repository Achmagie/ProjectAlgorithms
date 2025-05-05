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
    [SerializeField] private GameObject roomParent;
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
        HashSet<Vector3> doorPositions = generator.Doors.Select(d => new Vector3(d.position.x, 0, d.position.y)).ToHashSet();

        Vector3 wallBottomOffset = new(SPAWN_OFFSET, 0, SPAWN_OFFSET);
        Vector3 wallTopOffset = new(SPAWN_OFFSET, 0, -SPAWN_OFFSET);
        Vector3 wallLeftOffset = new(SPAWN_OFFSET, 0, SPAWN_OFFSET);
        Vector3 wallRightOffset = new(-SPAWN_OFFSET, 0, SPAWN_OFFSET);

        foreach (Room room in generator.Rooms) {        
            HashSet<Vector3> wallPositions = new();

            for (int i = 0; i < room.Size.x; i++) {
                Vector3 wallBottom = new(room.Position.x + i, 0, room.Position.y);
                Vector3 wallTop = new(room.Position.x + i, 0, room.Position.y + room.Size.y);

                wallPositions.Add(wallBottom + wallBottomOffset);
                wallPositions.Add(wallTop + wallTopOffset);
            }

            for (int j = 0; j < room.Size.y; j++) {
                Vector3 wallLeft = new(room.Position.x, 0, room.Position.y + j);
                Vector3 wallRight = new(room.Position.x + room.Size.x, 0, room.Position.y + j);

                wallPositions.Add(wallLeft + wallLeftOffset);
                wallPositions.Add(wallRight + wallRightOffset);
            }

            GameObject wallParent = new("Room_" + room.Position + "_" + room.Size);
            wallParent.transform.parent = roomParent.transform;

            foreach (Vector3 wallPos in wallPositions) {
                if (doorPositions.Contains(wallPos - wallBottomOffset) || doorPositions.Contains(wallPos - wallLeftOffset)) continue; 

                GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, wallParent.transform);
                wall.name = "Wall_" + wall.transform.position;  

                if (generationType != GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
            }
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
