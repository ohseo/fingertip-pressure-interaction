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

        private Gradient _defaultColorGradient, _coarseDraggingColorGradient,
                    _precisePointingColorGradient, _preciseDraggingColorGradient;

        void Awake()
        {
            _defaultColorGradient = new Gradient();
            _defaultColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.90f, 0.90f, 0.90f), 0.0f),
    	        	  new GradientColorKey(new Color(0.90f, 0.90f, 0.90f), 1.0f) },
			    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            _coarseDraggingColorGradient = new Gradient();
            _coarseDraggingColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.0f, 0.90f, 0.90f), 0.0f),
    	        	  new GradientColorKey(new Color(0.0f, 0.90f, 0.90f), 1.0f) },
			    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            _precisePointingColorGradient = new Gradient();
            _precisePointingColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.90f, 0.0f, 0.90f), 0.0f),
    	        	  new GradientColorKey(new Color(0.90f, 0.20f, 0.90f), 1.0f) },
			    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            _preciseDraggingColorGradient = new Gradient();
            _preciseDraggingColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.90f, 0.0f, 0.90f), 0.0f),
    	        	  new GradientColorKey(new Color(0.90f, 0.0f, 0.90f), 1.0f) },
			    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            // var myPosition = _rayCastingTool.RayTransform.position;
			// var myForward = _rayCastingTool.RayTransform.forward;
            // var ray = new Ray(myPosition, myForward);
            // _lineRenderer.SetPosition(0, ray.origin);
            // _lineRenderer.SetPosition(1, ray.origin + ray.direction * _rayLength);
        }

        public void SetRayState(RayCastingTool.RayState state)
        {
            switch(state)
            {
                case RayCastingTool.RayState.CoarseDragging:
                    _lineRenderer.widthMultiplier = 0.002f;
                    _lineRenderer.colorGradient = _coarseDraggingColorGradient;
                    break;
                case RayCastingTool.RayState.PrecisePointing:
                    _lineRenderer.widthMultiplier = 0.001f;
                    _lineRenderer.colorGradient = _precisePointingColorGradient;
                    break;
                case RayCastingTool.RayState.PreciseDragging:
                    _lineRenderer.widthMultiplier = 0.002f;
                    _lineRenderer.colorGradient = _preciseDraggingColorGradient;
                    break;
                default:
                    _lineRenderer.widthMultiplier = 0.001f;
                    _lineRenderer.colorGradient = _defaultColorGradient;
                    break;
            }
        }

        public void SetRayLength(float length)
        {
            _lineRenderer.SetPosition(1, new Vector3(0, 0, length));
        }
    }
}