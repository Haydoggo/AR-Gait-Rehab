using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Input;

public class GamepadInputHandler : MonoBehaviour
{
    public UnityEvent OnButtonA = new UnityEvent();
    public UnityEvent OnButtonB = new UnityEvent();
    public UnityEvent OnButtonX = new UnityEvent();
    public UnityEvent OnButtonY = new UnityEvent();

    private void OnEnable()
    {
        OnButtonA.AddListener(()=>print("A"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button0)) OnButtonA.Invoke();
        if (Input.GetKeyDown(KeyCode.Joystick1Button1)) OnButtonB.Invoke();
        if (Input.GetKeyDown(KeyCode.Joystick1Button2)) OnButtonX.Invoke();
        if (Input.GetKeyDown(KeyCode.Joystick1Button3)) OnButtonY.Invoke();
    }
}
