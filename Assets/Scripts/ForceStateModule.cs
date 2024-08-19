using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class ForceStateModule
{
    private const float PINCH_STRENGTH_THRESHOLD = 1.0f;

    private enum ForceState
    {
        PinchOpen = 0,
        PinchDown,
        PinchStay,
        PinchUp,
        NoneDown,
        NoneStay,
        SoftDown,
        SoftStay,
        HardDown,
        HardStay
    }
    
    private ForceState _currForceState;
    private ForceState _prevForceState;
    private int _waitingCount;
    private const int WAITING_THRESHOLD = 20;
    private int _taskNum = 1;

    public string currentForceState
    {
        get
        {
            return _currForceState.ToString();
        }
    }

    public bool IsHolding
    {
        get
        {
            return _currForceState == ForceState.NoneDown
                || _currForceState == ForceState.NoneStay
                || _currForceState == ForceState.HardDown
                || _currForceState == ForceState.HardStay;
        }
    }

    public bool IsInPreciseMode
    {
        get
        {
            return _currForceState == ForceState.SoftDown
                || _currForceState == ForceState.SoftStay
                || _currForceState == ForceState.HardDown
                || _currForceState == ForceState.HardStay;
        }
    }

    public bool IsPreciseDragging
    {
        get
        {
            return _currForceState == ForceState.HardDown
                || _currForceState == ForceState.HardStay;
        }
    }

    // for 3d cursor
    public bool IsInCursorMode
    {
        get
        {
            return _currForceState == ForceState.SoftDown
                || _currForceState == ForceState.SoftStay
                || _currForceState == ForceState.HardDown
                || _currForceState == ForceState.HardStay;
        }
    }

    public bool IsCursorHolding
    {
        get
        {
            return _currForceState == ForceState.HardDown
                || _currForceState == ForceState.HardStay;
        }
    }

    public bool IsRayHolding
    {
        get
        {
            return _currForceState == ForceState.NoneDown
                || _currForceState == ForceState.NoneStay;
        }
    }

    // end for 3d cursor

    public bool IsWaiting
    {
        get
        {
            return _currForceState == ForceState.PinchDown
                || _currForceState == ForceState.PinchStay;
        }
    }

    public bool IsPinching
    {
        get
        {
            return _currForceState != ForceState.PinchOpen;
        }
    }

    public ForceStateModule()
    {
        _currForceState = ForceState.PinchOpen;
        _prevForceState = ForceState.PinchOpen;
        _waitingCount = 0;
    }

    public void UpdateState(OVRHand hand, string forceLevel) // Interactable
    {
        // if(forceLevel == "uncertain") Debug.Log("uncertain");
        float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool isPinching = Mathf.Abs(PINCH_STRENGTH_THRESHOLD - pinchStrength) < Mathf.Epsilon;
        _prevForceState = _currForceState;

        switch(_prevForceState)
        {
            case ForceState.PinchOpen:  //Pinch Open
            case ForceState.PinchUp:    //Pinch Open
                if (isPinching)
                {
                    _currForceState = ForceState.PinchDown;
                }
                else
                {
                    _currForceState = ForceState.PinchOpen;
                }
                break;
            case ForceState.PinchDown:  //Pinch Closed
                if (isPinching)
                {
                    _currForceState = ForceState.PinchStay;
                    _waitingCount = 0;;
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }
                break;
            case ForceState.PinchStay:  //Pinch Stay
                if (isPinching)
                {
                    _waitingCount++;
                    if (_waitingCount > WAITING_THRESHOLD)
                    {
                        if (forceLevel == "1")
                        {
                            if(_taskNum != 2)
                            {
                                _currForceState = ForceState.SoftDown;
                            }
                        } else if (forceLevel == "2")
                        {
                            _currForceState = ForceState.HardDown;
                        } else if (forceLevel == "0")
                        {
                            if(_taskNum != 1)
                            {
                                _currForceState = ForceState.NoneDown;
                            }
                        }
                    }
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }
                break;
            case ForceState.NoneDown:   //0-None
                if (isPinching)
                {
                    if (forceLevel == "2")
                    {
                        _currForceState = ForceState.HardDown;
                    // } else if (forceLevel == "1")
                    // {
                    //     _currForceState = ForceState.SoftDown;
                    } else if (forceLevel == "0")
                    {
                        _currForceState = ForceState.NoneStay;
                    }
                                            
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }
                break;
            case ForceState.NoneStay:   //0-None
                if (isPinching)
                {
                    if(forceLevel == "2")
                    {
                        _currForceState = ForceState.HardDown;
                    // } else if (forceLevel == "1")
                    // {
                    //     _currForceState = ForceState.SoftDown;
                    } else
                    {
                        _currForceState = ForceState.NoneStay;
                    }
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }    
                break;
            case ForceState.SoftDown:   //3-Moderate
            case ForceState.SoftStay:
                if (isPinching)
                {
                    if (forceLevel == "2")
                    {
                        _currForceState = ForceState.HardDown;
                    } else
                    {
                        _currForceState = ForceState.SoftStay;
                    }
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }
                break;
            case ForceState.HardDown:   //7-Very Strong
            case ForceState.HardStay:
                if (isPinching)
                {
                    // if (forceLevel == "0")
                    // {
                        // _currForceState = ForceState.NoneDown;
                    // } else if (forceLevel == "1")
                    // {
                        // _currForceState = ForceState.SoftDown;
                        // _currForceState = ForceState.NoneDown; //advantage entering coarse dragging state
                    // } else
                    // {
                        _currForceState = ForceState.HardStay;
                    // }
                }
                else
                {
                    _currForceState = ForceState.PinchUp;
                }
                break;
        }
    }

    public void SetTaskNum(int num)
    {
        _taskNum = num;
    }
}