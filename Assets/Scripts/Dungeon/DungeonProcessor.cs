using System.Collections;
using UnityEngine;

public class DungeonProcessor : MonoBehaviour
{
    [Header("Processing Params")]
    [SerializeField] private float timeBetweenOperations;
    [SerializeField] private ProcessingType _generationType;

    public ProcessingType GenerationType => _generationType;

    public enum ProcessingType {
        INSTANT,
        TIMED,
        KEYPRESS
    }

    public static DungeonProcessor Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    public IEnumerator WaitForGeneration() {
        switch (GenerationType) {
            case ProcessingType.TIMED:
                yield return new WaitForSeconds(timeBetweenOperations);
                break;
            
            case ProcessingType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
    }
}
