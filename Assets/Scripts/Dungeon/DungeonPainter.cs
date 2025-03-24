using UnityEngine;

public class DungeonPainter
{
    private DungeonGenerator generator;
    private DungeonGraph graph;

    public DungeonPainter(DungeonGenerator dungeonGenerator, DungeonGraph dungeonGraph) {
        generator = dungeonGenerator;
        graph = dungeonGraph;
    }

    public void PaintRooms(Color color) {
        foreach (Room room in generator.Rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, color);
        }
    }

    public void PaintDoors(Color color) {
        foreach (RectInt door in generator.Doors) {
            AlgorithmsUtils.DebugRectInt(door, color);
        }
    } 

    public void PaintGraph(Color nodeColor, Color colorLine) {
        foreach (Vector2 node in graph.Nodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in graph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }

    public void PaintPath(Color nodeColor, Color colorLine) {
        foreach (Vector2 node in graph.DiscoveredNodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in graph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }
}
