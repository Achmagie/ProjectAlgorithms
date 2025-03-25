using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator 
{
    private readonly List<Room> _rooms = new();
    private readonly List<RectInt> _doors = new();

    public List<Room> Rooms => _rooms;
    public List<RectInt> Doors => _doors;

    private DungeonBuilder.GenerationType generationType;

    private float timeBetweenOperations;

    public IEnumerator GenerateDungeon(Vector2Int dungeonSize, Vector2Int minRoomSize, DungeonGraph graph, int seed) {
        System.Random rand = new(seed);

        Room startRoom = new Room(new Vector2Int(0, 0), dungeonSize);

        _rooms.Clear();
        _rooms.Add(startRoom);
        
        Queue<(Room, bool)> roomQueue = new Queue<(Room, bool)>();
        roomQueue.Enqueue((startRoom, rand.Next(0, 2) == 1));

        while (roomQueue.Count > 0) {
            (Room room, bool splitHorizontally) = roomQueue.Dequeue();
            
            (Room newRoom1, Room newRoom2) = room.Split(splitHorizontally, minRoomSize, rand);

            if (newRoom1 == null && newRoom2 == null) continue;

            _rooms.Remove(room);
            _rooms.AddRange(new[] {newRoom1, newRoom2});

            roomQueue.Enqueue((newRoom1, splitHorizontally));
            roomQueue.Enqueue((newRoom2, !splitHorizontally));

            if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
        }

        _rooms.ForEach(r => graph.AddNode(r.Bounds.center));
    }

    public IEnumerator GenerateDoors(DungeonGraph graph) {
        _doors.Clear();

        for (int i = 0; i < _rooms.Count; i++) {
            for (int j = i + 1; j < _rooms.Count; j++) {
                CreateDoor(_rooms[i], _rooms[j], graph);
                if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
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

    private void CreateDoor(Room room1, Room room2, DungeonGraph graph) {
        RectInt intersection = AlgorithmsUtils.Intersect(room1.Bounds, room2.Bounds);

        if (intersection.width <= 4 && intersection.height <= 4) return;

        Vector2Int doorPosition;

        if (intersection.width > 1) doorPosition = new Vector2Int(Random.Range(intersection.xMin + 2, intersection.xMin + intersection.width - 2), intersection.yMin);
        else if (intersection.height > 1) doorPosition = new Vector2Int(intersection.xMin, Random.Range(intersection.yMin + 2, intersection.yMin + intersection.height - 2));
        else return;

        RectInt door = new RectInt(doorPosition, new Vector2Int(1, 1));

        _doors.Add(door);
        graph.AddNode(doorPosition);

        graph.AddEdge(doorPosition, room1.Bounds.center);
        graph.AddEdge(doorPosition, room2.Bounds.center);

        room1.AddDoor(door);
        room2.AddDoor(door);
    }

    public IEnumerator PurgeRooms(DungeonGraph graph, DungeonBuilder.PurgeType purgeType) {
        switch (purgeType) {
            case DungeonBuilder.PurgeType.TEN_PROCENT:
                List<Room> smallestRooms = _rooms.OrderByDescending(r => r.Area).Where(r => graph.GetNeighbors(r.Bounds.center).Count >= 2).TakeLast((int)Mathf.Ceil(_rooms.Count * .1f)).ToList();
                
                foreach (Room room in smallestRooms) {
                    foreach (RectInt door in room.Doors) {
                        _doors.Remove(door);
                        graph.RemoveNode(door.position);
                    }

                    _rooms.Remove(room);
                    graph.RemoveNode(room.Bounds.center);

                    yield return WaitForGeneration();
                }

                break;
        }
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        this.generationType = generationType;
        this.timeBetweenOperations = timeBetweenOperations;
    }

    private IEnumerator WaitForGeneration() {
        switch (generationType) {
            case DungeonBuilder.GenerationType.TIMED:
                yield return new WaitForSeconds(timeBetweenOperations);
                break;
            
            case DungeonBuilder.GenerationType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
    }
}
