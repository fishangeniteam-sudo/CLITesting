using System.Collections.Generic;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject PipePrefab;

    [Header("Spawn Settings")]
    public float SpawnInterval = 1.8f;
    public float MinY = -1.6f;
    public float MaxY = 2.0f;
    public float SpawnX = 8f;

    private float timer = 0f;
    private List<GameObject> activePipes = new List<GameObject>();

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        timer += Time.deltaTime;
        if (timer >= SpawnInterval)
        {
            SpawnPipe();
            timer = 0f;
        }
    }

    public void ResetSpawner()
    {
        timer = 0.5f; // spawn first pipe quickly
        for (int i = activePipes.Count - 1; i >= 0; i--)
        {
            if (activePipes[i] != null)
            {
                Destroy(activePipes[i]);
            }
        }
        activePipes.Clear();
    }

    public void StopSpawning()
    {
        // Keep active pipes in scene so player sees where they hit
    }

    private void SpawnPipe()
    {
        if (PipePrefab == null) return;

        float spawnY = Random.Range(MinY, MaxY);
        Vector3 spawnPos = new Vector3(SpawnX, spawnY, 0f);
        GameObject pipe = Instantiate(PipePrefab, spawnPos, Quaternion.identity);
        activePipes.Add(pipe);
    }
}
