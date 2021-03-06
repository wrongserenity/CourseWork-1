using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;

    public float fitness            = 0f;
    public float fitnessResortSum   = 0f;
    public int fitnessResortCount   = 0;

    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
            this.layers[i] = layers[i];

        InitNeurons();
        InitBiases();
        InitWeights();
    }

    public void ClearFitness(bool isWithResortData)
    {
        fitness = 0f;
        if (isWithResortData)
        {
            fitnessResortSum = 0f;
            fitnessResortCount = 0;
        }
    }

    public void AddFitnessToResort()
    {
        fitnessResortSum += fitness;
        fitnessResortCount++;
    }

    public void AddFitnessToResort(float value)
    {
        fitnessResortSum += value;
        fitnessResortCount++;
    }

    private void InitNeurons() // initialize empty storage
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
            neuronsList.Add(new float[layers[i]]);

        neurons = neuronsList.ToArray();
    }

    private void InitBiases() // initialize biases with random values
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
                bias[j] = UnityEngine.Random.Range(-0.5f, 0.5f);

            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    private void InitWeights() //initializes weights with random values
    {
        List<float[][]> weightsList = new List<float[][]>();
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);

                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)//feed forward, inputs >==> outputs.
    {
        for (int i = 0; i < inputs.Length; i++)
            neurons[0][i] = inputs[i];

        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                    value += weights[i - 1][j][k] * neurons[i - 1][k];

                neurons[i][j] = activate(value + biases[i][j]);
            }
        }
        return neurons[neurons.Length - 1];
    }

    public float activate(float value)
    {
        return (float)Math.Tanh(value);
    }

    public void Mutate(float chance, float val) //random mutation
    {
        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                if (UnityEngine.Random.Range(0f, 1) <= chance)
                    biases[i][j] += UnityEngine.Random.Range(-val, val);

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    if (UnityEngine.Random.Range(0f, 1) <= chance)
                        weights[i][j][k] += UnityEngine.Random.Range(-val, val);
    }

    public void Mutate(float chance, float val, NeuralNetwork target) //mutate toward target NeuralNetwork
    {
        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                if (UnityEngine.Random.Range(0f, 1) <= chance)
                    biases[i][j] += UnityEngine.Random.Range(0, val) 
                                        * (target.biases[i][j] - biases[i][j]);

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    if (UnityEngine.Random.Range(0f, 1) <= chance)
                        weights[i][j][k] += UnityEngine.Random.Range(0, val) 
                                                * (target.weights[i][j][k] - weights[i][j][k]);
    }

    public int CompareTo(NeuralNetwork other) //comparing For NeuralNetworks performance.
    {
        if (other == null) return 1;

        if (fitnessResortSum > other.fitnessResortSum)
            return 1;
        else if (fitnessResortSum < other.fitnessResortSum)
            return -1;
        else
            return 0;
    }

    public NeuralNetwork copy(NeuralNetwork nn) //For creatinga deep copy, to ensure arrays are serialzed.
    {
        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                nn.biases[i][j] = biases[i][j];

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    nn.weights[i][j][k] = weights[i][j][k];

        return nn;
    }

    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
            ListLines[i] = tr.ReadLine();

        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
            for (int j = 0; j < biases[i].Length; j++)
                writer.WriteLine(biases[i][j]);

        for (int i = 0; i < weights.Length; i++)
            for (int j = 0; j < weights[i].Length; j++)
                for (int k = 0; k < weights[i][j].Length; k++)
                    writer.WriteLine(weights[i][j][k]);

        writer.Close();
    }

    public float GetCosDistanceWith(NeuralNetwork other)
    {
        // check architecture
        for (int i = 0; i < layers.Length; i++)
            if (layers[i] != other.layers[i])
                return 0;

        float biasSum = 0f;
        float biasOwnSquared = 0f;
        float biasOtherSquared = 0f; 
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biasSum += biases[i][j] * other.biases[i][j];
                biasOwnSquared += biases[i][j] * biases[i][j];
                biasOtherSquared += other.biases[i][j] * other.biases[i][j];
            }
        }

        float weightSum = 0f;
        float weightOwnSquared = 0f;
        float weightOtherSquared = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weightSum += weights[i][j][k] * other.weights[i][j][k];
                    weightOwnSquared += weights[i][j][k] * weights[i][j][k];
                    weightOtherSquared += other.weights[i][j][k] * other.weights[i][j][k];
                }
            }
        }

        float biasCosDist   = biasSum / (Mathf.Sqrt(biasOwnSquared) * Mathf.Sqrt(biasOtherSquared));
        float weightCosDist = weightSum / (Mathf.Sqrt(weightOwnSquared) * Mathf.Sqrt(weightOtherSquared));

        return (biasCosDist + weightCosDist) / 2;
    }
}
