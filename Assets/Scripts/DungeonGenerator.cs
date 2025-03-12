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
    }

    private IEnumerator GenerateDoors() {
        doors.Clear();

        for (int i = 0; i < rooms.Count; i++) {
            for (int j = i + 1; j < rooms.Count; j++) {
                CreateDoor(rooms[i].Bounds, rooms[j].Bounds);
            }
        }

        switch (generationType) {
            case GenerationType.TIMED:
                yield return new WaitForSeconds(timeBetween);
                break;
        
            case GenerationType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
        
    }

    private void CreateDoor(RectInt room1, RectInt room2) {
        RectInt doorArea = AlgorithmsUtils.Intersect(room1, room2);

        if (doorArea.width == 1 && doorArea.height == 1) {
            if (room1.width > room1.height) {
                doorArea = new RectInt(doorArea.x, doorArea.y - 1, 1, 1);
            } else {
                doorArea = new RectInt(doorArea.x - 1, doorArea.y, 1, 1); 
            }
        }

        Vector2Int doorPosition;

        if (doorArea.width > doorArea.height) {
            doorPosition = new Vector2Int(doorArea.xMin + doorArea.width / 2, doorArea.yMin);
            doors.Add(new RectInt(doorPosition, new Vector2Int(2, 1)));
        } else {
            doorPosition = new Vector2Int(doorArea.xMin, doorArea.yMin + doorArea.height / 2);
            doors.Add(new RectInt(doorPosition, new Vector2Int(1, 2)));
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
