using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [Header("General")]
    public bool isPlayMode = false;

    public LogManager logManager;
    public GameObject mainCamera;

    public int populationSize;
    public GameObject levelOrigin;

    [Range(0.1f, 50f)] 
    public float gameSpeed  = 1f;
    public int levelsLeft   = 0;


    [Header("Neural Network")]
    public int fromSaveNumber = 0;

    const int nnCurIntPosInputs = 2;
    const int nnMoveOutputs     = 5;
    public int nnMemorySize     = 4;
    public int[] hiddenLayers   = new int[2] { 32, 16 };

    public float nnReactTime        = 0.1f;
    public int nnRaysCount          = 16;
    public float nnRaycastLength    = 8f;

    [Space]
    public float nnPunishmentCooldown       = 0.3f;
    public float nnMovePunishment           = 0.1f;
    public float nnSuicidePunishment        = 10f;
    public float nnNearDamageCubePunishment = 0.1f;

    private int[] layers = new int[4] { 22, 32, 16, 9 };


    [Header("Genetic Algorithm")]
    [Range(0.0001f, 1f)] 
    public float mutationChance     = 0.01f;

    [Range(0f, 1f)] 
    public float mutationStrength   = 0.5f;
    public int reweigntGeneration   = 3;
    public float reweightStrength   = 0.99f;

    [Space]
    public int resortGeneration     = 3;
    public int parallelComputing    = 3;

    public int bestNNCount          = 10;

    private List<NeuralNetwork> baseNetworks;
    private List<List<Level>> levels;
    private List<List<NeuralNetwork>> networks;
    private List<NeuralNetwork> bestNN;

    private bool isLevelLoading = false;
    private bool isLaunched = false;

    private int generationCount = 0;
    private float lastGenTime   = 0f;


    [Header("Gameplay")]
    public int toPlayerSpawnIndex   = 2;
    public int activeCubesAmount    = 3;
    public float cubePassTime       = 3f;

    private int curSpawnIter        = 0;
    private float spawnDelay        = 0f;
    private float spawnDelayTimer   = 0f;


    [Header("Optimizer")]
    public Optimizer optimizer;

    private List<float> meanHistoryByGen = new List<float>();
    private int optGeneration = -1;


    void Start()
    {
        if (optimizer == null || !optimizer.isEnable)
            LaunchManager(); // otherwise Optimizer controls manager launching
    }

    void LaunchManager()
    {
        if (!isPlayMode)
        {
            layers[0] = nnRaysCount + nnMemorySize + nnCurIntPosInputs;
            layers[layers.Length - 1] = nnMemorySize + nnMoveOutputs;
            if (layers.Length != hiddenLayers.Length + 2)
                Debug.Log("ERROR: Not correct hidden layers number!");
            else
                for (int i = 0; i < hiddenLayers.Length; i++)
                    layers[i + 1] = hiddenLayers[i];

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
        isLaunched = true;
    }

    public void StartWithParameters(Parameters parameters)
    {
        meanHistoryByGen.Clear();
        generationCount = 0;

        if (levels != null)
            foreach (List<Level> levelLayer in levels)
                foreach (Level level in levelLayer)
                    Destroy(level.gameObject);
        levels = null;
        levelsLeft = 0;

        populationSize      = parameters.populationSize;
        nnMemorySize        = parameters.memorySize;
        nnRaysCount         = parameters.raysCount;
        mutationChance      = parameters.mutationChance;
        mutationStrength    = parameters.mutationChance;

        optGeneration       = parameters.generationNumber;

        LaunchManager();
    }

    void EndWithParameters()
    {
        isLaunched = false;
        WriteNewScore();

        float sum = 0f;
        for (int i = 0; i < optGeneration - 1; i++)
            sum += meanHistoryByGen[i] * ((float)i / (float)optGeneration);

        optimizer.EndWithParameters(sum);
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
            return;

        if (levels == null || isLevelLoading)
        {
            if (!isLevelLoading)
                StartCoroutine(UploadLevels());
            return;
        }

        if (levelsLeft == 0)
        {
            if (optGeneration > 0 && generationCount == optGeneration)
                EndWithParameters();
            else
                TryStartNewGeneration();
        }

        if (spawnDelayTimer < 0.0001f)
        {
            spawnDelayTimer = spawnDelay;
            SpawnerSystem();
        }
        else
            spawnDelayTimer -= Time.deltaTime;
    }

    void TryStartNewGeneration()
    {
        logManager.Message("TryStartNewGeneration(): STARTED", this);
        
        Time.timeScale = gameSpeed;
        Debug.Log("generation: " + generationCount + "  last time:  " + (Time.time - lastGenTime) + "\n");

        if (!isPlayMode)
        {
            if (generationCount % reweigntGeneration == 0)
                mutationStrength *= reweightStrength;

            UpdateBots();
            foreach (List<Level> levelLayer in levels)
                for (int i = 0; i < populationSize; i++)
                    levelLayer[i].Reload();
            levelsLeft = populationSize * parallelComputing;

            generationCount++;
        }
        lastGenTime = Time.time;

        logManager.Message("TryStartNewGeneration(): FINISHED", this);
    }

    void SpawnerSystem()
    {
        //logManager.Message("Spawner System(): STARTED", this);

        foreach(List<Level> levelLayer in levels)
        {
            int xOrZ                = Random.Range(0, 2);
            int forwardOrBackward   = Random.Range(0, 2);
            int posInt              = Random.Range(0, 8);

            for (int j = 0; j < populationSize; j++)
            {
                if (curSpawnIter % toPlayerSpawnIndex == 0)
                {
                    if (levelLayer[j].agent.isAlive)
                        levelLayer[j].damageCubeSpawner.SpawnDamageCubeIn();

                    curSpawnIter = 0;
                }
                else if (levelLayer[j].agent.isAlive)
                    levelLayer[j].damageCubeSpawner.SpawnDamageCubeIn(xOrZ, forwardOrBackward, posInt);
            }
        }
        curSpawnIter++;

        //logManager.Message("Spawner System(): FINISHED", this);
    }

    public void InitNetworks()
    {
        logManager.Message("InitNetworks(): STARTED", this);

        baseNetworks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            if (i < fromSaveNumber)
                net.Load("Assets/Save.txt");//on start load the network save
            baseNetworks.Add(net);
        }

        bestNN = new List<NeuralNetwork>();
        for (int i = 0; i < bestNNCount; i++)
            bestNN.Add(baseNetworks[populationSize - bestNNCount + i].copy(new NeuralNetwork(layers)));

        logManager.Message("InitNetworks(): FINISHED", this);
    }

    void SetNeuralNetworksToLevelsLayers()
    {
        logManager.Message("SetNeuralNetworksToLevelsLayers(): STARTED", this);

        NeuralNetwork net;
        List<NeuralNetwork> nets;
        networks = new List<List<NeuralNetwork>>();

        foreach (List<Level> levelLayer in levels)
        {
            nets = new List<NeuralNetwork>();
            for (int i = 0; i < populationSize; i++)
            {
                if (!isPlayMode)
                {
                    net = new NeuralNetwork(layers);
                    baseNetworks[i].copy(net);
                    
                    nets.Add(net);
                    levelLayer[i].agent.brain = nets[nets.Count - 1];
                }
            }            
            networks.Add(nets);
        }
        logManager.Message("SetNeuralNetworksToLevelsLayers(): FINISHED", this);
    }

    IEnumerator UploadLevels()
    {
        logManager.Message("UploadLevels(): STARTED", this);

        isLevelLoading = true;

        levels = new List<List<Level>>();
        List<Level> levelsLayer;
        for (int i = 0; i < parallelComputing; i++)
        {
            levelsLayer = new List<Level>();

            int lines = Mathf.RoundToInt(Mathf.Pow(populationSize, 0.5f));
            int curLine = 0;

            int spawned = 0;
            while (spawned < populationSize)
            {
                Level level = (Instantiate(levelOrigin, new Vector3((float)curLine * 12f, 30f * (float)i, (float)(spawned % lines) * 12f), new Quaternion(0, 0, 1, 0))).GetComponent<Level>();//create botes
                level.manager = this;
                levelsLayer.Add(level);

                spawned++;
                if (spawned % lines == 0)
                    curLine++;
            }
            levels.Add(levelsLayer);
        }
        logManager.Message("UploadLevels(): levels inited", this);

        yield return new WaitForSecondsRealtime(0.5f);

        SetNeuralNetworksToLevelsLayers();
        isLevelLoading = false;

        logManager.Message("UploadLevels(): FINISHED", this);
    }

    public void UpdateBots()
    {
        logManager.Message("CreateBots(): STARTED", this);

        AddNetworksFitness();
        if (generationCount % resortGeneration == 0)
        {
            if (generationCount != 0)
            {
                WriteNewScore();
                SortNetworks();
            }
                
            ClearBaseNetworksFitness();
            SetNeuralNetworksToLevelsLayers();
        }
        else
        {
            WriteNewScore(false);
            foreach (List<NeuralNetwork> nets in networks)
                foreach (NeuralNetwork net in nets)
                    net.ClearFitness(false);
        }
        
        logManager.Message("CreateBots(): FINISHED", this);
    }

    void WriteNewScore(bool isWithResort=true)
    {
        float evf;
        float df;

        if (isWithResort)
        {
            evf = MeanValue();
            df  = Difference();
        }
        else
        {
            evf = MeanValue(false);
            df  = Difference(false);
        }

        logManager.WriteNewScore(Time.time - lastGenTime, evf, df);
        meanHistoryByGen.Add(evf);
    }

    void AddNetworksFitness()
    {
        logManager.Message("AddNetworksFitness(): STARTED", this);

        for(int i = 0; i < populationSize; i++)
            for (int j = 0; j < parallelComputing; j++)
                baseNetworks[i].AddFitnessToResort(networks[j][i].fitness);

        logManager.Message("AddNetworkFitness(): FINISHED", this);
    }

    void ClearBaseNetworksFitness()
    {
        logManager.Message("ClearBaseNetworksFitness(): STARTED", this);

        foreach (NeuralNetwork network in baseNetworks)
            network.ClearFitness(true);

        foreach (NeuralNetwork netBest in bestNN)
            netBest.ClearFitness(true);

        logManager.Message("ClearBaseNetworksFitness(): FINISHED", this);
    }

    public void SortNetworks()
    {
        logManager.Message("SortNetworks(): STARTED", this);

        float mv = MeanValue();
        float df = Difference();
        Debug.Log("M:" + mv + "  -  D:" + df);

        baseNetworks.Sort();
        Debug.Log((baseNetworks[0].fitnessResortSum / (float)(resortGeneration*parallelComputing)) + " - " + (baseNetworks[populationSize - 1].fitnessResortSum / (float)(resortGeneration * parallelComputing)));

        baseNetworks[populationSize - 1].Save("Assets/Save.txt");

        logManager.Message("SortNetworks(): networks sorted and saved", this);
        
        // insert best networks
        baseNetworks.AddRange(bestNN);
        baseNetworks.Sort();
        baseNetworks.RemoveRange(0, bestNNCount);
        for (int i = 0; i < bestNNCount; i++)
            bestNN[i] = baseNetworks[populationSize - bestNNCount + i].copy(new NeuralNetwork(layers));

        // check for retraining
        if (baseNetworks[0].GetCosDistanceWith(baseNetworks[populationSize - 1]) < 0.1)
        {
            float chance = Mathf.Min(1f, 10 * mutationChance);
            float strength = Mathf.Min(1f, 10 * mutationStrength);
            for (int i = 0; i < populationSize / 2; i++)
                baseNetworks[i].Mutate(chance, strength);
        }

        logManager.Message("SortNetworks(): best networks inserted", this);

        int divideCount = 5;
        int tmpDivNNCount = (7 * populationSize / 8) / divideCount;

        // worst copy the best and mutate
        for (int i = 0; i < (tmpDivNNCount); i++)
        {
            baseNetworks[i] = baseNetworks[populationSize - 1 - i].copy(new NeuralNetwork(layers));
            baseNetworks[i].Mutate(mutationChance, mutationStrength);
        }
        logManager.Message("SortNetworks(): worst networks managed", this);

        // average mutate towards the best, while the worse they are, the more they change, and then they simply mutate
        for (int j = 1; j < divideCount; j++)
        {
            for (int i = 0; i < (tmpDivNNCount); i++)
            {
                float coef = (divideCount - j) / divideCount;
                baseNetworks[divideCount * j + i].Mutate(mutationChance * coef, mutationStrength, baseNetworks[populationSize - 1 - i]);
                baseNetworks[divideCount * j + i].Mutate(mutationChance, mutationStrength);
            }
        }

        logManager.Message("SortNetworks(): common networks mutated", this);

        // best mutate
        for (int i = 0; i < populationSize / 8; i++)
            baseNetworks[populationSize - 1 - i].Mutate(1 - (1 / (float)divideCount), 1 - (1 / (float)divideCount));

        logManager.Message("SortNetworks(): FINISHED", this);
    }

    float MeanValue(bool isWithResort=true)
    {
        float sum = 0f;
        if (isWithResort)
            foreach (NeuralNetwork n in baseNetworks)
                sum += (n.fitnessResortSum / (float)n.fitnessResortCount);
        else
        {
            foreach (List<NeuralNetwork> nnLayer in networks)
                foreach (NeuralNetwork n in nnLayer)
                    sum += n.fitness;
            sum /= networks.Count;
        }

        return sum / populationSize;
    }

    float Difference(bool isWithResort=true)
    {
        List<float> netsTemp = new List<float>() { };
        if (isWithResort)
            foreach (NeuralNetwork net in baseNetworks)
                netsTemp.Add(net.fitnessResortSum / net.fitnessResortCount);
        else
            foreach (List<NeuralNetwork> nnLayer in networks)
                foreach (NeuralNetwork n in nnLayer)
                    netsTemp.Add(n.fitness);

        netsTemp.Sort();
        return netsTemp[netsTemp.Count - 1] - netsTemp[0];
    }
}
