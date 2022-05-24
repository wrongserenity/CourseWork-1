using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCubeSpawner : MonoBehaviour
{
    public Level level;
    public float spawnDelayTimer    = 0f;
    public bool spawning            = false;

    private int cubesAmount = 5;
    private float boxSpeed  = 0f;

    private List<DamageCube> damageCubes    = new List<DamageCube>();
    private List<bool> readyDamageCubes     = new List<bool>();

    void Start()
    {
        boxSpeed = 10f / level.manager.cubePassTime;
        CheckForCubesCount();
    }

    void FixedUpdate()
    {
        if (!spawning)
            for (int i = cubesAmount - 1; i >= 0; i--)
                if (readyDamageCubes[i])
                    damageCubes[i].disA = true;
    }

    DamageCube CheckForCubesCount()
    {
        if (damageCubes.Count < cubesAmount)
        {
            GameObject go = Resources.Load("Prefabs/DamageCube") as GameObject;
            for (int i = cubesAmount - damageCubes.Count; i > 0; i--)
            {
                damageCubes.Add(Instantiate(go, transform.position + new Vector3(0f, -1f, 0f), transform.rotation).GetComponent<DamageCube>());
                readyDamageCubes.Add(true);
                damageCubes[damageCubes.Count - 1].transform.SetParent(transform);
                damageCubes[damageCubes.Count - 1].gameObject.SetActive(false);
                damageCubes[damageCubes.Count - 1].posOffset = transform.position;
            }
        }
        return damageCubes[damageCubes.Count - 1];
    }

    public void SetReady(DamageCube dc) { readyDamageCubes[damageCubes.FindIndex(x => x == dc)] = true; }

    public void Disable()
    {
        for (int i = damageCubes.Count - 1; i >= 0; i--)
        {
            damageCubes[i].disA = true;
            readyDamageCubes[i] = true;
        }
        spawning = false;
    }

    DamageCube FindReadyCube()
    {
        DamageCube dc = damageCubes[0];
        bool spawned = false;
        for (int i = cubesAmount - 1; i >= 0; i--)
        {
            if (!spawned && readyDamageCubes[i])
            {
                dc = damageCubes[i];
                readyDamageCubes[i] = false;
                spawned = true;
            }
        }
        if (spawned)
            return dc;
        else
        {
            cubesAmount++;
            return CheckForCubesCount();
        }
    }

    public void SpawnDamageCubeIn()
    {
        DamageCube dc = FindReadyCube();
        if (dc != null)
        {
            dc.ChangeColor(true);
            dc.SetStartPosition(new Vector3(-5f, 0f, -4.5f + (float)level.agent.curIntPos.y));
            dc.SetVelocity(boxSpeed * Vector3.right);
            dc.gameObject.SetActive(true);
        }
    }

    public void SpawnDamageCubeIn(int xOrZ, int forwardOrBackward, int posInt)
    {
        DamageCube dc = FindReadyCube();
        if (dc != null)
        {
            dc.ChangeColor(false);
            if (xOrZ == 0)
            {
                if (forwardOrBackward == 0)
                {
                    dc.SetStartPosition(new Vector3(5f, 0f, (float)posInt - 3.5f));
                    dc.SetVelocity(boxSpeed * Vector3.left);
                }
                else
                {
                    dc.SetStartPosition(new Vector3(-5f, 0f, (float)posInt - 3.5f));
                    dc.SetVelocity(boxSpeed * Vector3.right);
                }
            }
            else
            {
                if (forwardOrBackward == 0)
                {
                    dc.SetStartPosition(new Vector3((float)posInt - 3.5f, 0f, -5f));
                    dc.SetVelocity(boxSpeed * Vector3.forward);
                }
                else
                {
                    dc.SetStartPosition(new Vector3((float)posInt - 3.5f, 0f, 5f));
                    dc.SetVelocity(boxSpeed * Vector3.back);
                }
            }
            dc.gameObject.SetActive(true);
        }
    }
}
