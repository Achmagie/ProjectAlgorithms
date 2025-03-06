using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] Vector2Int dungeonSize;
    [SerializeField] private Vector2Int minRoomSize;
    [SerializeField] private float timeBetweenRooms;
    [SerializeField] private GenerationType generationType;

    public enum GenerationType {
        INSTANT,
        TIMED,
        KEYPRESS
    }
    
    private List<Room> rooms = new List<Room>();
    private Room startRoom;

    void Start() {
        startRoom = new Room(new Vector2Int(0, 0), dungeonSize);
    }

    void Update() {
        if (rooms.Count == 0) return;

        foreach (Room room in rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, Color.red);
        }
    }

    private IEnumerator GenerateDungeon() {
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
                    yield return new WaitForSeconds(timeBetweenRooms);
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
}
