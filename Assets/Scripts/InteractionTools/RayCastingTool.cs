using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace OculusSampleFramework
{
    public class RayCastingTool : MonoBehaviour
    {
        [SerializeField] private RayVisualizer _rayVisualizer = null;
        [SerializeField] private float _rayLength = 4.2f; // 6.13f;
        [SerializeField] private ForceLevelManager _forceLevelManager = null;
        [SerializeField] private Transform _parentTransform;

        public enum RayState
        {
            CoarsePointing = 0,
            CoarseDragging,
            PrecisePointing,
            PreciseDragging
        }

        public RayState RayInputState
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

        private const float CD_GAIN = 0.089f;
        private const int NUM_MAX_HITS = 10;
        private const float MIN_RAYCAST_DISTANCE = 0.1f;
        private const float MAX_RAYCAST_DISTANCE = 4.2f;

        public Transform RayTransform { get { return this.transform;} }
        public bool IsRightHandedTool { get; set; }
        public int _raycastingMode = 4;
        public OVRHand _hand;
        private ForceStateModule _forceStateModule = new ForceStateModule();
        public TextMeshProUGUI _text;
        private Vector3 prevPointingPosition, prevPointingForward, prevResultPosition, prevResultForward;
        private bool _currIsHolding = false;
        private bool _prevIsHolding = false;
        private bool _currIsPreciseMode = false;
        private bool _prevIsPreciseMode = false;
        private bool _currIsPinching = false;
        private bool _prevIsPinching = false;
        private bool _refPointSaved = false;

        private List<TargetSphere> _prevTargetsHit = new List<TargetSphere>();
        private RaycastHit[] _raycastHits = new RaycastHit[NUM_MAX_HITS];
        protected TargetSphere _grabbedObj = null;
        protected TargetSphere _prevGrabbedObj = null;
        protected Vector3 _grabbedObjPosOff;

        private Action<bool, bool> updateCastedRayDelegate;



        void Awake()
        {
            // OVRCameraRig rig = transform.GetComponentInParent<OVRCameraRig>();
            // if (rig != null)
            // {
            //     rig.UpdatedAnchors += (r) => {OnUpdatedAnchors();};
            // }
            _forceLevelManager.udpManager.UDPReceiveHandler.AddListener(OnUDPReceive);
        }

        // Start is called before the first frame update
        void Start()
        {
            if(_parentTransform == null)
            {
                _parentTransform = gameObject.transform;
            }
            IsRightHandedTool = true;   // TODO: Connect to tool creator later
            // _rayVisualizer._rayCastingTool = this;
            _text = GameObject.Find("Canvas/InteractionToolState").GetComponent<TextMeshProUGUI>();
            SetRayMode(_raycastingMode);
        }

        // Update is called once per frame
        void Update()
        {
            if (!HandsManager.Instance || !HandsManager.Instance.IsInitialized())
            {
                return;
            }

            var hand = IsRightHandedTool ? HandsManager.Instance.RightHand : HandsManager.Instance.LeftHand;

            var pointer = _hand.PointerPose;

            transform.position = pointer.position;
            transform.rotation = pointer.rotation;

            _forceStateModule.UpdateState(hand, _forceLevelManager.forceLevel);
            _text.text = _forceStateModule.currentForceState.ToString();

            _prevIsPreciseMode = _currIsPreciseMode;
            _currIsPreciseMode = _forceStateModule.IsInPreciseMode;            
            // UpdateCastedRay(_raycastingMode, _prevIsPreciseMode, _currIsPreciseMode);
            _prevIsPinching = _currIsPinching;
            _currIsPinching = _forceStateModule.IsPinching;

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
                CheckForGrabOrRelease(_prevIsHolding, _currIsHolding);
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
                    updateCastedRayDelegate = ForceCtrlRay;
                    break;
                case 4:
                    updateCastedRayDelegate = ForceCtrlRayAnchored;
                    break;
                case 5:
                    updateCastedRayDelegate = CDGainRayAnchored;
                    break;
                default:
                    updateCastedRayDelegate = null;
                    break;
            }
        }

        public void UpdateCastedRay(int mode, bool prevIsPreciseMode, bool currIsPreciseMode)
        {
            switch(mode)
            {
                case 1:
                    break;
                case 2:
                    CDGainRay(prevIsPreciseMode, currIsPreciseMode);
                    break;
                case 3:
                    ForceCtrlRay(prevIsPreciseMode, currIsPreciseMode);
                    break;
            }
        }

        private void CDGainRay(bool prevIsPreciseMode, bool currIsPreciseMode)
        {

            if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
            {
                prevPointingPosition = transform.position;
                prevPointingForward = transform.forward;
                prevResultPosition = transform.position;
                prevResultForward = transform.forward;
                // _refPointSaved = true;
            }
            else if(_forceStateModule.IsPinching && currIsPreciseMode)
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

        private void ForceCtrlRay(bool prevIsPreciseMode, bool currIsPreciseMode)
        {
            // if (prevPointingForward == null)
            if (!_refPointSaved)
            {
                if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
                {
                    prevPointingForward = transform.forward;
                    prevResultForward = transform.forward;
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
                    var newForward = prevResultForward - (transform.forward - prevPointingForward) * CD_GAIN;
                    prevPointingForward = transform.forward;
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

        private TargetSphere FindTargetSphere()
        {
            var rayOrigin = GetRaycastOrigin(); //avoid collision with hand collider
            var rayDirection = transform.forward;
            TargetSphere targetHit = null;

            int numHits = Physics.RaycastNonAlloc(new Ray(rayOrigin, rayDirection), _raycastHits);
            float minHitDistance = 0.0f;
            for (int i = 0; i < numHits; i++)
            {
                RaycastHit hit = _raycastHits[i];

                //TODO: continue if the object is not a TargetSphere

                TargetSphere currTarget = hit.collider.gameObject.GetComponent<TargetSphere>();

                if (currTarget == null)
                {
                    continue;
                }

                var distanceToTarget = (currTarget.transform.position - rayOrigin).magnitude;
                
                if (targetHit == null || distanceToTarget < minHitDistance)
                {
                    targetHit = currTarget;
                    minHitDistance = distanceToTarget;
                }
            }

            if(targetHit != null)
            {
                _rayLength = (targetHit.transform.position - transform.position).magnitude;
                foreach(TargetSphere sphere in _prevTargetsHit)
                {
                    if(sphere != targetHit)
                    {
                        sphere.Mute();
                    }
                }
                if(!_prevTargetsHit.Contains(targetHit))
                {
                    _prevTargetsHit.Add(targetHit);
                }
                if (!targetHit.IsGrabbed)
                {
                    targetHit.Highlight();
                }
            } else
            {
                _rayLength = MAX_RAYCAST_DISTANCE;
                foreach(TargetSphere sphere in _prevTargetsHit)
                {
                    if (!sphere.IsGrabbed)
                    {
                        sphere.Mute();  
                    }
                }
            }
            

            return targetHit;
        }

        private Vector3 GetRaycastOrigin()
        {
            return transform.position + MIN_RAYCAST_DISTANCE * transform.forward;
        }

        protected void CheckForGrabOrRelease(bool prevIsHolding, bool currIsHolding)
        {
            if (!prevIsHolding && currIsHolding)
            {
                GrabBegin();
            } else if (prevIsHolding && !currIsHolding)
            {
                GrabEnd();
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

        protected void GrabBegin()
        {
            _grabbedObj = FindTargetSphere();
            if(_grabbedObj != null)
            {
                _grabbedObj.GrabBegin();
                _grabbedObj.transform.parent = transform;
                _grabbedObjPosOff = _grabbedObj.transform.localPosition;
                Debug.Log("Position Offset: "+_grabbedObjPosOff.ToString());
            }
            _prevGrabbedObj = _grabbedObj;
        }

        protected void GrabEnd()
        {
            if(_grabbedObj != null)
            {
                _grabbedObj.GrabEnd();
                _grabbedObj.transform.parent = null;
                _grabbedObj = null;
            }
            _prevGrabbedObj = _grabbedObj;
        }

        void OnUDPReceive(string msg)
        {
        }
    }
}