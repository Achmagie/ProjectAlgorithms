using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int areaSize;
    [SerializeField] private Vector2Int minRoomSize;
    
    private List<Room> rooms = new List<Room>();
    private Room parentRoom;

    [SerializeField] private bool splitHorizontally; 


    private bool minSizeHit = false;

    void Start() {
        parentRoom = new Room(new Vector2Int(0, 0), areaSize);

        SplitRoom(parentRoom);
    }

    void Update() {
        if (rooms.Count == 0) return;

        foreach (Room room in rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, Color.red);
        }
    }

    private void SplitRoom(Room room) {
        if (room.Size.x <= minRoomSize.x || room.Size.y <= minRoomSize.y) return;

        (Room newRoom1, Room newRoom2) =  room.Split(splitHorizontally);

        rooms.Add(newRoom1);
        rooms.Add(newRoom2);

        splitHorizontally = !splitHorizontally;

        SplitRoom(newRoom1);
        SplitRoom(newRoom2);
    }
}
