using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Expiry : MonoBehaviour
{
    public float lifetime = 10f;
    public UnityEvent OnDestroy = new UnityEvent();
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime >= lifetime)
        {
            OnDestroy.Invoke();
            Destroy(gameObject);
        }
    }
}
