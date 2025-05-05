using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Vector3 clickPosition;
    private NavMeshAgent navMeshAgent;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo)) {
                clickPosition = hitInfo.point;

                navMeshAgent.SetDestination(clickPosition);
            }
        }

        Debug.DrawRay(Camera.main.transform.position, clickPosition - Camera.main.transform.position);
        DebugExtension.DebugWireSphere(clickPosition, Color.blue, 2);
    }
}
