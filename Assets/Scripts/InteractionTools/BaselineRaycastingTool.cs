using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using TMPro;

public class BaselineRaycastingTool : RaycastingTool
{
    private RayState _prevRayState;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
        if(_expSceneManager._isTimeout)
        {
            ForceEnd();
            return;   
        }

        if(_grabbedObj == null)
        {
            FindTargetSphere();
        }

        interactionCheckDelegate?.Invoke(_prevIsPinching, _currIsPinching);
        // CheckForGrabOrRelease(_prevIsPinching, _currIsPinching);
        // CheckForSelection(_prevIsPinching, _currIsPinching);

        _rayVisualizer.SetRayState(RayInputState);
        if(_expSceneManager._isInTrial && _expManager._taskNum != 0)
        {
            if(_prevRayState != RayInputState)
            {
                _inputStateChangeTrigger.Invoke(_expSceneManager.GetTrialDuration(), _prevRayState.ToString(), RayInputState.ToString());
                // Debug.Log("Logger: ray state changed: from "+_prevRayState.ToString()+" to "+RayInputState.ToString());
            }
        }
        _prevRayState = RayInputState;
        if(_currIsPinching)
        {
            _rayVisualizer.SetRayLength(_rayLength);
        } else
        {
            _rayVisualizer.SetRayLength(MAX_RAYCAST_DISTANCE);
        }

        _ray = transform;
    }
}
