using UnityEngine;

public class Room
{
    private Vector2Int position;
    private Vector2Int size;

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

    public (Room room1, Room room2) Split(bool splitHorizontally) {
        if (splitHorizontally) {
            int splitX = Position.x + Size.x / 2;

            Room room1 = new Room(Position, new Vector2Int(Size.x / 2, Size.y));
            Room room2 = new Room(new Vector2Int(splitX - 1, Position.y), new Vector2Int(Size.x - Size.x / 2 + 1, Size.y));

            return (room1, room2);
        } else {
            int splitY = Position.y + Size.y / 2;

            Room room1 = new Room(Position, new Vector2Int(Size.x, Size.y / 2));
            Room room2 = new Room(new Vector2Int(Position.x, splitY - 1), new Vector2Int(Size.x, Size.y - Size.y / 2 + 1));

            return (room1, room2);
        }
    }
}
