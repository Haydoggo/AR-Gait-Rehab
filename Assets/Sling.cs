using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class Sling : MonoBehaviour
{
    public Transform connectionPoint1;
    public Transform connectionPoint2;
    public Transform restPosition;
    public Transform fingerTip;
    public Transform cup;

    public GameObject projectilePrefab;
    public Transform slingCup;

    private bool canShoot = true;
    public bool CanShoot
    {
        get
        {
            return canShoot;
        }
        set
        {
            canShoot = value;
            slingCup.gameObject.SetActive(canShoot);
        }
    }

    private PointerHandler cupPointerHandler;
    private void Awake()
    {
        cupPointerHandler = slingCup.GetComponent<PointerHandler>();
        cupPointerHandler.OnPointerDragged.AddListener((MixedRealityPointerEventData eventData) => cup.position = Vector3.Lerp(restPosition.position, fingerTip.position, 0.6f));
        cupPointerHandler.OnPointerUp.AddListener((MixedRealityPointerEventData eventData) => ShootProjectile());
    }

    void LateUpdate()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.SetPosition(0, connectionPoint1.position);
        line.SetPosition(1, cup.position);
        line.SetPosition(2, connectionPoint2.position);
    }

    public void ShootProjectile() 
    {
        if (!CanShoot) return;
        CanShoot = false;
        StartCoroutine(SpringBackRoutine(0.2f));
        GameObject proj = Instantiate(projectilePrefab);
        proj.transform.position = slingCup.position;
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Vector3 vel = restPosition.position - slingCup.position;
        rb.velocity = vel*30;

        PathController pc = FindObjectOfType<PathController>();

        Transform oldTransform = pc.pathLayingTransform;
        pc.pathLayingTransform = proj.transform;
        pc.LayingTrack = true;
        proj.GetComponent<Expiry>().OnDestroy.AddListener(() => pc.pathLayingTransform = oldTransform);
        proj.GetComponent<Expiry>().OnDestroy.AddListener(() => pc.LayingTrack = false);
        proj.GetComponent<Expiry>().OnDestroy.AddListener(() => CanShoot = true);
    }

    IEnumerator SpringBackRoutine(float returnTime)
    {
        Vector3 startPos = slingCup.position;
        for (float t = 0; t < returnTime; t += Time.deltaTime)
        {
            float T = 1 - Mathf.Cos(2 * Mathf.PI * t / returnTime) * (1 - t / returnTime);
            slingCup.position = Vector3.LerpUnclamped(startPos, restPosition.position, T);
            yield return null;
        }
        slingCup.position = restPosition.position;
    }
}
