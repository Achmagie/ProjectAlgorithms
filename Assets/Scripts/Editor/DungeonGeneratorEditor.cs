using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        DungeonGenerator dungeonGenerator = (DungeonGenerator)target;
       
        if (GUILayout.Button("Generate Dungeon")) {
            dungeonGenerator.StartDungeonGeneration();
        }

        if (GUILayout.Button("Generate Doors")) {
            dungeonGenerator.StartDoorGeneration();
        }

        if (GUILayout.Button("Generate Graph")) {
            dungeonGenerator.StartGraphGeneration();
        }

        if (GUILayout.Button("Start DFS")) {
            dungeonGenerator.StartGraphSearch();
        }
    }
}
