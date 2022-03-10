using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Manager manager;
    public Agent agent;
    public DamageCubeSpawner dcs;

    // Start is called before the first frame update
    void Start()
    {
        agent = transform.GetComponentInChildren<Agent>();
        agent.level = this;
        agent.spawnPos = transform.position + new Vector3(0.5f, -0.5f, 0.5f);
        dcs = transform.GetComponentInChildren<DamageCubeSpawner>();
    }

    public void Deactivate()
    {
        dcs.Disable();
        manager.levelsLeft--;
    }

    public void Reload()
    {
        dcs.spawning = true;
        agent.reloadReq = true;
    }
}
