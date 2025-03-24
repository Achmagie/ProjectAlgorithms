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

    private DungeonBuilder.GenerationType _generationType;
    private float _timeBetweenOperations;

    public void AddNode(Vector2 node) {
        graph.AddNode(node);
    }

    public void AddEdge(Vector2 fromNode, Vector2 toNode) {
        graph.AddEdge(fromNode, toNode);
    }

    public IEnumerator GenerateGraph() {
        foreach (Vector2 node in graph.GetNodes()) {
            _nodes.Add(node);
            if (_generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
        }
    }

    public IEnumerator SearchGraph() {
        HashSet<Vector2> discovered = graph.BFS(graph.GetNodes()[0]);

        foreach (Vector2 node in discovered) {
            _discoveredNodes.Add(node);

            if (_generationType != DungeonBuilder.GenerationType.INSTANT) yield return WaitForGeneration();
        }
    }

    public List<Vector2> GetNeighbors(Vector2 node) {
        return graph.GetNeighbors(node);
    }

    public void SetGenType(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        _generationType = generationType;
        _timeBetweenOperations = timeBetweenOperations;
    }


    private IEnumerator WaitForGeneration() {
        switch (_generationType) {
            case DungeonBuilder.GenerationType.TIMED:
                yield return new WaitForSeconds(_timeBetweenOperations);
                break;
            
            case DungeonBuilder.GenerationType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
    }
}
