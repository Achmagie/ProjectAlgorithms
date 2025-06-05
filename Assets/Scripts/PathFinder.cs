using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    private Vector3 startNode;
    private Vector3 endNode;
    
    private HashSet<Vector3> discovered = new();
    
    private Graph<Vector3> _graph;

    public void SetGraph(Graph<Vector3> graph) {
        _graph = graph;
    }

    private Vector3 GetClosestNodeToPosition(Vector3 position) {
        Vector3 closestNode = Vector3.zero;
        float closestDistance = Mathf.Infinity;
        
        foreach(Vector3 node in _graph.GetNodes()) {
            if ((node - position).magnitude < closestDistance) {
                closestDistance = (node - position).magnitude;
                closestNode = node;
            }            
        }

        return closestNode;
    }

    public List<Vector3> CalculatePath(Vector3 from, Vector3 to) {
        Vector3 playerPosition = from;
        
        startNode = GetClosestNodeToPosition(playerPosition);
        endNode = GetClosestNodeToPosition(to);

        List<Vector3> shortestPath = AStar(startNode, endNode);
        
        return shortestPath;
    }

    List<Vector3> AStar(Vector3 start, Vector3 end) {
        //Use this "discovered" list to see the nodes in the visual debugging used on OnDrawGizmos()
        discovered.Clear();
        
        List<(Vector3 node, float priority)> nodeQ = new();
        Dictionary<Vector3, float> cost = new();
        Dictionary<Vector3, Vector3> path = new();

        nodeQ.Add((start, 0));
        cost[start] = 0;

        while (nodeQ.Count > 0) {
            nodeQ = nodeQ.OrderByDescending(n => n.priority).ToList();
            Vector3 node = nodeQ[^1].node;
            nodeQ.RemoveAt(nodeQ.Count - 1);

            if (node == end) return ReconstructPath(path, start, end);

            foreach(Vector3 neighbor in _graph.GetNeighbors(node)) {
                float newCost = cost[node] + Cost(node, neighbor);
                if (!cost.ContainsKey(neighbor) || newCost < cost[neighbor]) {
                    cost[neighbor] = newCost;
                    path[neighbor] = node;

                    nodeQ.Add((neighbor, newCost + Heuristic(neighbor, end)));
                }
            }
        }
        /* */
        return new List<Vector3>(); // No path found
    }

    public float Cost(Vector3 from, Vector3 to) {
        return Vector3.Distance(from, to);
    }
    
    public float Heuristic(Vector3 from, Vector3 to) {
        return Vector3.Distance(from, to);
    }
    
    List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> parentMap, Vector3 start, Vector3 end) {
        List<Vector3> path = new();
        Vector3 currentNode = end;

        while (currentNode != start) {
            path.Add(currentNode);
            currentNode = parentMap[currentNode];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }
}
