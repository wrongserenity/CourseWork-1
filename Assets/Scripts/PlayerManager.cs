using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject levelOrigin;

    Level level;

    // spawn parameters
    int toPlayerSpawnIndex = 1;
    int activeCubesAmount = 3;
    int curSpawnIter = 0;
    float spawnDelay = 0f;
    float spawnDelayTimer = 0f;

    public float cubePassTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        spawnDelay = 3f / (float)activeCubesAmount;

        level = Instantiate(levelOrigin, new Vector3(0, 0, 0), new Quaternion(0, 0, 1, 0)).GetComponent<Level>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (spawnDelayTimer < 0.0001f)
        {
            spawnDelayTimer = spawnDelay;
            SpawnerSystem();
        }
        else
        {
            spawnDelayTimer -= Time.deltaTime;
        }
    }

    void SpawnerSystem()
    {
        int xOrZ = Random.Range(0, 2);
        int forwardOrBackward = Random.Range(0, 2);
        int posInt = Random.Range(0, 8);

        curSpawnIter++;

    }
}
