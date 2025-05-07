using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonBuilder))]
public class DungeonGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        DungeonBuilder builder = (DungeonBuilder)target;
       
        if (GUILayout.Button("Generate Dungeon")) {
            builder.StartDungeonGeneration();
        }

        if (GUILayout.Button("Generate Doors")) {
            builder.StartDoorGeneration();
        }
        
        if (GUILayout.Button("Purge Rooms")) {
            builder.StartRoomPurge();
        }

        if (GUILayout.Button("Generate Graph")) {
            builder.StartGraphGeneration();
        }

        if (GUILayout.Button("Purge Doors")) {
            builder.StartDoorPurge();
        }

        if (GUILayout.Button("Search Graph")) {
            builder.StartGraphSearch();
        }

        if (GUILayout.Button("Generate TileMap")) {
            builder.GenerateTileMap();
        }

        if (GUILayout.Button("Print TileMap")) {
            builder.PrintTileMap();
        }

        if (GUILayout.Button("Spawn Walls")) {
            builder.StartSpawnWalls();
        }

        if (GUILayout.Button("Bake Mesh")) {
            builder.BakeNavMesh();
        }
    }
}
