using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools : MonoBehaviour
{
    [Header("Test Room Settings")]
    public GameObject enemyPrefab;
    public List<Transform> spawnPoints;
    public List<EnemyHealth> enemyList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            RespawnEnemies();
        }
    }

    public void RespawnEnemies()
    {
        if(enemyList != null)
        {
            foreach (EnemyHealth enemy in enemyList)
            {
                if(enemy.gameObject.GetComponentsInChildren<KnifeProjectile>() != null)
                {
                    KnifeProjectile[] knives = enemy.gameObject.GetComponentsInChildren<KnifeProjectile>();
                    foreach(KnifeProjectile knife in knives)
                    {
                        knife.DropKnife();
                    }
                }
                Destroy(enemy.gameObject);
            }

            enemyList.Clear();

            for(int i = 0; i < spawnPoints.Count; i++)
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                EnemyHealth newEnemyHealth = newEnemy.GetComponent<EnemyHealth>();
                if (newEnemyHealth != null)
                {
                    enemyList.Add(newEnemyHealth);
                }

                if(i == 1) // gross hardcoded shit for now just ignore
                {
                    newEnemyHealth.gameObject.GetComponent<Enemy3D>().pathPoints = new List<Vector3> { new Vector3(-spawnPoints[1].position.x, 0, -5), new Vector3(spawnPoints[1].position.x, 0, -5)};
                }
            }
        }
    }
}
