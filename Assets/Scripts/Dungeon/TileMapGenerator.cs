using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TileMapGenerator
{
    private int [,] _tileMap;

    public int [,] GenerateTileMap(Vector2Int dungeonSize, List<Room> rooms, List<RectInt> doors) {
        int [,] tileMap = new int[dungeonSize.y, dungeonSize.x];
        int rows = tileMap.GetLength(0);
        int cols = tileMap.GetLength(1);

        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < cols; x++) {
                tileMap[y, x] = 0;
            }
        }

        foreach (Room room in rooms) {
            for (int x = 0; x < room.Size.x; x++) {
                tileMap[room.Position.y, room.Position.x + x] = 1;
                tileMap[room.Position.y + room.Size.y - 1, room.Position.x + x] = 1;
            }

            for (int y = 0; y < room.Size.y; y++) {
                tileMap[room.Position.y + y, room.Position.x] = 1;
                tileMap[room.Position.y + y, room.Position.x + room.Size.x - 1] = 1;
            }
        }   

        foreach (RectInt door in doors) {
            tileMap[door.y, door.x] = 0;
        }

        _tileMap = tileMap;

        return tileMap;
    }

    private string ToString(bool flip) {
        if (_tileMap == null) return "Tile map not generated yet.";
        
        int rows = _tileMap.GetLength(0);
        int cols = _tileMap.GetLength(1);
        
        var sb = new StringBuilder();
    
        int start = flip ? rows - 1 : 0;
        int end = flip ? -1 : rows;
        int step = flip ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append((_tileMap[i, j]==0?'0':'#')); //Replaces 1 with '#' making it easier to visualize
            }
            sb.AppendLine();
        }
    
        return sb.ToString();
    }

    public void PrintTileMap() {
        Debug.Log(ToString(true));
    }
}
