using System.Collections;
using System.Collections.Generic;
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

        // GenerateDoors(startRoom);
    }

    private IEnumerator GenerateDoors() {
        doors.Clear();

        Queue<Room> doorQueue = new Queue<Room>();
        doorQueue.Enqueue(startRoom);

        while (doorQueue.Count > 0) {
            Room room = doorQueue.Dequeue();

            if (room.childRooms.Count < 2) continue;

            Room room1 = room.childRooms[0];
            Room room2 = room.childRooms[1];

            if (AlgorithmsUtils.Intersects(room1.Bounds, room2.Bounds)) {
                RectInt door = AlgorithmsUtils.Intersect(room1.Bounds, room2.Bounds);
                
            }

            doorQueue.Enqueue(room1);
            doorQueue.Enqueue(room2);

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

    public void StartDungeonGeneration() {
        StopAllCoroutines();
        StartCoroutine(GenerateDungeon());
    }

    public void StartDoorGeneration() {
        StopAllCoroutines();
        StartCoroutine(GenerateDoors());
    }
}
