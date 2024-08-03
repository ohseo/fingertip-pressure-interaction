using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using OculusSampleFramework;


public class RayModifyingTool : RaycastingTool
{
    public new enum RayState
    {
        CoarsePointing = 0,
        CoarseDragging,
        PrecisePointing,
        PreciseDragging
    }

    public new RayState RayInputState
    {
        get
        {
            if(_currIsPinching && _currIsHolding && !_currIsPreciseMode)
            {
                return RayState.CoarseDragging;
            } else if(_currIsPinching && !_currIsHolding && _currIsPreciseMode)
            {
                return RayState.PrecisePointing;
            } else if(_currIsPinching && _currIsHolding && _currIsPreciseMode)
            {
                return RayState.PreciseDragging;
            }
            return RayState.CoarsePointing;
        }
    }

    private const float CD_GAIN = 0.1f;
    public int _raycastingMode = 4;
    private Vector3 prevPointingPosition, prevPointingForward, prevResultPosition, prevResultForward;
    private bool _currIsHolding = false;
    private bool _prevIsHolding = false;
    private bool _currIsPreciseMode = false;
    private bool _prevIsPreciseMode = false;
    private bool _refPointSaved = false;

    private Action<bool, bool> updateCastedRayDelegate;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetRayMode(_raycastingMode);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(_expSceneManager._isTimeout)
        {
            ForceEnd();
            return;   
        }

        _prevIsPreciseMode = _currIsPreciseMode;
        _currIsPreciseMode = _forceStateModule.IsInPreciseMode;

        updateCastedRayDelegate?.Invoke(_prevIsPreciseMode, _currIsPreciseMode);
        
        if(_grabbedObj == null)
        {
            FindTargetSphere();
        }

