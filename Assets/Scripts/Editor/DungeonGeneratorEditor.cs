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
    }
}
