using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    
    private List<Room> rooms = new List<Room>();
    private Room startRoom;

    void Start() {
        startRoom = new Room(new Vector2Int(0, 0), dungeonSize);

        GenerateDungeon();
    }

    void Update() {
        if (rooms.Count == 0) return;

        foreach (Room room in rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, Color.red);
        }
    }

    private void GenerateDungeon() {
        Queue<(Room, bool)> roomQueue = new Queue<(Room, bool)>();
        roomQueue.Enqueue((startRoom, true));

        while (roomQueue.Count > 0) {
            (Room room, bool splitHorizontally) = roomQueue.Dequeue();

            if (room.Size.x / 2 <= minRoomSize.x && room.Size.y / 2 <= minRoomSize.y) continue;

            if (room.Size.x / 2 > minRoomSize.x) splitHorizontally = true;
            else if (room.Size.y / 2 > minRoomSize.y) splitHorizontally = false;

            (Room newRoom1, Room newRoom2) = room.Split(splitHorizontally);

            rooms.Remove(room);
            rooms.Add(newRoom1);
            rooms.Add(newRoom2);

            roomQueue.Enqueue((newRoom1, splitHorizontally));
            roomQueue.Enqueue((newRoom2, !splitHorizontally));
        }
    }
}
