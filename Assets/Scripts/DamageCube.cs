using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCube : MonoBehaviour
{
    Vector3 vel = new Vector3(0f, 0f, 0f);
    float wayLenght = 0f;
    bool isLaunched = false;
    public float maxWayLength = 9f;

    public Vector3 posOffset;
    public bool disA = false;

    Color startColor;

    private void Start()
    {
        gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        startColor = Color.red;
    }

    public void SetStartPosition(Vector3 point)
    {
        transform.position = point + posOffset;
        wayLenght = 0f;
        isLaunched = true;
    }

    public void SetVelocity(Vector3 velocity)
    {
        vel = velocity;
    }

    public void ChangeColor(bool isHighlight)
    {
        MeshRenderer goChild = gameObject.GetComponentInChildren<MeshRenderer>();
        if (isHighlight)
        {
            goChild.material.color = Color.white;
        }
        else
        {
            goChild.material.color = startColor;
        }
    }

    public void Deactivate()
    {
        vel = new Vector3(0f, 0f, 0f);
        transform.position += Vector3.down;
        //transform.position = posOffset + new Vector3(0f, -5f, 0f);
        transform.GetComponentInParent<DamageCubeSpawner>().SetReady(this);
        //gameObject.SetActive(false);
        isLaunched = false;

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (disA && isLaunched)
        {
            Deactivate();
            disA = false;
            
        }
        if (isLaunched)
        {
            Vector3 delta_ = vel * Time.deltaTime;
            transform.position += delta_;
            wayLenght += delta_.magnitude;
            if (wayLenght - maxWayLength >= 0)
            {
                disA = true;
            }
        }
    }
}
