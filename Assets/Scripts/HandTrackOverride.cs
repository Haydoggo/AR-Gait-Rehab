using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandTrackOverride : MonoBehaviour, IMixedRealityHandJointHandler
{
    public Handedness hand;
    public TrackedHandJoint joint;
    void OnEnable()
    {
        CoreServices.InputSystem.RegisterHandler<IMixedRealityHandJointHandler>(this);
    }
    public void OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
    {
        if ((eventData.Handedness & hand) != 0)
        {
            MixedRealityPose jointPose = eventData.InputData[joint];
            transform.position = jointPose.Position;
            transform.rotation = jointPose.Rotation;
        }
    }
}
