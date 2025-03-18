using System.Collections;
using System.Collections.Generic;
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

            Debug.Log("New Room Added at: " + newRoom1.Position + " with size: " + newRoom1.Size);
            Debug.Log("New Room Added at: " + newRoom2.Position + " with size: " + newRoom2.Size);

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

        Dictionary<Vector2Int, Room> roomPositions = rooms.ToDictionary(r => r.Position, r => r);

        // for (int i = 0; i < rooms.Count; i++) {
        //     for (int j = i + 1; j < rooms.Count; j++) {
        //         CreateDoor(rooms[i].Bounds, rooms[j].Bounds);

                // switch (generationType) {
                //     case GenerationType.TIMED:
                //         yield return new WaitForSeconds(timeBetween);
                //         break;
                
                //     case GenerationType.KEYPRESS:
                //         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                //         break;
                // }
            // }
        // }

        foreach (Room room in rooms) {
            // Room adjacentRoomRight = rooms.FirstOrDefault(r => r.Value.x == room.Position.x + room.Size.x - 1 && r.Value.y <= room.Position.y || r.Value.x == room.Position.x + room.Size.x - 1 && r.Value.y >= room.Position.y + room.Size.y / 2).Key;
            // Room adjacentRoomTop = rooms.FirstOrDefault(r => r.Value.y == room.Position.y + room.Size.y - 1 && r.Value.x <= room.Position.x || r.Value.y == room.Position.y + room.Size.y - 1 && r.Value.x >= room.Position.x + room.Size.x / 2).Key;

            // if (adjacentRoomRight != null) CreateDoor(room.Bounds, adjacentRoomRight.Bounds);
            // if (adjacentRoomTop != null) CreateDoor(room.Bounds, adjacentRoomTop.Bounds);

            Vector2Int rightPos = new Vector2Int(room.Position.x + room.Size.x - 1, room.Position.y);
            Vector2Int topPos = new Vector2Int(room.Position.x, room.Position.y + room.Size.y - 1);

            if (roomPositions.TryGetValue(rightPos, out Room adjacentRoomRight)) CreateDoor(room.Bounds, adjacentRoomRight.Bounds);
            if (roomPositions.TryGetValue(topPos, out Room adjacentRoomTop)) CreateDoor(room.Bounds, adjacentRoomTop.Bounds);

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
