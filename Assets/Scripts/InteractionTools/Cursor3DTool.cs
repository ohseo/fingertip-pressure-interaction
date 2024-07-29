using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OculusSampleFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Cursor3DTool : MonoBehaviour
{
    
    [SerializeField] private RayVisualizer _rayVisualizer = null;
    [SerializeField] private float _rayLength = 4.2f;
    [SerializeField] private ForceLevelManager _forceLevelManager = null;
    public enum InputState
    {
        None = 0,
        RayActivated,
        CursorControl,
        CursorDown
    }

    public InputState PointerInputState
    {
        get
        {
            if(_currIsPinching && !_currIsCursorMode)
            {
                return InputState.RayActivated;
            } else if (_currIsPinching && _currIsCursorMode && !_currIsCursorHolding)
            {
                return InputState.CursorControl;
            } else if (_currIsPinching && _currIsCursorMode && _currIsCursorHolding)
            {
                return InputState.CursorDown;
            }
            return InputState.None;
        }
    }

    private const float CD_GAIN = 0.5f;
    private const float HEAD_RAY_OFFSET = -0.1f;
    private const float RAY_ORIGIN_OFFSET = 0.2f;
    private const float CURSOR_SIZE = 0.005f;
    private const int NUM_MAX_HITS = 10;
    public bool IsRightHandedTool { get; set; }
    public OVRHand _hand;
    private ForceStateModule _forceStateModule = new ForceStateModule();
    public TextMeshProUGUI _text;

    private Vector3 _prevCursorPosition, _currCursorPosition, _prevPointerPosition, _currPointerPosition;
    private bool _currIsPinching = false;
    private bool _prevIsPinching = false;
    private bool _currIsCursorMode = false;
    private bool _prevIsCursorMode = false;
    private bool _currIsCursorHolding = false;
    private bool _prevIsCursorHolding = false;
    private bool _prevIsWaiting = false;
    private bool _currIsWaiting = false;

    private LineRenderer _headRenderer;
    private Transform _centerEyeAnchor;
    private GameObject _cursorMarker;
    private TargetSphere _grabbedObj;
    private Collider[] _collidersOverlapped = new Collider[NUM_MAX_HITS];
    private List<TargetSphere> _prevTargetsHit = new List<TargetSphere>();

    void Awake()
    {
        _forceLevelManager.udpManager.UDPReceiveHandler.AddListener(OnUDPReceive);
    }
    // Start is called before the first frame update
    void Start()
    {
        IsRightHandedTool = true;
        // _rayVisualizer._rayCastingTool = this;
        _text = GameObject.Find("Canvas/InteractionToolState").GetComponent<TextMeshProUGUI>();

        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if(cameraRig != null)
        {
            _centerEyeAnchor = cameraRig.centerEyeAnchor;
        } 
        _headRenderer = _centerEyeAnchor.gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer(_headRenderer);

        _cursorMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _cursorMarker.transform.localScale = new Vector3(CURSOR_SIZE, CURSOR_SIZE, CURSOR_SIZE);
        _currCursorPosition = Vector3.forward;
        _cursorMarker.transform.position = _currCursorPosition;
        _cursorMarker.GetComponent<SphereCollider>().isTrigger = true;
        _cursorMarker.AddComponent<Rigidbody>();
        _cursorMarker.GetComponent<Rigidbody>().isKinematic = true;
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
        _prevPointerPosition = _currPointerPosition;
        _currPointerPosition = pointer.position;

        _forceStateModule.UpdateState(hand, _forceLevelManager.forceLevel);
        _text.text = _forceStateModule.currentForceState.ToString();

        _prevIsPinching = _currIsPinching;
        _currIsPinching = _forceStateModule.IsPinching;
        _prevIsCursorMode = _currIsCursorMode;
        _currIsCursorMode = _forceStateModule.IsInPreciseMode;
        _prevIsCursorHolding = _currIsCursorHolding;
        _currIsCursorHolding = _forceStateModule.IsCursorHolding;
        _prevIsWaiting = _currIsWaiting;
        _currIsWaiting = _forceStateModule.IsWaiting;


        if (_currIsPinching && !_currIsCursorMode && !_currIsWaiting)
        {
            if(_prevIsWaiting)
            {
                _headRenderer.enabled = true;
                _rayVisualizer.SetActive();
            }
            Vector3 intersectionPoint;
            if (FindIntersection(out intersectionPoint))
            {
                _prevCursorPosition = _currCursorPosition;
                _currCursorPosition = intersectionPoint;
                _cursorMarker.transform.position = _currCursorPosition;
            }
        } else if (_currIsPinching && _currIsCursorMode && !_currIsWaiting)
        {
            MoveCursor();
            _headRenderer.enabled = false;
            _rayVisualizer.SetInactive();
        } else
        {
            _headRenderer.enabled = false;
            _rayVisualizer.SetInactive();
        }

        if(_grabbedObj == null)
        {
            FindTargetSphere();
        }

        CheckForGrabOrRelease(_prevIsCursorHolding, _currIsCursorHolding);


    }

    private bool FindIntersection(out Vector3 intersection)
    {
        Ray handRay = new Ray(transform.position, transform.forward);
        Ray headRay = new Ray(_centerEyeAnchor.position+new Vector3(0, HEAD_RAY_OFFSET, 0), _centerEyeAnchor.forward);
        // _headRenderer.SetPosition(0, headRay.origin);
        // _headRenderer.SetPosition(1, headRay.origin+headRay.direction);

        intersection = Vector3.zero;

        Vector3 p1 = handRay.origin;
        Vector3 d1 = handRay.direction;
        Vector3 p2 = headRay.origin;
        Vector3 d2 = headRay.direction;

        // Solving equations of the form:
        // p1 + t1 * d1 = p2 + t2 * d2
        // Rearranged to solve for t1 and t2:
        // t1 * d1 - t2 * d2 = p2 - p1

        Vector3 r = p2 - p1;
        Vector3 crossD1D2 = Vector3.Cross(d1, d2);
        float crossD1D2SqrMagnitude = crossD1D2.sqrMagnitude;

        // Check if rays are parallel
        if (crossD1D2SqrMagnitude < Mathf.Epsilon)
        {
            return false;
        }

        float t1 = Vector3.Dot(Vector3.Cross(r, d2), crossD1D2) / crossD1D2SqrMagnitude;
        float t2 = Vector3.Dot(Vector3.Cross(r, d1), crossD1D2) / crossD1D2SqrMagnitude;
        if (t1 > 4 || t2 > 4)
        {
            return false;
        }

        // Calculate the intersection point
        Vector3 pointOnRay1 = p1 + t1 * d1;
        Vector3 pointOnRay2 = p2 + t2 * d2;

        // Check if the points are close enough to be considered intersecting
        if (Vector3.Distance(pointOnRay1, pointOnRay2) < 0.01f)
        {
            intersection = (pointOnRay1 + pointOnRay2) / 2;
            return true;
        }

        return false;
    }

    private void MoveCursor()
    {
        Vector3 newPosition = _currCursorPosition + (_currPointerPosition - _prevPointerPosition) * CD_GAIN;
        _prevCursorPosition = _currCursorPosition;
        _currCursorPosition = newPosition;
        _cursorMarker.transform.position = _currCursorPosition;
    }

    void SetupLineRenderer(LineRenderer renderer)
    {
        renderer.positionCount = 2;
        renderer.startWidth = 0.001f;
        renderer.endWidth = 0.001f;
        renderer.useWorldSpace = false;

        renderer.startColor = Color.blue;
        renderer.endColor = Color.blue;

        renderer.material = new Material(Shader.Find("Sprites/Default"));   

        renderer.SetPosition(0, new Vector3(0, HEAD_RAY_OFFSET, RAY_ORIGIN_OFFSET));
        renderer.SetPosition(1, new Vector3(0, HEAD_RAY_OFFSET, _rayLength));
        renderer.enabled = false;
    }

    private TargetSphere FindTargetSphere()
    {
        TargetSphere targetHit = null;

        int numHits = Physics.OverlapSphereNonAlloc(_cursorMarker.transform.position, CURSOR_SIZE, _collidersOverlapped);
        float minHitDistance = 0.0f;
        
        for (int i = 0; i < numHits; i++)
        {
            Collider collider = _collidersOverlapped[i];
            TargetSphere currTarget = collider.gameObject.GetComponent<TargetSphere>();

            if (currTarget == null)
            {
                continue;
            }

            var distanceToTarget = (currTarget.transform.position - _cursorMarker.transform.position).magnitude;

            if (targetHit == null || distanceToTarget < minHitDistance)
            {
                targetHit = currTarget;
                minHitDistance = distanceToTarget;
            }
        }

        if (targetHit != null)
        {
            foreach(TargetSphere sphere in _prevTargetsHit)
            {
                if (sphere != targetHit)
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

    private void CheckForGrabOrRelease(bool prevIsCursorHolding, bool currIsCursorHolding)
    {
        if (!prevIsCursorHolding && currIsCursorHolding)
        {
            GrabBegin();
        } else if (prevIsCursorHolding && !currIsCursorHolding)
        {
            GrabEnd();
        }
    }


    private void GrabBegin()
    {
        _grabbedObj = FindTargetSphere();
        if(_grabbedObj != null)
        {
            _grabbedObj.GrabBegin();
            _grabbedObj.transform.parent = _cursorMarker.transform;
        }
    }

    private void GrabEnd()
    {
        if(_grabbedObj != null)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj.transform.parent = null;
            _grabbedObj = null;
        }
    }

    void OnUDPReceive(string msg)
    {
    }
}
