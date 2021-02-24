using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BowlingBall : MonoBehaviour
{
    Rigidbody rb;
    List<Vector3> positionBuffer = new List<Vector3>();
    int bufferSize = 5;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        positionBuffer.Add(transform.position);
        if (positionBuffer.Count > bufferSize)
        {
            positionBuffer.RemoveAt(0);
        }
    }

    public void Release(ManipulationEventData eventData)
    {
        Vector3 dir = transform.position - positionBuffer[0];
        StartCoroutine(ReleaseRoutine(dir));
        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        GetComponent<LineRenderer>().SetPosition(1, transform.position + dir);
    }

    IEnumerator ReleaseRoutine(Vector3 dir)
    {
        yield return null;
        rb.isKinematic = false;
        rb.velocity = dir / Time.deltaTime / bufferSize;
    }

    public void Bounce()
    {
        rb.velocity = Vector3.up * 10;
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(RiseRoutine());
    }

    IEnumerator RiseRoutine()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos;
        endPos.y = 0;
        for(float t = 0; t < 1; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, 1 - ((t-1)*(t-1)));
            yield return null;
        }
        transform.position = endPos;
    }
}
