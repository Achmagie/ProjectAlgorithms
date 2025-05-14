using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{    
    [SerializeField]
    private float speed = 5f;
    
    private bool isMoving = false;

    private Vector3 clickPosition;
    private NavMeshAgent navMeshAgent;

    public DungeonBuilder pathFinder;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo)) {
                clickPosition = hitInfo.point;

                navMeshAgent.SetDestination(clickPosition);
                GoToDestination(clickPosition);
            }
        }

        Debug.DrawRay(Camera.main.transform.position, clickPosition - Camera.main.transform.position);
        DebugExtension.DebugWireSphere(clickPosition, Color.blue, 2);
    }

    public void GoToDestination(Vector3 destination) {
        if (!isMoving) StartCoroutine(FollowPathCoroutine(pathFinder.CalculatePath(transform.position, destination)));   
    }

    IEnumerator FollowPathCoroutine(List<Vector3> path) {
        if (path == null || path.Count == 0) {
            Debug.Log("No path found");
            yield break;
        }
        
        isMoving = true;

        for (int i = 0; i < path.Count; i++) {
            Vector3 target = path[i];
            // Move towards the target position
            while (Vector3.Distance(transform.position, target) > 0.1f) {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
                yield return null;
            }
            
            Debug.Log($"Reached target: {target}");
        }

        isMoving = false;
    }
}
