using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public LogManager logManager;

    public float timeframe;
    public int populationSize;//creates population size
    public GameObject levelOrigin;//holds bot prefab

    public int[] layers = new int[4] { 14, 32, 16, 5 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<Agent> agents;
    private List<Level> levels;
    public int levelsLeft = 0;
    bool isLevelLoading = false;

    int generationCount = 0;
    float lastGenTime = 0f;

    public float NNReactTime = 0.1f;

    



    // spawn parameters
    int toPlayerSpawnIndex = 2;
    int activeCubesAmount = 3;
    int curSpawnIter = 0;
    float spawnDelay = 0f;
    float spawnDelayTimer = 0f;

    public float cubePassTime = 3f;


    void Start()// Start is called before the first frame update
    {
        if (populationSize % 2 != 0)
            populationSize = 10;//if population size is not even, sets it to fifty

        InitNetworks();

        spawnDelay = 3f / (float)activeCubesAmount;
    }

    private void FixedUpdate()
    {
        if (levelsLeft == 0)
        {
            CreateBots();
            for (int i = 0; i < populationSize; i++)
            {
                levels[i].agent.brain = networks[i];
                levels[i].Reload();
            }
            levelsLeft = populationSize;
            Debug.Log("generation: " + generationCount + "  last time:  " + (Time.time - lastGenTime) + "\n");
            lastGenTime = Time.time;
            generationCount++;
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

    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            //net.Load("Assets/Save.txt");//on start load the network save
            networks.Add(net);
        }
    }

    IEnumerator UploadLevels()
    {
        isLevelLoading = true;
        levels = new List<Level>();
        agents = new List<Agent>();

        int lines = Mathf.RoundToInt(Mathf.Pow(populationSize, 0.5f));
        print(lines + "  asdasdsad");
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

        

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < populationSize; i++)
        {
            levels[i].agent.brain = networks[i];//deploys network to each learner
            levels[i].Reload();

        }
        levelsLeft = populationSize;
        isLevelLoading = false;
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;//sets gamespeed, which will increase to speed up training
        if (levels != null)
        {
            SortNetworks();//this sorts networks and mutates them
        }
        else
        {
            if (!isLevelLoading)
                StartCoroutine(UploadLevels());
        }   
    }

    public void SortNetworks()
    {
        networks.Sort();
        Debug.Log(networks[0].fitness + " - " + networks[populationSize - 1].fitness);
        networks[populationSize - 1].Save("Assets/Save.txt");//saves networks weights and biases to file, to preserve network performance

        float evf = ExpectedValueFitness();
        float df = DispersionFitness(evf);
        Debug.Log("M:" + evf + "  -  D:" + df);

        int divideCount = 5;
        int tmpDivNNCount = (7 * populationSize / 8) / divideCount;

        // запись
        logManager.WriteNewScore(Time.time - lastGenTime, evf, df);

        // худшие копируют лучших и мутируют
        for (int i = 0; i < (tmpDivNNCount); i++)
        {
            networks[i] = networks[populationSize - 1 - i].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)((1 / divideCount) / MutationChance), (int)(MutationStrength / divideCount));
        }

        // все средние мутируют в сторону лучших, при этом чем они хуже - тем сильнее мен€ютс€
        for (int j = 1; j < divideCount; j++)
        {
            for (int i = 0; i < (tmpDivNNCount); i++)
            {
                networks[divideCount * j + i].Mutate((int)(((divideCount - j - 1)/divideCount) / MutationChance), 
                    (int)(((divideCount - j - 1) / divideCount) * MutationStrength), networks[populationSize - 1 - i]);
            }
        }

        float tempMutChance = MutationChance;
        if (df < 1f)
        {
            tempMutChance = Mathf.Pow(tempMutChance, 0.0001f);
        }
        // лучшие мутируют
        for (int i = 0; i < populationSize / 8; i++)
        {
            networks[populationSize - 1 - i].Mutate((int)((1 / divideCount) / tempMutChance), (int)(tempMutChance / divideCount));
        }

    }

    float ExpectedValueFitness()
    {
        float sum = 0f;
        foreach (NeuralNetwork n in networks)
            sum += n.fitness;
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
}
