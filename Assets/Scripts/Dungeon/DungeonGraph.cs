using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGraph
{
    private readonly Graph<Vector2> graph = new();
    private readonly List<Vector2> _nodes = new();
    private readonly HashSet<Vector2> _discoveredNodes = new();

    public List<Vector2> Nodes => _nodes;
    public HashSet<Vector2> DiscoveredNodes => _discoveredNodes;

    private DungeonBuilder.GenerationType generationType;
    private float timeBetweenOperations;

    public void AddNode(Vector2 node) {
        graph.AddNode(node);
    }

    public void RemoveNode(Vector2 node) {
        graph.RemoveNode(node);
    }

    public void AddEdge(Vector2 fromNode, Vector2 toNode) {
        graph.AddEdge(fromNode, toNode);
    }

    public IEnumerator GenerateGraph(List<Room> rooms) {
        foreach (Room room in rooms) {
            graph.AddNode(room.Bounds.center);

            foreach (RectInt door in room.Doors) {
                graph.AddNode(door.position);
                graph.AddEdge(door.position, room.Bounds.center);
            }
        }

        foreach (Vector2 node in graph.GetNodes()) {
            _nodes.Add(node);
            if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
    }

    public IEnumerator SearchGraph() {
        HashSet<Vector2> discovered = graph.BFS(graph.GetNodes()[0]);

        foreach (Vector2 node in discovered) {
            _discoveredNodes.Add(node);

            if (generationType != DungeonBuilder.GenerationType.INSTANT) yield return GenerationHelper.WaitForGeneration(generationType, timeBetweenOperations);
        }
    }

    public List<Vector2> GetNeighbors(Vector2 node) {
        return graph.GetNeighbors(node);
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        this.generationType = generationType;
        this.timeBetweenOperations = timeBetweenOperations;
    }
}
