using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusSampleFramework
{
    public class RayVisualizer : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer = null;
        [SerializeField] private float _rayLength = 4.2f;

        public RayCastingTool _rayCastingTool { get; set; }

        private const int NUM_MAX_PRIMARY_HITS = 10;
        protected List<TargetSphere> _currIntersectingSpheres = new List<TargetSphere>();
        private RaycastHit[] _primaryHits = new RaycastHit[NUM_MAX_PRIMARY_HITS];

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            var myPosition = _rayCastingTool.RayTransform.position;
			var myForward = _rayCastingTool.RayTransform.forward;
            var ray = new Ray(myPosition, myForward);
            _lineRenderer.SetPosition(0, ray.origin);
            _lineRenderer.SetPosition(1, ray.origin + ray.direction * _rayLength);
        }
    }
}