using System.Collections.Generic;
using UnityEngine;

public class DungeonPainter
{
    private DungeonGraph _dungeonGraph;

    public DungeonPainter (DungeonGraph dungeonGraph) {
        _dungeonGraph = dungeonGraph;
    }

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

    public void PaintGraph(Color nodeColor, Color colorLine) {
        foreach (Vector2 node in _dungeonGraph.Nodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in _dungeonGraph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }

    public void PaintPath(Color nodeColor, Color colorLine) {
        foreach (Vector2 node in _dungeonGraph.DiscoveredNodes) {
            DebugExtension.DebugCircle(new Vector3(node.x, 0, node.y), Vector3.up, nodeColor);
            
            foreach (Vector2 edge in _dungeonGraph.GetNeighbors(node)) {
                Debug.DrawLine(new Vector3(node.x, 0, node.y), new Vector3(edge.x, 0, edge.y), colorLine);
            }
        }
    }

    // public void PaintTraversal(Graph<Vector3> graph) {
    //     foreach (var node in _traversalGraph.GetNodes()) {
    //         DebugExtension.DebugWireSphere(node, Color.cyan, .2f);

    //         foreach (var neighbor in _traversalGraph.GetNeighbors(node)) {
    //             Debug.DrawLine(node, neighbor, Color.red);
    //         }
    //     }
    // }
}
