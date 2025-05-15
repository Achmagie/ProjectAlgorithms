using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGraph
{
    private readonly Graph<Vector2> graph = new();

    public List<Vector2> Nodes { get; private set; } = new();
    public HashSet<Vector2> DiscoveredNodes { get; private set; } = new();

    public void AddNode(Vector2 node) {
        graph.AddNode(node);
    }

    public void RemoveNode(Vector2 node) {
        graph.RemoveNode(node);
    }

    public void AddEdge(Vector2 fromNode, Vector2 toNode) {
        graph.AddEdge(fromNode, toNode);
    }

    /// <summary>
    /// Creates a graph representation of the dungeon by adding nodes for each room and door and connecting them with edges
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns>Yields execution based on generation type</returns>
    public IEnumerator GenerateGraph(List<Room> rooms) {
        foreach (Room room in rooms) {
            graph.AddNode(room.Bounds.center);

            foreach (RectInt door in room.Doors) {
                graph.AddNode(door.position);
                graph.AddEdge(door.position, room.Bounds.center);
            }
        }

        foreach (Vector2 node in graph.GetNodes()) {
            Nodes.Add(node);
            if (DungeonProcessor.Instance.GenerationType != DungeonProcessor.ProcessingType.INSTANT) yield return DungeonProcessor.Instance.WaitForGeneration();
        }
    }

    /// <summary>
    /// Gets the discovered nodes from running breadth first search and adds them to list of discovered nodes
    /// </summary>
    /// <returns>Yields execution based on generation type</returns>
    public IEnumerator SearchGraph() {
        HashSet<Vector2> discovered = graph.BFS(graph.GetNodes()[0]);

        foreach (Vector2 node in discovered) {
            DiscoveredNodes.Add(node);

            if (DungeonProcessor.Instance.GenerationType != DungeonProcessor.ProcessingType.INSTANT) yield return DungeonProcessor.Instance.WaitForGeneration();
        }
    }

    public List<Vector2> GetNeighbors(Vector2 node) {
        return graph.GetNeighbors(node);
    }
}
