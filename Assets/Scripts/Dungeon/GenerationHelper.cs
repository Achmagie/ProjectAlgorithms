using System.Collections;
using UnityEngine;

public static class GenerationHelper 
{
    public static IEnumerator WaitForGeneration(DungeonBuilder.GenerationType generationType, float timeBetweenOperations) {
        switch (generationType) {
            case DungeonBuilder.GenerationType.TIMED:
                yield return new WaitForSeconds(timeBetweenOperations);
                break;
            
            case DungeonBuilder.GenerationType.KEYPRESS:
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
                break;
        }
    }
}