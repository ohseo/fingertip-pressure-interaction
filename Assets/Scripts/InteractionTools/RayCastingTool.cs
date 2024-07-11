using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OculusSampleFramework
{
    public class RayCastingTool : MonoBehaviour
    {
        [SerializeField] private RayVisualizer _rayVisualizer = null;
        [SerializeField] private float _rayMaxDistance = 4.0f;

        [SerializeField] private ForceLevelManager _forceLevelManager = null;

        public Transform RayTransform { get { return this.transform;} }
        public bool IsRightHandedTool { get; set; }
        public Vector3 InteractionPosition {get; protected set;}
        public OVRHand _hand;
        private ForceStateModule _forceStateModule = new ForceStateModule();
        public TextMeshProUGUI _text;
        private Vector3 prevPointingPosition, prevPointingForward, prevResultPosition, prevResultForward;
        private Vector3 preciseRefPosition, preciseRefForward;
        private bool _currIsHolding = false;
        private bool _prevIsHolding = false;
        private bool _currIsPreciseMode = false;
        private bool _prevIsPreciseMode = false;
        private const float CD_GAIN = 0.07f;
        private Vector3 _targetOffsetOnRay;
        private float _raycastHitDistance;

        //from Interactable Tools

        private const int NUM_COLLIDERS_TO_TEST = 3;
        private const int NUM_MAX_HITS = 10;
        private const float MIN_RAYCAST_DISTANCE = 0.3f;
        private const float POINTER_COLLIDER_RADIUS = 0.01f;
        private TargetSphere _currTargetHit = null;
        private Collider[] _collidersOverlapped = new Collider[NUM_COLLIDERS_TO_TEST];
        private List<TargetSphere> _currIntersectingSpheres = new List<TargetSphere>();
        private RaycastHit[] _raycastHits = new RaycastHit[NUM_MAX_HITS];
        private TargetSphere _currTarget = null;

        //from Grabbers

        protected Vector3 _anchorOffsetPosition;
        protected Quaternion _anchorOffsetRotation;
        protected TargetSphere _grabbedObj = null;
        protected TargetSphere _prevGrabbedObj = null;
        protected Vector3 _grabbedObjPosOff;
        protected Quaternion _grabbedObjRotOff;
        protected Vector3 _lastPos;
        protected Quaternion _lastRot;

        [SerializeField]
        protected Transform _parentTransform;

        void Awake()
        {
            _anchorOffsetPosition = transform.localPosition;
            _anchorOffsetRotation = transform.localRotation;

            OVRCameraRig rig = transform.GetComponentInParent<OVRCameraRig>();
            if (rig != null)
            {
                rig.UpdatedAnchors += (r) => {OnUpdatedAnchors();};
            }

            _forceLevelManager.udpManager.UDPReceiveHandler.AddListener(OnUDPReceive);
        }

        // Start is called before the first frame update
        void Start()
        {
            _lastPos = transform.position;
            _lastRot = transform.rotation;
            if(_parentTransform == null)
            {
                _parentTransform = gameObject.transform;
            }
            IsRightHandedTool = true;   // TODO: Connect to tool creator later
            _rayVisualizer._rayCastingTool = this;
            _text = GameObject.Find("Canvas/InteractionToolState").GetComponent<TextMeshProUGUI>();
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

            var prevPosition = InteractionPosition;
			var currPosition = transform.position;
			InteractionPosition = currPosition;

            _forceStateModule.UpdateState(hand, _forceLevelManager.forceLevel);
            _text.text = _forceStateModule.currentForceState.ToString();

            _prevIsPreciseMode = _currIsPreciseMode;
            _currIsPreciseMode = _forceStateModule.IsInPreciseMode;
            UpdateCastedRay(_prevIsPreciseMode, _currIsPreciseMode);
        }

        void OnUpdatedAnchors()
        {
            _prevIsHolding = _currIsHolding;
            _currIsHolding = _forceStateModule.IsHolding;
            CheckForGrabOrRelease(_prevIsHolding, _currIsHolding);
        }

        public void UpdateCastedRay(bool prevIsPreciseMode, bool currIsPreciseMode)
        {
            if (prevPointingForward == null)
            {
                if(_forceStateModule.IsPinching && !prevIsPreciseMode && currIsPreciseMode)
                {
                    prevPointingForward = transform.forward;
                    prevResultForward = transform.forward;
                } 
            } else
            {
                if(_forceStateModule.IsPinching && currIsPreciseMode)
                {
                    var newForward = prevResultForward - (transform.forward - prevPointingForward) * CD_GAIN;
                    prevPointingForward = transform.forward;
                    prevResultForward = newForward;
                    transform.forward = newForward;
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

                var distanceToTarget = (currTarget.transform.position - rayOrigin).magnitude;
                
                if (targetHit == null || distanceToTarget < minHitDistance)
                {
                    targetHit = currTarget;
                    minHitDistance = distanceToTarget;
                }
            }

            _raycastHitDistance = minHitDistance;
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

        protected void GrabBegin()
        {
            _grabbedObj = FindTargetSphere();
            if(_grabbedObj != null)
            {
                _grabbedObj.GrabBegin();
                _grabbedObj.transform.parent = transform;
                _grabbedObjPosOff = _grabbedObj.transform.localPosition;
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
            _forceStateModule.IsWaiting = false;
        }
    }
}