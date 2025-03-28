using System.Collections.Generic;
using UnityEngine;

public class DungeonPainter
{
    public void PaintRooms(List<Room> rooms, Color color) {
        foreach (Room room in rooms) {
            AlgorithmsUtils.DebugRectInt(room.Bounds, color);
        }
    }

    public void PaintDoors(List<RectInt> doors, Color color) {
        foreach (RectInt door in doors) {
            AlgorithmsUtils.DebugRectInt(door, color);
        }
    } 

    public void PaintGraph(DungeonGraph graph, Color nodeColor, Color colorLine) {
        foreach (Vector2 node in graph.Nodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in graph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }

    public void PaintPath(DungeonGraph graph, Color nodeColor, Color colorLine) {
        foreach (Vector2 node in graph.DiscoveredNodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in graph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }
}
