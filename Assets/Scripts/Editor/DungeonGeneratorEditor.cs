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

        if (GUILayout.Button("Generate Graph")) {
            builder.StartGraphGeneration();
        }

        if (GUILayout.Button("Search Graph")) {
            builder.StartGraphSearch();
        }
    }
}
