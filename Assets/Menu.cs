using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class Menu : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(GotoCenter());
    }

    IEnumerator GotoCenter()
    {
        float maxViewDegrees = GetComponent<RadialView>().MaxViewDegrees;
        GetComponent<RadialView>().MaxViewDegrees = 0;
        yield return new WaitForSeconds(0.2f);
        GetComponent<RadialView>().MaxViewDegrees = maxViewDegrees;
    }
}
