using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogManager : MonoBehaviour
{
    public bool isLogScore = false;
    public bool isLogMessages = false;
    public bool isLogOptimizer = false;

    public int maxLogLines = 1001;

    public bool isCloseGameOnLogEnd = false;

    int loggedLines = 0;

    List<float> bestScoreList = new List<float>() { };
    List<float> meanValList = new List<float>() { };
    List<float> dispersionList = new List<float>() { };

    public string filesPostfix = "_1";
    string fileName = "learn_";

    string bestPostfix = "_best";
    string meanPostfix = "_mean";
    string disPostfix = "_diff";

    string messagesPostfix = "_msg";
    string msgMemory = "";

    string optimizerPostfix = "_opt";
    List<string> optimizerKeyOrder;

    string fullPath = "";

    int saveStep = 10;

    // Start is called before the first frame update
    void Start()
    {
        var currentDate = System.DateTime.Now;
        fullPath = "Assets/Progress/" + fileName + currentDate.Month.ToString() + "_" + currentDate.Day.ToString() + filesPostfix;

        if (isLogScore)
        {
            File.Create(fullPath + bestPostfix + ".txt").Close();
            File.Create(fullPath + meanPostfix + ".txt").Close();
            File.Create(fullPath + disPostfix + ".txt").Close();
        }

        if (isLogMessages)
            File.Create(fullPath + messagesPostfix + ".txt").Close();
        
        if (isLogOptimizer)
            File.Create(fullPath + optimizerPostfix + ".txt").Close();


        Debug.Log("LOG MANAGER CREATED");

    }

    public void WriteNewScore(float newBestScore, float meanValue, float newDispersion)
    {
        if (isLogScore && loggedLines < maxLogLines)
        {
            loggedLines++;

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

                Message("LOG MANAGER WRITED");

                if (loggedLines == maxLogLines && isCloseGameOnLogEnd)
                {
                    Application.Quit();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
    }

    public void Message(string text, Object owner = null)
    {
        if (isLogMessages)
        {
            string message = msgMemory + System.DateTime.Now.ToString() + " | " + (owner != null ? (owner.ToString() + " : ") : "") + text;
            Debug.Log(message);
            if (fullPath != "")
            {
                StreamWriter writerMsg = new StreamWriter(fullPath + messagesPostfix + ".txt", true);
                writerMsg.WriteLine(message);
                writerMsg.Close();

                msgMemory = "";
            }
            else
                msgMemory = message + "\n";
        }
    }

    public void WriteOptimizerData(Dictionary<string, float> resultDict, SummaryType type=SummaryType.UNDEFINED)
    {
        if (isLogOptimizer)
        {
            StreamWriter writerOpt = new StreamWriter(fullPath + optimizerPostfix + ".txt", true);
            if (optimizerKeyOrder == null)
                optimizerKeyOrder = new List<string>(resultDict.Keys);

            if (type != SummaryType.UNDEFINED)
                writerOpt.WriteLine(type.ToString());

            foreach (string key in optimizerKeyOrder)
                writerOpt.WriteLine(key + ": " + resultDict[key]);
            writerOpt.WriteLine(" ");
            writerOpt.Close();

        }
    }
}
