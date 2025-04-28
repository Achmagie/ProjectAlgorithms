using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
    

    /// <summary>
    /// Generates a dungeon layout using binary space partitioning
    /// The dungeon is split into rooms until all rooms meet the minimum size requirements
    /// The process is randomized using the provided seed
    /// </summary>
    /// <param name="dungeonSize">Total size of the dungeon</param>
    /// <param name="minRoomSize">Minimum size a room has to be</param>
    /// <param name="seed">The value used to randomize room splitting</param>
    /// <returns>Yields execution based on generation type</returns>
    public IEnumerator GenerateDungeon(Vector2Int dungeonSize, Vector2Int minRoomSize, int seed) {
        System.Random rand = new(seed);

        Room startRoom = new(new Vector2Int(0, 0), dungeonSize);

        _rooms.Clear();
        _rooms.Add(startRoom);
        
        Queue<(Room, bool)> roomQueue = new();
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

    // private IEnumerator SpawnWalls(Room room, GameObject wallPrefab) {
    //     for (int i = 0; i < room.Size.x; i++) {
    //         Instantiate
    //     }
    // }

    /// <summary>
    /// Attempts to create doors between all pairs of rooms in the dungeon
    /// </summary>
    /// <returns>Yields execution based on generation type</returns>
    public IEnumerator GenerateDoors(int seed) {
        _doors.Clear();

        for (int i = 0; i < _rooms.Count; i++) {
            for (int j = i + 1; j < _rooms.Count; j++) {
                if (CreateDoor(_rooms[i], _rooms[j], seed) && generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
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

    /// <summary>
    /// Creates a door between two rooms if they intersect
    /// A door is placed randomly within the overlapping area
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    private bool CreateDoor(Room room1, Room room2, int seed) {
        System.Random rand = new(seed);

        RectInt intersection = AlgorithmsUtils.Intersect(room1.Bounds, room2.Bounds);

        if (intersection.width <= DOOR_SPACE && intersection.height <= DOOR_SPACE) return false;

        Vector2Int doorPosition;

        if (intersection.width > 1) doorPosition = new Vector2Int(rand.Next(intersection.xMin + DOOR_SEPARATION, intersection.xMin + intersection.width - DOOR_SEPARATION + 1), intersection.yMin);
        else if (intersection.height > 1) doorPosition = new Vector2Int(intersection.xMin, rand.Next(intersection.yMin + 2, intersection.yMin + intersection.height - DOOR_SEPARATION + 1));
        else return false;

        RectInt door = new(doorPosition, new Vector2Int(1, 1));

        _doors.Add(door);

        room1.AddDoor(door);
        room2.AddDoor(door);

        return true;
    }

    /// <summary>
    /// Removes the smallest 10% of rooms from the dungeon while ensuring the dungeon remains fully connected
    /// If removing a room breaks dungeon connectivity, the room and its connections are restored
    /// </summary>
    /// <returns>Yields execution based on generation type</returns>
    public IEnumerator PurgeRooms() {
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

    /// <summary>
    /// Removes unnecessary doors from the dungeon using a minimum spanning tree algorithm to ensure full connectivity
    /// </summary>
    /// <param name="graph">The dungeon graph for rooms and door connections</param>
    /// <returns></returns>
    public IEnumerator PurgeDoors(DungeonGraph graph) {
        (HashSet<Vector2> mstDoors, List<Vector2> mst) = PrimMST(graph);

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

    /// <summary>
    /// https://www.geeksforgeeks.org/prims-minimum-spanning-tree-mst-greedy-algo-5/
    /// </summary>
    /// <param name="graph">The dungeon graph for rooms and door connections</param>
    /// <returns>All doors present in the minimum spanning tree as well as the minimum spanning tree</returns>
    private (HashSet<Vector2>, List<Vector2>) PrimMST(DungeonGraph graph) {
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

        return (mstDoors, mst);
    }

    // Checks if given node is present in list of doors
    private bool IsDoor(Vector2 node) {
        return _doors.Any(d => d.position == node);
    }

    // Uses a modified DFS algorithm to check if the dungeon is still fully connected
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

    // Gets connected rooms through the given door
    private Room GetConnectedRoom(RectInt door, Room currentRoom) {
        return _rooms.FirstOrDefault(r => r != currentRoom && r.Doors.Contains(door));
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        this.generationType = generationType;
        this.timeBetweenOperations = timeBetweenOperations;
    }
}
