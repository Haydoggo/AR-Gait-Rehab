using UnityEngine;
using UnityEngine.Events;

public class Waypoint : MonoBehaviour
{

    public float approachDist = 1f;
    public Transform arrow;
    public UnityEvent onApproached = new UnityEvent();
    void Update()
    {
        transform.GetChild(0).Rotate(Vector3.up * Time.deltaTime * 20);
        Vector3 d = Camera.main.transform.position - transform.position;
        if (new Vector2(d.x, d.z).magnitude <= approachDist)
        {
            onApproached.Invoke();
            Destroy(gameObject);
        }
        Vector3 lookTarget = transform.position;
        lookTarget.y = Mathf.Lerp(Camera.main.transform.position.y, transform.position.y, 0.5f);
        arrow.transform.rotation = Quaternion.LookRotation(lookTarget - Camera.main.transform.position);
        arrow.transform.LookAt(lookTarget);
    }
}
