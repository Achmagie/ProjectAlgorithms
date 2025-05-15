using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonManager))]
public class DungeonGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        DungeonManager manager = (DungeonManager)target;
       
        if (GUILayout.Button("Generate Dungeon")) {
            manager.StartDungeonGeneration();
        }

        if (GUILayout.Button("Generate Doors")) {
            manager.StartDoorGeneration();
        }
        
        if (GUILayout.Button("Purge Rooms")) {
            manager.StartRoomPurge();
        }

        if (GUILayout.Button("Generate Graph")) {
            manager.StartGraphGeneration();
        }

        if (GUILayout.Button("Purge Doors")) {
            manager.StartDoorPurge();
        }

        if (GUILayout.Button("Search Graph")) {
            manager.StartGraphSearch();
        }

        if (GUILayout.Button("Generate TileMap")) {
            manager.GenerateTileMap();
        }

        if (GUILayout.Button("Print TileMap")) {
            manager.PrintTileMap();
        }

        if (GUILayout.Button("Spawn Tiles")) {
            manager.SpawnTiles();
        }

        if (GUILayout.Button("Spawn Floor")) {
            manager.SpawnFloor();
        }

        if (GUILayout.Button("Bake Mesh")) {
            manager.BakeNavMesh();
        }
    }
}