        // if(!ForceRelease(_prevIsPreciseMode, _currIsPreciseMode))
        if(!ForceReleaseByPinch(_prevIsPinching, _currIsPinching))
        {
            _prevIsHolding = _currIsHolding;
            _currIsHolding = _forceStateModule.IsHolding;
            // CheckForGrabOrRelease(_prevIsHolding, _currIsHolding);
            CheckForSelection(_prevIsHolding, _currIsHolding);
        }
        _rayVisualizer.SetRayState(RayInputState);
        if(_currIsHolding)
        {
            _rayVisualizer.SetRayLength(_rayLength);
        } else
        {
            _rayVisualizer.SetRayLength(MAX_RAYCAST_DISTANCE);
        }
    }

    public void SetRayMode(int mode)
    {
        switch(mode)
        {
            case 2:
                updateCastedRayDelegate = CDGainRay;
                break;
            case 3:
                // updateCastedRayDelegate = ForceCtrlRay;
                updateCastedRayDelegate = CDGainForwardRay;
                break;
            case 4:
                updateCastedRayDelegate = ForceCtrlRay;
                // updateCastedRayDelegate = ForceCtrlRayAnchored;
                break;
            // case 5:
            //     updateCastedRayDelegate = CDGainRayAnchored;
            //     break;
            // case 6:
            //     updateCastedRayDelegate = CDGainForwardRay;
            //     break;
            default:
                updateCastedRayDelegate = null;
                break;
        }
    }

    private void CDGainRay(bool prevIsPreciseMode, bool currIsPreciseMode)
    {

        if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
        {
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultPosition = transform.position;
            prevResultForward = transform.forward;
            // _refPointSaved = true;
        }
        else if(_forceStateModule.IsPinching && currIsPreciseMode)
        {
            var newPosition = (pointer.position - prevPointingPosition) * CD_GAIN + prevResultPosition;
            var newForward = (pointer.forward - prevPointingForward) * CD_GAIN + prevResultForward;
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultPosition = newPosition;
            prevResultForward = newForward;
            transform.position = newPosition;
            transform.forward = newForward;
        } else if(_forceStateModule.IsPinching && prevIsPreciseMode && !currIsPreciseMode) // prevents "jump" on release
        {
            var newPosition = (pointer.position - prevPointingPosition) * CD_GAIN + prevResultPosition;
            var newForward = (pointer.forward - prevPointingForward) * CD_GAIN + prevResultForward;
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultPosition = newPosition;
            prevResultForward = newForward;
            transform.position = newPosition;
            transform.forward = newForward;
        }

    }

    private void CDGainRayAnchored(bool prevIsPreciseMode, bool currIsPreciseMode)
    {
        if(!_refPointSaved)
        {
            if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
            {
                prevPointingPosition = transform.position;
                prevPointingForward = transform.forward;
                prevResultPosition = transform.position;
                prevResultForward = transform.forward;
                _refPointSaved = true;
            }
        } else
        {
            if(_forceStateModule.IsPinching && currIsPreciseMode)
            {
                var newPosition = (transform.position - prevPointingPosition) * CD_GAIN + prevResultPosition;
                var newForward = (transform.forward - prevPointingForward) * CD_GAIN + prevResultForward;
                prevPointingPosition = transform.position;
                prevPointingForward = transform.forward;
                prevResultPosition = newPosition;
                prevResultForward = newForward;
                transform.position = newPosition;
                transform.forward = newForward;
            } else if(_forceStateModule.IsPinching && prevIsPreciseMode && !currIsPreciseMode) // prevents "jump" on release
            {
                var newPosition = (transform.position - prevPointingPosition) * CD_GAIN + prevResultPosition;
                var newForward = (transform.forward - prevPointingForward) * CD_GAIN + prevResultForward;
                prevPointingPosition = transform.position;
                prevPointingForward = transform.forward;
                prevResultPosition = newPosition;
                prevResultForward = newForward;
                transform.position = newPosition;
                transform.forward = newForward;
            } else if (!_forceStateModule.IsPinching)
            {
                _refPointSaved = false;
            } else
            {
                var deltaForward = prevResultForward - prevPointingForward;
                var newForward = transform.forward + deltaForward;
                var deltaPosition = prevResultPosition - prevPointingPosition;
                var newPosition = transform.position + deltaPosition;
                prevPointingForward = transform.forward;
                prevResultForward = newForward;
                prevPointingPosition = transform.position;
                prevResultPosition = newPosition;
                transform.forward = newForward;
                transform.position = newPosition;
            }
        }

    }

    private void CDGainForwardRay(bool prevIsPreciseMode, bool currIsPreciseMode)
    {
        if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
        {
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultPosition = transform.position;
            prevResultForward = transform.forward;
            // _refPointSaved = true;
        }
        else if(_forceStateModule.IsPinching && currIsPreciseMode)
        {
            var newForward = (pointer.forward - prevPointingForward) * CD_GAIN + prevResultForward;
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultForward = newForward;
            transform.forward = newForward;
        } else if(_forceStateModule.IsPinching && prevIsPreciseMode && !currIsPreciseMode) // prevents "jump" on release
        {
            var newForward = (pointer.forward - prevPointingForward) * CD_GAIN + prevResultForward;
            prevPointingPosition = pointer.position;
            prevPointingForward = pointer.forward;
            prevResultForward = newForward;
            transform.forward = newForward;
        }
    }

    private void ForceCtrlRay(bool prevIsPreciseMode, bool currIsPreciseMode)
    {
        // if (prevPointingForward == null)
        if (!_refPointSaved)
        {
            if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
            {
                prevPointingForward = pointer.forward;
                prevResultForward = transform.forward;
                _refPointSaved = true;
            } 
        } else
        {
            if(_forceStateModule.IsPinching && currIsPreciseMode)
            {
                var newForward = prevResultForward - (pointer.forward - prevPointingForward) * CD_GAIN;
                prevPointingForward = pointer.forward;
                prevResultForward = newForward;
                transform.forward = newForward;
            // } else if(!_forceStateModule.IsPinching)
            // {
            //     _refPointSaved = false;
                // var deltaForward = prevResultForward - prevPointingForward;
                // var newForward = transform.forward + deltaForward;
                // prevPointingForward = transform.forward;
                // prevResultForward = newForward;
                // transform.forward = newForward;
            } else if(_forceStateModule.IsPinching && prevIsPreciseMode && !currIsPreciseMode) // prevent "jump" on release
            {
                var newForward = prevResultForward - (pointer.forward - prevPointingForward) * CD_GAIN;
                prevPointingForward = pointer.forward;
                prevResultForward = newForward;
                transform.forward = newForward;
            } else if (_forceStateModule.IsPinching && !prevIsPreciseMode && !currIsPreciseMode)
            {
                _refPointSaved = false;
            } else if (!_forceStateModule.IsPinching)
            {
                _refPointSaved = false;
            }
        }

        // _grabbedObj.transform.localPosition = _grabbedObjPosOff;
    }

    public void ForceCtrlRayAnchored(bool prevIsPreciseMode, bool currIsPreciseMode)
    {
        if (!_refPointSaved)
        {
            if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
            {
                prevPointingForward = transform.forward;
                prevResultForward = transform.forward;

            } else if(_forceStateModule.IsPinching && prevIsPreciseMode && currIsPreciseMode)   //prevents "jump" of grabbed object
            {
                Vector3 worldPos = new Vector3();
                if(_grabbedObj != null)
                {
                    worldPos = _grabbedObj.transform.position;
                }
                var newForward = prevResultForward - (transform.forward - prevPointingForward) * CD_GAIN;
                prevPointingForward = transform.forward;
                prevResultForward = newForward;
                transform.forward = newForward;
                if(_grabbedObj != null)
                {
                    _grabbedObj.transform.position = worldPos;
                }
                _refPointSaved = true;
            }
        } else
        {
            if(_forceStateModule.IsPinching && currIsPreciseMode)
            {
                var newForward = prevResultForward - (transform.forward - prevPointingForward) * CD_GAIN;
                prevPointingForward = transform.forward;
                prevResultForward = newForward;
                transform.forward = newForward;
            } else if(!_forceStateModule.IsPinching)
            {
                _refPointSaved = false;
            } else
            {
                var deltaForward = prevResultForward - prevPointingForward;
                var newForward = transform.forward + deltaForward;
                prevPointingForward = transform.forward;
                prevResultForward = newForward;
                transform.forward = newForward;
            }
        }
    }

    protected override void CheckForGrabOrRelease(bool prevIsHolding, bool currIsHolding)
    {
        if (!prevIsHolding && currIsHolding)
        {
            GrabBegin();
        } else if (prevIsHolding && !currIsHolding)
        {
            GrabEnd();
        }

    }

    protected override void CheckForSelection(bool prevIsHolding, bool currIsHolding)
    {
        if (!prevIsHolding && currIsHolding)
        {
            SelectionStart();
        } else if (prevIsHolding && !currIsHolding)
        {
            SelectionEnd();
        }
    }

    protected bool ForceRelease(bool prevIsPreciseMode, bool currIsPreciseMode)
    {
        if(prevIsPreciseMode && !currIsPreciseMode)
        {
            GrabEnd();
            return true;
        }
        return false;
    }
    
    protected bool ForceReleaseByPinch(bool prevIsPinching, bool currIsPinching)
    {
        if(prevIsPinching && !currIsPinching)
        {
            GrabEnd();
            return true;
        }
        return false;
    }
}
