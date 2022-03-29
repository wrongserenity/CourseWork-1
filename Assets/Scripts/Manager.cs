using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [Header("General")]
    public bool isPlayMode = false;

    public LogManager logManager;
    public GameObject mainCamera;

    public int populationSize;//creates population size
    public GameObject levelOrigin;//holds bot prefab

    [Range(0.1f, 50f)] public float Gamespeed = 1f;
    public int levelsLeft = 0;

    [Header("Neural Network")]
    public int[] layers = new int[4] { 22, 32, 16, 9 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<Agent> agents;
    private List<Level> levels;
 
    bool isLevelLoading = false;

    int generationCount = 0;
    float lastGenTime = 0f;

    public float NNReactTime = 0.1f;

    List<NeuralNetwork> bestNN = new List<NeuralNetwork>() { };
    int bestNNCount = 10;

    public int resortGeneration = 3;
    int reweigntGeneration = 3;
    float reweightStrength = 0.99f;



    // spawn parameters
    [Header("Gameplay")]
    public int toPlayerSpawnIndex = 2;
    public int activeCubesAmount = 3;
    int curSpawnIter = 0;
    float spawnDelay = 0f;
    float spawnDelayTimer = 0f;

    public float cubePassTime = 3f;


    void Start()
    {
        if (!isPlayMode)
        {
            if (populationSize % 2 != 0)
                populationSize = 10;//if population size is not even, sets it to fifty

            InitNetworks();

            logManager.Message("Start(): Neural Mode Started", this);
        }
        else
        {
            populationSize = 1;
            StartCoroutine(UploadLevels());

            mainCamera.transform.position = new Vector3(0, 10, 0);
            mainCamera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            logManager.Message("Start(): Play Mode Started", this);
        }

        spawnDelay = 3f / (float)activeCubesAmount;
    }

    private void FixedUpdate()
    {
        if (levelsLeft == 0)
        {
            if (!isPlayMode)
            {
                CreateBots();
                for (int i = 0; i < populationSize; i++)
                {
                    levels[i].agent.brain = networks[i];
                    levels[i].Reload();
                }
                levelsLeft = populationSize;
            }
            Debug.Log("generation: " + generationCount + "  last time:  " + (Time.time - lastGenTime) + "\n");
            lastGenTime = Time.time;
            generationCount++;

            logManager.Message("FixedUpdate(): Levels Reloaded", this);
        }

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
        logManager.Message("Spawner System(): STARTED", this);

        int xOrZ = Random.Range(0, 2);
        int forwardOrBackward = Random.Range(0, 2);
        int posInt = Random.Range(0, 8);



        for (int j=0; j < populationSize; j++)
        {
            if (curSpawnIter % toPlayerSpawnIndex == 0)
            {
                if (levels[j].agent.isAlive)
                {
                    levels[j].dcs.SpawnDamageCubeIn();

                }
                curSpawnIter = 0;
            }
            else
            {
                if (levels[j].agent.isAlive)
                {
                    levels[j].dcs.SpawnDamageCubeIn(xOrZ, forwardOrBackward, posInt);

                }
            }
            
        }
        curSpawnIter++;

        logManager.Message("Spawner System(): FINISHED", this);
    }

    public void InitNetworks()
    {
        logManager.Message("InitNetworks(): STARTED", this);

        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            //net.Load("Assets/Save.txt");//on start load the network save
            networks.Add(net);
        }

        for (int i = 0; i < bestNNCount; i++)
        {
            bestNN.Add(networks[populationSize - bestNNCount + i].copy(new NeuralNetwork(layers)));
        }

        logManager.Message("InitNetworks(): FINISHED", this);
    }

    IEnumerator UploadLevels()
    {
        logManager.Message("UploadLevels(): STARTED", this);

        isLevelLoading = true;
        levels = new List<Level>();
        agents = new List<Agent>();

        int lines = Mathf.RoundToInt(Mathf.Pow(populationSize, 0.5f));
        int curLine = 0;

        int spawned = 0;

        while (spawned < populationSize)
        {
            Level level = (Instantiate(levelOrigin, new Vector3((float)curLine * 12f, 1f, (float)(spawned % lines) * 12f), new Quaternion(0, 0, 1, 0))).GetComponent<Level>();//create botes
            level.manager = this;
            levels.Add(level);
            spawned++;
            if (spawned % lines == 0)
                curLine++;
        }
        logManager.Message("UploadLevels(): levels inited", this);

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < populationSize; i++)
        {
            if (!isPlayMode)
                levels[i].agent.brain = networks[i];//deploys network to each learner
            levels[i].Reload();
        }
        levelsLeft = populationSize;
        isLevelLoading = false;

        logManager.Message("UploadLevels(): FINISHED", this);

    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;
        if (levels != null)
        {
            AddNetworksFitness();
            if (generationCount % resortGeneration == 0)
            {
                SortNetworks();
                ClearNetworksFitness();
            }

            if (generationCount % reweigntGeneration == 0)
            {
                MutationStrength *= reweightStrength;
            }
        }
        else
            if (!isLevelLoading)
                StartCoroutine(UploadLevels());
    }

    void AddNetworksFitness()
    {
        // запись
        float evf = MeanValue();
        float df = Difference();
        logManager.WriteNewScore(Time.time - lastGenTime, evf, df);

        foreach (NeuralNetwork network in networks)
            network.AddFitnessToResort();
    }

    void ClearNetworksFitness()
    {
        foreach (NeuralNetwork network in networks)
            network.ClearFitness();

        foreach (NeuralNetwork netBest in bestNN)
            netBest.ClearFitness();
    }

    public void SortNetworks()
    {
        logManager.Message("SortNetworks(): STARTED", this);

        float mv = MeanValue();
        float df = Difference();
        Debug.Log("M:" + mv + "  -  D:" + df);

        networks.Sort();
        Debug.Log((networks[0].fitnessResortSum / (float)resortGeneration) + " - " + (networks[populationSize - 1].fitnessResortSum / (float)resortGeneration));

        networks[populationSize - 1].Save("Assets/Save.txt");

        logManager.Message("SortNetworks(): networks sorted and saved", this);
        
        networks.AddRange(bestNN);
        networks.Sort();
        networks.RemoveRange(0, bestNNCount);
        for (int i = 0; i < bestNNCount; i++)
            bestNN[i] = networks[populationSize - bestNNCount + i].copy(new NeuralNetwork(layers));

        logManager.Message("SortNetworks(): best networks inserted", this);

        // logging
        logManager.WriteNewScore(Time.time - lastGenTime, mv, df);

        int divideCount = 5;
        int tmpDivNNCount = (7 * populationSize / 8) / divideCount;

        // worst copy the best and mutate
        for (int i = 0; i < (tmpDivNNCount); i++)
        {
            networks[i] = networks[populationSize - 1 - i].copy(new NeuralNetwork(layers));
            networks[i].Mutate(MutationChance, MutationStrength);
        }
        logManager.Message("SortNetworks(): worst networks managed", this);

        // average mutate towards the best, while the worse they are, the more they change, and then they simply mutate
        for (int j = 1; j < divideCount; j++)
        {
            for (int i = 0; i < (tmpDivNNCount); i++)
            {
                float coef = (divideCount - j) / divideCount;
                networks[divideCount * j + i].Mutate(MutationChance * coef, MutationStrength, networks[populationSize - 1 - i]);
                networks[divideCount * j + i].Mutate(MutationChance, MutationStrength);
            }
        }

        logManager.Message("SortNetworks(): common networks mutated", this);

        // best mutate
        for (int i = 0; i < populationSize / 8; i++)
            networks[populationSize - 1 - i].Mutate(1 - (1 / (float)divideCount), 1 - (1 / (float)divideCount));

        logManager.Message("SortNetworks(): FINISHED", this);
    }

    float MeanValue()
    {
        float sum = 0f;
        foreach (NeuralNetwork n in networks)
            sum += (n.fitnessResortSum / n.fitnessResortCount);
        return sum / populationSize;
    }

    float DispersionFitness(float ExpV)
    {
        float sum = 0f;
        foreach (NeuralNetwork n in networks)
            sum += n.fitness * n.fitness;
        sum /= populationSize;
        sum -= ExpV * ExpV;
        return sum;
    }

    float Difference()
    {
        List<float> netsTemp = new List<float>() { };
        foreach (NeuralNetwork net in networks)
            netsTemp.Add(net.fitnessResortSum / net.fitnessResortCount);

        netsTemp.Sort();
        return netsTemp[netsTemp.Count - 1] - netsTemp[0];
    }
}
