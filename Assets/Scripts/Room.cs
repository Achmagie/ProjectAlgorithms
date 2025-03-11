using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Room
{
    private Vector2Int position;
    private Vector2Int size;
    public readonly List<Room> childRooms = new List<Room>();
    private bool hasTriedToSplit = false;

    private const int WALL_SIZE = 1;

    public Vector2Int Position { 
        get => position;
        private set => position = value;
    }

    public Vector2Int Size {
        get => size;
        private set => size = value;
    }

    public RectInt Bounds => new RectInt(position, size);

    public Room (Vector2Int position, Vector2Int size) {
        this.position = position;
        this.size = size;
    }

    public (Room room1, Room room2) Split(bool splitHorizontally, Vector2Int minRoomSize) {
        if (splitHorizontally) {
            int minX = Position.x + minRoomSize.x;
            int maxX = Position.x + Size.x - minRoomSize.x;

            if (minX >= maxX) {
                if (!hasTriedToSplit) {
                    hasTriedToSplit = true;
                    return Split(!splitHorizontally, minRoomSize);
                } else return (null, null);
            } 

            int splitX = Random.Range(minX, maxX);

            Room room1 = new Room(Position, new Vector2Int(splitX - Position.x, Size.y));
            Room room2 = new Room(new Vector2Int(splitX - WALL_SIZE, Position.y), new Vector2Int(Position.x + Size.x - splitX + WALL_SIZE, Size.y));

            childRooms.AddRange(new Room[] {room1, room2});
            return (room1, room2);
        } else {
            int minY = Position.y + minRoomSize.y;
            int maxY = Position.y + Size.y - minRoomSize.y;

            if (minY >= maxY) {
                if (!hasTriedToSplit) {
                    hasTriedToSplit = true;
                    return Split(!splitHorizontally, minRoomSize);
                } else return (null, null);
            } 

            int splitY = Random.Range(minY, maxY);

            Room room1 = new Room(Position, new Vector2Int(Size.x, splitY - Position.y));
            Room room2 = new Room(new Vector2Int(Position.x, splitY - WALL_SIZE), new Vector2Int(Size.x, Position.y + Size.y - splitY + WALL_SIZE));

            childRooms.AddRange(new Room[] {room1, room2});
            return (room1, room2);
        }
    }

    public bool ChildIntersects() {
        return AlgorithmsUtils.Intersects(childRooms[0].Bounds, childRooms[1].Bounds);
    }
}
