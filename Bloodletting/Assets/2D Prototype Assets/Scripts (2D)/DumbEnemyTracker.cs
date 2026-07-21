using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DumbEnemyTracker : MonoBehaviour
{
    [SerializeField] List<EnemyHealth> trackedEnemies = new List<EnemyHealth>();
    [SerializeField] UnityEvent onAllEnemiesDefeated;

    private void Start()
    {
        trackedEnemies.Clear();
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        trackedEnemies = enemies.ToList();

        Time.timeScale = 1f; // Ensure the game is running
    }

    private void Update()
    {

        trackedEnemies.RemoveAll(item => item == null);
        

        if (trackedEnemies.Count<=0)
        {
            onAllEnemiesDefeated?.Invoke();
            Time.timeScale = 0f; // Pause the game
        }
    }
}
