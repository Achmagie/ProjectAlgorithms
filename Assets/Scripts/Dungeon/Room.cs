using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Room
{
    private Vector2Int position;
    private Vector2Int size;
    private bool hasTriedToSplit = false;

    private const int WALL_SIZE = 1;

    private readonly List<RectInt> _doors = new();

    public Vector2Int Position { 
        get => position;
        private set => position = value;
    }

    public Vector2Int Size {
        get => size;
        private set => size = value;
    }

    public RectInt Bounds => new RectInt(position, size);
    public float Area => size.x * size.y;

    public List<RectInt> Doors => _doors;

    public Room (Vector2Int position, Vector2Int size) {
        this.position = position;
        this.size = size;
    }

    public (Room room1, Room room2) Split(bool splitHorizontally, Vector2Int minRoomSize, System.Random rand) {
        if (splitHorizontally) {
            int minX = Position.x + minRoomSize.x;
            int maxX = Position.x + Size.x - minRoomSize.x;

            if (minX >= maxX) {
                if (!hasTriedToSplit) {
                    hasTriedToSplit = true;
                    return Split(!splitHorizontally, minRoomSize, rand);
                } else return (null, null);
            } 

            int splitX = rand.Next(minX, maxX + 1);

            Room room1 = new Room(Position, new Vector2Int(splitX - Position.x, Size.y));
            Room room2 = new Room(new Vector2Int(splitX - WALL_SIZE, Position.y), new Vector2Int(Position.x + Size.x - splitX + WALL_SIZE, Size.y));

            return (room1, room2);
        } else {
            int minY = Position.y + minRoomSize.y;
            int maxY = Position.y + Size.y - minRoomSize.y;

            if (minY >= maxY) {
                if (!hasTriedToSplit) {
                    hasTriedToSplit = true;
                    return Split(!splitHorizontally, minRoomSize, rand);
                } else return (null, null);
            } 

            int splitY = rand.Next(minY, maxY + 1);

            Room room1 = new Room(Position, new Vector2Int(Size.x, splitY - Position.y));
            Room room2 = new Room(new Vector2Int(Position.x, splitY - WALL_SIZE), new Vector2Int(Size.x, Position.y + Size.y - splitY + WALL_SIZE));

            return (room1, room2);
        }
    }

    public void AddDoor(RectInt door) {
        _doors.Add(door);
    }
}