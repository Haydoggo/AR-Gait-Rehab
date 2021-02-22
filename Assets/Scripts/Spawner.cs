using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;
    public float spawnFrequency = 1f;
    public float velocity = 0f;

    private float lastSpawnTime = 0f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastSpawnTime >= 1f / spawnFrequency)
        {
            GameObject obj = Instantiate(spawnObject);
            obj.transform.position = transform.position;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = velocity * transform.forward;
            }
            lastSpawnTime = Time.time;
        }
    }
}
