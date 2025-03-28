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

    private const int DOOR_SPACE = 4;
    private const int DOOR_SEPARATION = 2;
    

    public IEnumerator GenerateDungeon(Vector2Int dungeonSize, Vector2Int minRoomSize, int seed) {
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

            if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
    }

    public IEnumerator GenerateDoors() {
        _doors.Clear();

        for (int i = 0; i < _rooms.Count; i++) {
            for (int j = i + 1; j < _rooms.Count; j++) {
                CreateDoor(_rooms[i], _rooms[j]);
                if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
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

    private void CreateDoor(Room room1, Room room2) {
        RectInt intersection = AlgorithmsUtils.Intersect(room1.Bounds, room2.Bounds);

        if (intersection.width <= DOOR_SPACE && intersection.height <= DOOR_SPACE) return;

        Vector2Int doorPosition;

        if (intersection.width > 1) doorPosition = new Vector2Int(Random.Range(intersection.xMin + DOOR_SEPARATION, intersection.xMin + intersection.width - DOOR_SEPARATION), intersection.yMin);
        else if (intersection.height > 1) doorPosition = new Vector2Int(intersection.xMin, Random.Range(intersection.yMin + 2, intersection.yMin + intersection.height - DOOR_SEPARATION));
        else return;

        RectInt door = new(doorPosition, new Vector2Int(1, 1));

        _doors.Add(door);

        room1.AddDoor(door);
        room2.AddDoor(door);
    }

    public IEnumerator PurgeRooms() {
        Graph<Room> roomGraph = new();
        List<Room> smallestRooms = _rooms.OrderBy(r => r.Area).Take((int)Mathf.Ceil(_rooms.Count * 0.1f)).ToList();
        
        foreach (Room room in smallestRooms) {
            List<(Room neighbor, RectInt door)> affectedNeighbors = new();

            foreach (RectInt door in room.Doors) {
                Room neighbor = GetConnectedRoom(door, room);
                if (neighbor != null) {
                    affectedNeighbors.Add((neighbor, door));
                    neighbor.Doors.Remove(door);
                }

                _doors.Remove(door);    
            }

            _rooms.Remove(room);
            room.Doors.Clear();

            if (!IsDungeonConnected()) {
                _rooms.Add(room);

                foreach (var (neighbor, door) in affectedNeighbors) {
                    neighbor.Doors.Add(door);
                    _doors.Add(door);
                }

                room.Doors.AddRange(affectedNeighbors.Select(n => n.door));

            } else yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
    }

    public IEnumerator PurgeDoors(DungeonGraph graph) {
        List<Vector2> mst = new();
        HashSet<Vector2> visitedRooms = new();
        List<(Vector2 room, Vector2 door, int cost)> queue = new();

        HashSet<Vector2> mstDoors = new();

        Vector2 startRoom = _rooms[0].Bounds.center;
        mst.Add(startRoom);
        visitedRooms.Add(startRoom);

        foreach (Vector2 door in graph.GetNeighbors(startRoom)) {
            foreach (Vector2 neighborRoom in graph.GetNeighbors(door)) {
                if (!visitedRooms.Contains(neighborRoom)) {
                    int cost = graph.GetNeighbors(neighborRoom).Count;
                    queue.Add((neighborRoom, door, cost));
                }
            }
        }
        
        while (queue.Count > 0) {
            queue.Sort((a, b) => a.cost.CompareTo(b.cost));
            (var nextRoom, var connectingDoor, _) = queue[0];
            queue.RemoveAt(0);

            if (visitedRooms.Contains(nextRoom)) continue;

            mst.Add(nextRoom);
            visitedRooms.Add(nextRoom); 
            mstDoors.Add(connectingDoor);

            foreach (Vector2 door in graph.GetNeighbors(nextRoom)) {
                foreach (Vector2 neighborRoom in graph.GetNeighbors(door)) {
                    if (!visitedRooms.Contains(neighborRoom)) {
                        int cost = graph.GetNeighbors(neighborRoom).Count;
                        queue.Add((neighborRoom, door, cost));
                    }
                }
            }
        }

        List<Vector2> allDoors = graph.Nodes.Where(node => IsDoor(node)).ToList();
        foreach (Vector2 door in allDoors) {
            if (!mstDoors.Contains(door)) {
                graph.RemoveNode(door);
                _doors.Remove(new RectInt(new Vector2Int((int)door.x, (int)door.y), new Vector2Int(1, 1)));
            }
        }

        graph.Nodes.Clear();
        
        foreach (Vector2 node in mst) {
            graph.Nodes.Add(node);
            if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
    }

    private bool IsDoor(Vector2 node) {
        return _doors.Any(d => d.position == node);
    }

    private bool IsDungeonConnected() {
        if (_rooms.Count == 0) return false;

        HashSet<Room> visited = new();
        Stack<Room> stack = new();
        
        stack.Push(_rooms[0]);

        while (stack.Count > 0) {
            Room current = stack.Pop();
            if (!visited.Add(current)) continue;

            foreach (RectInt door in current.Doors) {
                Room neighbor = _rooms.FirstOrDefault(r => r != current && r.Doors.Contains(door));
                if (neighbor != null && !visited.Contains(neighbor)) {
                    stack.Push(neighbor);
                }
            }
        }

        return visited.Count == _rooms.Count;
    }

    private Room GetConnectedRoom(RectInt door, Room currentRoom) {
        return _rooms.FirstOrDefault(r => r != currentRoom && r.Doors.Contains(door));
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        this.generationType = generationType;
        this.timeBetweenOperations = timeBetweenOperations;
    }
}
