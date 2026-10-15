using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RespawningEnemy : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float respawnDelay = 2.5f;

    Enemy3D enemyScript;
    EnemyHealth healthScript;

    public Vector3 spawnPosition;
    public Quaternion spawnRotation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();
        enemyScript = GetComponent<Enemy3D>();

        spawnPosition = this.gameObject.transform.position;
        spawnRotation = this.gameObject.transform.rotation;

        healthScript.OnDeath.AddListener(() =>
        {
            RespawnSelf();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RespawnSelf()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        newEnemy.GetComponent<Enemy3D>().pathPoints = new List<Vector3>(enemyScript.pathPoints);
    }
}
