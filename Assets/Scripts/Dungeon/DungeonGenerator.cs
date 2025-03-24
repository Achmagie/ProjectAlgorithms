using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator 
{
    private readonly List<Room> _rooms = new();
    private readonly List<RectInt> _doors = new();

    private DungeonBuilder.GenerationType _generationType;
    private float _timeBetweenOperations;

    public List<Room> Rooms => _rooms;
    public List<RectInt> Doors => _doors;

    public IEnumerator GenerateDungeon(Vector2Int dungeonSize, Vector2Int minRoomSize, DungeonGraph graph) {
        Room startRoom = new Room(new Vector2Int(0, 0), dungeonSize);

        _rooms.Clear();
        _rooms.Add(startRoom);
        
        Queue<(Room, bool)> roomQueue = new Queue<(Room, bool)>();
        roomQueue.Enqueue((startRoom, Random.value > .5f));

        while (roomQueue.Count > 0) {
            (Room room, bool splitHorizontally) = roomQueue.Dequeue();
            
            (Room newRoom1, Room newRoom2) = room.Split(splitHorizontally, minRoomSize);

            if (newRoom1 == null && newRoom2 == null) continue;

            _rooms.Remove(room);
            _rooms.AddRange(new[] {newRoom1, newRoom2});

            roomQueue.Enqueue((newRoom1, splitHorizontally));
            roomQueue.Enqueue((newRoom2, !splitHorizontally));

            if (_generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
        }

        _rooms.ForEach(r => graph.AddNode(r.Bounds.center));
    }

    public IEnumerator GenerateDoors(DungeonGraph graph) {
        _doors.Clear();

        for (int i = 0; i < _rooms.Count; i++) {
            for (int j = i + 1; j < _rooms.Count; j++) {
                CreateDoor(_rooms[i].Bounds, _rooms[j].Bounds, graph);
                if (_generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
            }
        }

        // I will revisit this i will make it better
        
        // Dictionary<Vector2Int, Room> roomPositions = rooms.ToDictionary(r => r.Position, r => r);

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
    }

    private void CreateDoor(RectInt room1, RectInt room2, DungeonGraph graph) {
        RectInt intersection = AlgorithmsUtils.Intersect(room1, room2);

        if (intersection.width <= 4 && intersection.height <= 4) return;

        Vector2Int doorPosition;

        if (intersection.width > 1) doorPosition = new Vector2Int(Random.Range(intersection.xMin + 2, intersection.xMin + intersection.width - 2), intersection.yMin);
        else if (intersection.height > 1) doorPosition = new Vector2Int(intersection.xMin, Random.Range(intersection.yMin + 2, intersection.yMin + intersection.height - 2));
        else return;

        _doors.Add(new RectInt(doorPosition, new Vector2Int(1, 1)));
        graph.AddNode(doorPosition);

        graph.AddEdge(doorPosition, room1.center);
        graph.AddEdge(doorPosition, room2.center);
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        _generationType = generationType;
        _timeBetweenOperations = timeBetweenOperations;
    }

    private IEnumerator WaitForGeneration() {
        switch (_generationType) {
            case DungeonBuilder.GenerationType.TIMED:
                yield return new WaitForSeconds(_timeBetweenOperations);
                break;
            
            case DungeonBuilder.GenerationType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
    }
}
