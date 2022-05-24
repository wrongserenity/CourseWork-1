using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogManager : MonoBehaviour
{
    public bool isLogScore          = false;
    public bool isLogMessages       = false;
    public bool isLogOptimizer      = false;
    public bool isCloseGameOnLogEnd = false;

    public int maxLogLines = 1001;
    public string filesPostfix = "_1";

    private List<float> bestScoreList   = new List<float>() { };
    private List<float> meanValList     = new List<float>() { };
    private List<float> dispersionList  = new List<float>() { };

    const string FILE_NAME             = "learn_";

    const string BEST_POSTFIX   = "_best";
    const string MEAN_POSTFIX   = "_mean";
    const string DIS_POSTFIX    = "_diff";
    const string OPT_POSTFIX    = "_opt";
    
    const string MSG_POSTFIX    = "_msg";
    const int SAVE_STEP         = 10;
    
    private string msgMemory    = "";
    private string fullPath     = "";
    private int loggedLines     = 0;

    private List<string> optimizerKeyOrder;

    void Start()
    {
        var currentDate = System.DateTime.Now;
        fullPath = "Assets/Progress/" + FILE_NAME + currentDate.Month.ToString() + "_" + currentDate.Day.ToString() + filesPostfix;

        if (isLogScore)
        {
            File.Create(fullPath + BEST_POSTFIX + ".txt").Close();
            File.Create(fullPath + MEAN_POSTFIX + ".txt").Close();
            File.Create(fullPath + DIS_POSTFIX + ".txt").Close();
        }

        if (isLogMessages)
            File.Create(fullPath + MSG_POSTFIX + ".txt").Close();
        
        if (isLogOptimizer)
            File.Create(fullPath + OPT_POSTFIX + ".txt").Close();

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
            if (count >= SAVE_STEP)
            {
                StreamWriter writerBest = new StreamWriter(fullPath + BEST_POSTFIX + ".txt", true);
                StreamWriter writerMean = new StreamWriter(fullPath + MEAN_POSTFIX + ".txt", true);
                StreamWriter writerDis = new StreamWriter(fullPath + DIS_POSTFIX + ".txt", true);
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
                    //Application.Quit();
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
                StreamWriter writerMsg = new StreamWriter(fullPath + MSG_POSTFIX + ".txt", true);
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
            StreamWriter writerOpt = new StreamWriter(fullPath + OPT_POSTFIX + ".txt", true);
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
