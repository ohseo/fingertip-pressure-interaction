using System.Collections;
using System.Collections.Generic;
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

        private const int NUM_COLLIDERS_TO_TEST = 3;
        private const int NUM_MAX_HITS = 10;
        private const float MIN_RAYCAST_DISTANCE = 0.3f;
        private const float POINTER_COLLIDER_RADIUS = 0.01f;
        private TargetSphere _currTargetHit = null;
        private Collider[] _collidersOverlapped = new Collider[NUM_COLLIDERS_TO_TEST];
        private List<TargetSphere> _currIntersectingSpheres = new List<TargetSphere>();
        private RaycastHit[] _raycastHits = new RaycastHit[NUM_MAX_HITS];

        // Start is called before the first frame update
        void Start()
        {
            Assert.
            IsRightHandedTool = true;   // TODO: Connect to tool creator later
            _rayVisualizer._rayCastingTool = this;
        }

        // Update is called once per frame
        void Update()
        {
            var pointer = _hand.PointerPose;

            transform.position = pointer.position;
            transform.rotation = pointer.rotation;

            var prevPosition = InteractionPosition;
			var currPosition = transform.position;
			// Velocity = (currPosition - prevPosition) / Time.deltaTime;
			InteractionPosition = currPosition;
        }

        public List<TargetSphere> GetNextIntersectingTargets()
        {
            //TODO: check if we already have a focused target

            if(_currTargetHit == null)
            {
                _currIntersectingSpheres.Clear();
                _currTargetHit = FindTargetSphere();

                if (_currTargetHit != null)
                {
                    var targetHitPoint = _currTargetHit.transform.position;
                    int numHits = Physics.OverlapSphereNonAlloc(targetHitPoint, POINTER_COLLIDER_RADIUS, _collidersOverlapped);

                    for (int i = 0; i < numHits; i++)
                    {
                        Collider colliderHit = _collidersOverlapped[i];
                        TargetSphere hitTargetSphere = colliderHit.gameObject.GetComponent<TargetSphere>();
                        if (hitTargetSphere == null || hitTargetSphere != _currTargetHit)
                        {
                            continue;
                        }
                        _currIntersectingSpheres.Add(hitTargetSphere);
                    }

                    if (_currIntersectingSpheres.Count == 0)
                    {
                        _currTargetHit = null;
                    }
                }
            }

            return _currIntersectingSpheres;
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

            return targetHit;
        }

        private Vector3 GetRaycastOrigin()
        {
            return transform.position + MIN_RAYCAST_DISTANCE * transform.forward;
        }
    }
}