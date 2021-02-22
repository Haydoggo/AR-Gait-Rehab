using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expiry : MonoBehaviour
{
    public float lifetime = 10f;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
