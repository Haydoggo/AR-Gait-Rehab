using UnityEngine;
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        transform.forward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1,0,1)).normalized;
    }
}
