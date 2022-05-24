using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCube : MonoBehaviour
{
    public float maxWayLength = 9f;

    public Vector3 posOffset;
    public bool disA = false;

    private bool isLaunched = false;
    private float wayLenght = 0f;
    private Vector3 vel     = new Vector3(0f, 0f, 0f);
    private Color startColor;

    private void Start()
    {
        gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        startColor = Color.red;
    }

    public void SetStartPosition(Vector3 point)
    {
        transform.position  = point + posOffset;
        wayLenght           = 0f;
        isLaunched          = true;
    }

    public void SetVelocity(Vector3 velocity){ vel = velocity; }

    public void ChangeColor(bool isHighlight)
    {
        MeshRenderer goChild = gameObject.GetComponentInChildren<MeshRenderer>();
        if (isHighlight)
            goChild.material.color = Color.white;
        else
            goChild.material.color = startColor;
    }

    public void Deactivate()
    {
        vel = new Vector3(0f, 0f, 0f);
        transform.position += Vector3.down;
        transform.GetComponentInParent<DamageCubeSpawner>().SetReady(this);
        isLaunched = false;
    }

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
                disA = true;
        }
    }
}
