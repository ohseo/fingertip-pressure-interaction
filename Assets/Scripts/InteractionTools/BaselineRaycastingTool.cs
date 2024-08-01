using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using TMPro;

public class BaselineRaycastingTool : RaycastingTool
{

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(_grabbedObj == null)
        {
            FindTargetSphere();
        }

        CheckForGrabOrRelease(_prevIsPinching, _currIsPinching);

        _rayVisualizer.SetRayState(RayInputState);
        if(_currIsPinching)
        {
            _rayVisualizer.SetRayLength(_rayLength);
        } else
        {
            _rayVisualizer.SetRayLength(MAX_RAYCAST_DISTANCE);
        }
    }
}
