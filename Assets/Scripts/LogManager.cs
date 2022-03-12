using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    List<float> bestScoreList = new List<float>() { };
    List<float> meanValList = new List<float>() { };
    List<float> dispersionList = new List<float>() { };

    string filesPostfix = "_2";
    string fileName = "learning_";

    string bestPostfix = "_best";
    string meanPostfix = "_mean";
    string disPostfix = "_disp";

    string fullPath;

    int saveStep = 10;

    // Start is called before the first frame update
    void Start()
    {
        var currentDate = System.DateTime.Now;
        fullPath = "Assets/Progress/" + fileName + currentDate.Day.ToString() + filesPostfix;

        File.Create(fullPath + bestPostfix + ".txt").Close();
        File.Create(fullPath + meanPostfix + ".txt").Close();
        File.Create(fullPath + disPostfix + ".txt").Close();

        Debug.Log("LOG MANAGER CREATED");
    }

    public void WriteNewScore(float newBestScore, float meanValue, float newDispersion)
    {
        bestScoreList.Add(newBestScore);
        meanValList.Add(meanValue);
        dispersionList.Add(newDispersion);

        float count = bestScoreList.Count;
        if (count >= saveStep)
        {
            StreamWriter writerBest = new StreamWriter(fullPath + bestPostfix + ".txt", true);
            StreamWriter writerMean = new StreamWriter(fullPath + meanPostfix + ".txt", true);
            StreamWriter writerDis = new StreamWriter(fullPath + disPostfix + ".txt", true);
            for (int i = 0; i < count; i++)
            {
                writerBest.WriteLine(System.Math.Round(bestScoreList[i], 2));
                writerMean.WriteLine(System.Math.Round(meanValList[i], 2));
                writerDis.WriteLine(System.Math.Round(dispersionList[i], 2));
            }
            writerBest.Close();
            writerMean.Close();
            writerDis.Close();

            bestScoreList.Clear();
            meanValList.Clear();
            dispersionList.Clear();

            Debug.Log("LOG MANAGER WRITED");
        }
    }
}
