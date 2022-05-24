using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Manager manager;
    public Agent agent;
    public DamageCubeSpawner damageCubeSpawner;

    void Start()
    {
        agent               = transform.GetComponentInChildren<Agent>();
        agent.level         = this;
        agent.spawnPos      = transform.position + new Vector3(0.5f, -0.5f, 0.5f);

        damageCubeSpawner   = transform.GetComponentInChildren<DamageCubeSpawner>();
    }

    public void Deactivate()
    {
        damageCubeSpawner.Disable();
        manager.levelsLeft--;
    }

    public void Reload()
    {
        damageCubeSpawner.spawning = true;
        agent.RequestReloading();
    }
}
