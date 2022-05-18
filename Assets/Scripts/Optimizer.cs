using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Optimizer : MonoBehaviour
{
    public Manager manager;

    public bool isEnable        = false;
    public int summaryNumber    = 2;

    [SerializeField]
    [ContextMenuItem("EstimateHours", "EstimatedHoursFromRightClick")]
    private double estimatedHours;

    [Header("Parameters")]
    public int generationNumber;
    public int[] populationSize;

    public int[] nnMemorySize;
    public int[] nnRaysCount;

    public float[] mutationChance;
    public float[] initMutationStrength;

    private void EstimatedHoursFromRightClick()
    {
        estimatedHours = generationNumber * 0.005
            * populationSize.Length * nnMemorySize.Length * nnRaysCount.Length
            * mutationChance.Length * initMutationStrength.Length;
    }

    public List<Dictionary<string, float>> results = new List<Dictionary<string, float>>();

    private List<Parameters> parametersList = new List<Parameters>() { };
    private int curIteration = 0;


    void Start()
    {
        if (isEnable)
        {
            SetParametersList();
            StartWithParameters();
        }
    }

    void SetParametersList()
    {
        parametersList.Clear();
        foreach (int popSize in populationSize)
            foreach (int memSize in nnMemorySize)
                foreach (int rayCount in nnRaysCount)
                    foreach (float mutChance in mutationChance)
                        foreach (float initMutStrength in initMutationStrength)
                            parametersList.Add(new Parameters(generationNumber,
                                                                popSize,
                                                                memSize,
                                                                rayCount,
                                                                mutChance,
                                                                initMutStrength));
    }

    void StartWithParameters()
    {
        Parameters parameters = parametersList[curIteration];

        results.Add(new Dictionary<string, float>());
        results[curIteration].Add("Iter", curIteration);
        results[curIteration].Add("GenNum", generationNumber);
        results[curIteration].Add("PopSize", parameters.populationSize);
        results[curIteration].Add("MemSize", parameters.memorySize);
        results[curIteration].Add("RaysCount", parameters.raysCount);
        results[curIteration].Add("MutChance", parameters.mutationChance);
        results[curIteration].Add("MutStrength", parameters.initialMutationStrength);

        manager.StartWithParameters(parameters);
    }

    public void EndWithParameters(float weightedValue)
    {
        results[curIteration].Add("Score", weightedValue);
        manager.logManager.WriteOptimizerData(results[curIteration]);

        curIteration++;
        if (parametersList.Count > curIteration)
            StartWithParameters();
        else
            SortAndWriteResults();
    }

    void SortAndWriteResults()
    {
        List<ScoreByIteration> scoreByIter = new List<ScoreByIteration>() { };
        foreach (Dictionary<string, float> res in results)
            scoreByIter.Add(new ScoreByIteration((int)res["Iter"], res["Score"]));

        scoreByIter.Sort();

        int sumNum = Mathf.Min(scoreByIter.Count, summaryNumber);
        for (int i = 0; i < sumNum; i++)
            manager.logManager.WriteOptimizerData(results[scoreByIter[i].iteration], SummaryType.WORST);

        // divided because of worst-best alternation
        int scoreByIterCount = scoreByIter.Count;
        for (int i = 0; i < sumNum; i++)
            manager.logManager.WriteOptimizerData(results[scoreByIter[scoreByIterCount - sumNum + i].iteration], SummaryType.BEST);
    }
}

public class Parameters
{
    public int generationNumber;
    public int populationSize;
    public int memorySize;
    public int raysCount;
    public float mutationChance;
    public float initialMutationStrength;

    public Parameters(int genNum, int popSize, int memSize, int rCount, float mutChance, float iMutStrength)
    {
        generationNumber = genNum;
        populationSize = popSize;
        memorySize = memSize;
        raysCount = rCount;
        mutationChance = mutChance;
        initialMutationStrength = iMutStrength;
    }
}

class ScoreByIteration : IComparable<ScoreByIteration>
{
    public int iteration;
    public float score;

    public ScoreByIteration(int iterationRef, float scoreRef)
    {
        iteration = iterationRef;
        score = scoreRef;
    }

    public int CompareTo(ScoreByIteration other)
    {
        if (other == null) return 1;

        if (score > other.score)
            return 1;
        else if (score < other.score)
            return -1;
        else
            return 0;
    }
}

public enum SummaryType
{
    UNDEFINED,
    WORST,
    BEST
}
