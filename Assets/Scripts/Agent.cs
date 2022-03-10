using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public Level level;

    Vector2Int intBounds = new Vector2Int(8, 8);

    public Vector2Int curIntPos = new Vector2Int(5, 5);
    Collider hitBox;

    public NeuralNetwork brain;
    public float fitness = 0f;
    float spawnTime = 0f;
    public Vector3 spawnPos = new Vector3(0.5f, 0.5f, 0.5f);

    public bool isAlive = false;

    public int raysCount = 8;
    RaycastHit[] shotRay;
    float raycastLength = 4f;
    Vector3[] raysDir;

    public Material matA;
    public Material matB;

    public bool reloadReq = false;

    int[] moveCounter = new int[5] { 0, 0, 0, 0, 0 };

    float reactTime = 0f;
    float curReactTime = 0f;

    float movePunishment = 0.1f;
    float cooldownPunishment = 0.3f;
    float curPunishmentCooldownTime = 0.0f;
    int punishmentStack = 1;
    float sumMovePunishment = 0f;

    float[] memory = new float[4] { 0f, 0f, 0f, 0f};


    // Start is called before the first frame update
    void Start()
    {
        hitBox = gameObject.GetComponentInChildren<Collider>();
        reactTime = level.manager.NNReactTime;
    }

    void Reload()
    {
        if (!isAlive)
        {
            isAlive = true;
            transform.position = spawnPos;
            curIntPos = new Vector2Int(5, 5);
            spawnTime = Time.time;
            raysDir = new Vector3[raysCount];
            shotRay = new RaycastHit[raysCount];
            SetNeuralNetworkRays();
            gameObject.GetComponentInChildren<MeshRenderer>().material = matA;
        }
    }

    public void Kill()
    {
        if (isAlive)
        {
            brain.fitness = Time.time - spawnTime - sumMovePunishment;
            sumMovePunishment = 0f;
            isAlive = false;
            level.Deactivate();
            gameObject.GetComponentInChildren<MeshRenderer>().material = matB;
            //Debug.Log("[" + moveCounter[0] + ", " + moveCounter[1] + ", " + moveCounter[2] + ", " + moveCounter[3] + ", " + moveCounter[4] + "]");
        }
    }

    void CooldownUpdater(float deltaTime)
    {
        if (curPunishmentCooldownTime > 0)
        {
            curPunishmentCooldownTime -= deltaTime;
        }
        else
        {
            punishmentStack = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (reloadReq)
        {
            Reload();
            reloadReq = false;
        }
        if (isAlive)
        {
            if (brain != null)
            {
                if (curReactTime < 0.0001)
                {
                    UseNeuralNetwork();
                    curReactTime = reactTime;
                }
                else
                {
                    curReactTime -= Time.deltaTime;
                }


                Collider[] cols = Physics.OverlapBox(hitBox.bounds.center, hitBox.bounds.extents, hitBox.transform.rotation);
                foreach (Collider col in cols)
                {
                    if (col.gameObject.transform.parent.gameObject.CompareTag("DamageCube"))
                    {
                        Kill();
                    }
                }
            }
            else
                Debug.Log("no brain in: " + transform.position);
        }

        CooldownUpdater(Time.deltaTime);
    }

    void SetNeuralNetworkRays()
    {
        float degree = 360f / (float)raysCount;
        for (int i = 0; i < raysCount; i++)
        {
            raysDir[i] = Quaternion.AngleAxis(i * degree, Vector3.up) * Vector3.forward;
        }
    }

    void UseNeuralNetwork()
    {
        float[] inputs = new float[raysCount+2+4];
        for (int i = 0; i < raysCount; i++)
        {
            Physics.Raycast(transform.position, raysDir[i], out shotRay[i], raycastLength);
            if (shotRay[i].collider != null)
            {
                Debug.DrawLine(transform.position + Vector3.down * 0.1f, transform.position + raysDir[i] * raycastLength + Vector3.down * 0.1f, Color.black, reactTime); ;
                Debug.DrawLine(transform.position, transform.position + raysDir[i] * shotRay[i].distance, Color.cyan, reactTime);
                inputs[i] = shotRay[i].distance / raycastLength;
            }
            else
            {
                Debug.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + raysDir[i] * raycastLength + Vector3.up * 0.1f, Color.red, level.manager.NNReactTime);
                inputs[i] = -1;
            }
        }
        inputs[raysCount] = (float)curIntPos.x / (float)intBounds.x;
        inputs[raysCount+1] = (float)curIntPos.y / (float)intBounds.x;

        for (int i = 0; i < 4; i++)
        {
            inputs[raysCount + 2 + i] = memory[i];
        }
        //Debug.Log(inputs[0] + " - " + inputs[1] + " - " + inputs[2] + " - " + inputs[3] + " - " + inputs[4] + " - " + inputs[5] + " - " + inputs[6] + " - " + inputs[7] + " - " + inputs[8] + " - " + inputs[9]);

        var output = brain.FeedForward(inputs);

        float valM = -1;
        int iM = -1;
        for (int i =0; i < 5; i++)
        {
            if (output[i] > valM) {
                valM = output[i];
                iM = i;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            memory[i] = output[i + 4];
        }

        

        if (iM >= 0)
        {
            moveCounter[iM]++;
            //Debug.Log("[" + output[0] + ", " + output[1] + ", " + output[2] + ", " + output[3] + "]");
            MoveSignal(Mathf.RoundToInt(iM)-1);
        }


    }

    // 0 - up, 1 - down, 2 - right, 3 - left
    public void MoveSignal(int direction)
    {
        if (direction != -1)
        {
            if (curPunishmentCooldownTime > 0)
            {
                punishmentStack++;
                curPunishmentCooldownTime = cooldownPunishment;
            }
            sumMovePunishment += movePunishment * punishmentStack;
        }
        //Debug.Log("move to : " + direction);
        if (direction == 0)
        {
            if (intBounds.y - curIntPos.y - 1 >= 0)
            {
                curIntPos.y++;
                transform.position += Vector3.forward;
            }
        }
        else if (direction == 1)
        {
            if (curIntPos.y - 2 >= 0)
            {
                curIntPos.y--;
                transform.position -= Vector3.forward;
            }
        }
        else if (direction == 2)
        {
            if (intBounds.x - curIntPos.x - 1 >= 0)
            {
                curIntPos.x++;
                transform.position += Vector3.right;
            }
        }
        else if (direction == 3)
        {
            if (curIntPos.x - 2 >= 0)
            {
                curIntPos.x--;
                transform.position -= Vector3.right;
            }
        }
    }

}
