using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusSampleFramework
{
    public class KeyStateModule
    {
        private const float PINCH_STRENGTH_THRESHOLD = 1.0f;
        private const float FORCE_TIME_WINDOW = 200;

        private enum KeyState
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
            HardDownDragging,   //Entered HardDown mode from NoneDown
            HardStay
        }

        private KeyState _currKeyState;

        public string currentKeyState
        {
            get
            {
                return _currKeyState.ToString();
            }
        }

        public bool NotPinching
        {
            get
            {
                return _currKeyState == KeyState.PinchOpen;
            }
        }

        public bool IsSoftDown
        {
            get
            {
                return _currKeyState == KeyState.SoftDown;
            }
        }

        public bool IsTouchDown
        {
            get
            {
                return _currKeyState == KeyState.NoneDown;
            }
        }

        public bool IsHardDown
        {
            get
            {
                return _currKeyState == KeyState.HardDown;
            }
        }

        public bool IsHardDownDragging
        {
            get
            {
                return _currKeyState == KeyState.HardDownDragging;
            }
        }

        public bool IsTouching
        {
            get
            {
                return _currKeyState == KeyState.NoneStay;
            }
        }

        public bool IsDragging
        {
            get
            {
                return _currKeyState == KeyState.NoneDown
                    || _currKeyState == KeyState.HardDown
                    || _currKeyState == KeyState.NoneStay
                    || _currKeyState == KeyState.HardStay
                    || _currKeyState == KeyState.HardDownDragging;
            }
        }

        public KeyStateModule()
        {
            _currKeyState = KeyState.PinchOpen;
        }

        public void UpdateState(OVRHand hand, Interactable currFocusedInteractable)
        {
            float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            bool isPinching = Mathf.Abs(PINCH_STRENGTH_THRESHOLD - pinchStrength) < Mathf.Epsilon;
            var oldKeyState = _currKeyState;
            
            // 1: 3-Moderate
            // 2: 7-Very Strong

            switch (oldKeyState)
            {
                case KeyState.PinchOpen: //Pinch Open
                case KeyState.PinchUp:  //Pinch Open
                    if (isPinching)
                    {   
                        _currKeyState = KeyState.PinchDown;
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchOpen;
                    }
                    break;
                case KeyState.PinchDown:    //Pinch Closed
                    if (isPinching)
                    {
                        _currKeyState = KeyState.PinchStay;
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
                case KeyState.PinchStay:    //Pinch Closed
                    if (isPinching)
                    {   
                        if (Input.GetKey(KeyCode.Alpha1))
                        {
                            _currKeyState = KeyState.SoftDown;
                        } else if (!Input.anyKey)
                        {
                            _currKeyState = KeyState.NoneDown;
                        }
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
                case KeyState.NoneDown:    //0-None
                    if (isPinching)
                    {
                        if (Input.GetKey(KeyCode.Alpha2))
                        {
                            _currKeyState = KeyState.HardDownDragging;
                        } else if (Input.GetKey(KeyCode.Alpha1))
                        {
                            _currKeyState = KeyState.SoftDown;
                        } else
                        {
                            _currKeyState = KeyState.NoneStay;
                        }
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
                case KeyState.NoneStay:    //0-None
                    if (isPinching)
                    {
                        if (Input.GetKey(KeyCode.Alpha2))
                        {
                            _currKeyState = KeyState.HardDownDragging;
                        } else
                        {
                            _currKeyState = KeyState.NoneStay;
                        }
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
                case KeyState.SoftDown:    //3-Moderate
                case KeyState.SoftStay:
                    if (isPinching)
                    {
                        if (Input.GetKey(KeyCode.Alpha2))
                        {
                            _currKeyState = KeyState.HardDown;
                        } else
                        {
                            _currKeyState = KeyState.SoftStay;
                        }
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
                case KeyState.HardDown:    //7-Very Strong
                case KeyState.HardDownDragging:
                case KeyState.HardStay:
                    if (isPinching)
                    {
                        if (!Input.anyKey)
                        {
                            _currKeyState = KeyState.NoneDown;
                        } else
                        {
                            _currKeyState = KeyState.HardStay;
                        }
                    }
                    else
                    {
                        _currKeyState = KeyState.PinchUp;
                    }
                    break;
            }
        }
    }
}