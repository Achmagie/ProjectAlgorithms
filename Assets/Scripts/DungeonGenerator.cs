using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    [SerializeField] private float timeBetween;
    [SerializeField] private GenerationType generationType;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
    }
    
    private List<Room> rooms = new List<Room>();
    // private Dictionary<Room, Vector2Int> rooms = new();
    private List<RectInt> doors = new List<RectInt>();

    private Room startRoom;

    void Update() {
        if (rooms.Count == 0) return;

        foreach (Room room in rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, Color.red);
        }

        foreach (RectInt door in doors) {
            AlgorithmsUtils.DebugRectInt(door, Color.blue);
        }
    }

    private IEnumerator GenerateDungeon() {
        startRoom = new Room(new Vector2Int(0, 0), dungeonSize);

        rooms.Clear();
        rooms.Add(startRoom);
        
        Queue<(Room, bool)> roomQueue = new Queue<(Room, bool)>();
        roomQueue.Enqueue((startRoom, Random.value > .5f));

        while (roomQueue.Count > 0) {
            (Room room, bool splitHorizontally) = roomQueue.Dequeue();
            
            (Room newRoom1, Room newRoom2) = room.Split(splitHorizontally, minRoomSize);

            if (newRoom1 == null && newRoom2 == null) continue;

            rooms.Remove(room);
            rooms.Add(newRoom1);
            rooms.Add(newRoom2);

            roomQueue.Enqueue((newRoom1, splitHorizontally));
            roomQueue.Enqueue((newRoom2, !splitHorizontally));

            switch (generationType) {
                case GenerationType.TIMED:
                    yield return new WaitForSeconds(timeBetween);
                    break;
            
                case GenerationType.KEYPRESS:
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                    break;
            }
        }
    }

    private IEnumerator GenerateDoors() {
        doors.Clear();

        // Dictionary<Vector2Int, Room> roomPositions = rooms.ToDictionary(r => r.Position, r => r);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();


        for (int i = 0; i < rooms.Count; i++) {
            for (int j = i + 1; j < rooms.Count; j++) {
                CreateDoor(rooms[i].Bounds, rooms[j].Bounds);

                switch (generationType) {
                    case GenerationType.TIMED:
                        yield return new WaitForSeconds(timeBetween);
                        break;
                
                    case GenerationType.KEYPRESS:
                        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                        break;
                }
            }
        }

        // I will revisit this i will make it better
        
        // foreach (Room room in rooms) {
        //     foreach (var potentialAdjacent in roomPositions) {
        //         if (potentialAdjacent.Key.x == room.Position.x + room.Size.x - 1) {
        //             int yDiff = Mathf.Abs(potentialAdjacent.Key.y - room.Position.y);

        //             if (yDiff <= room.Size.y * 1.5) {
        //                 CreateDoor(room.Bounds, potentialAdjacent.Value.Bounds);
        //             }
        //         }
        //     }

        //     foreach (var potentialAdjacent in roomPositions) {
        //         if (potentialAdjacent.Key.y == room.Position.y + room.Size.y - 1) {
        //             int xDiff = Mathf.Abs(potentialAdjacent.Key.x - room.Position.x);
        
        //             if (xDiff <= room.Size.x * 1.5) {
        //                 CreateDoor(room.Bounds, potentialAdjacent.Value.Bounds);
        //             }
        //         }
        //     }

        //     switch (generationType) {
        //         case GenerationType.TIMED:
        //             yield return new WaitForSeconds(timeBetween);
        //             break;
            
        //         case GenerationType.KEYPRESS:
        //             yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        //             break;
        //     }
        // }

        stopwatch.Stop();

        UnityEngine.Debug.Log("Elapsed time: " + stopwatch.Elapsed);
    }

    private void CreateDoor(RectInt room1, RectInt room2) {
        RectInt intersection = AlgorithmsUtils.Intersect(room1, room2);

        if (intersection.width <= 4 && intersection.height <= 4) return;

        Vector2Int doorPosition;

        if (intersection.width > 1) doorPosition = new Vector2Int(Random.Range(intersection.xMin + 2, intersection.xMin + intersection.width - 2), intersection.yMin);
        else if (intersection.height > 1) doorPosition = new Vector2Int(intersection.xMin, Random.Range(intersection.yMin + 2, intersection.yMin + intersection.height - 2));
        else return;

        doors.Add(new RectInt(doorPosition, new Vector2Int(1, 1)));
    }

    public void StartDungeonGeneration() {
        StopAllCoroutines();
        StartCoroutine(GenerateDungeon());
    }

    public void StartDoorGeneration() {
        StopAllCoroutines();
        StartCoroutine(GenerateDoors());
    }
}
