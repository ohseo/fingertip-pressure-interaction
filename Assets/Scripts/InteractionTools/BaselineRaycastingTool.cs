using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;
using TMPro;

public class BaselineRaycastingTool : MonoBehaviour
{
    [SerializeField] private RayVisualizer _rayVisualizer = null;
    [SerializeField] private float _rayLength = 4.2f; // 6.13f;
    [SerializeField] private ForceLevelManager _forceLevelManager = null;
    [SerializeField] private ExpSceneManager _expSceneManager = null;
    [SerializeField] private Transform _parentTransform;

    public enum BasicRayState
    {
        Pointing = 0,
        Dragging
    }

    public BasicRayState RayInputState
    {
        get
        {
            if(_currIsPinching)
            {
                return BasicRayState.Dragging;
            } else
            {
                return BasicRayState.Pointing;
            }
        }
    }

    private const int NUM_MAX_HITS = 10;
    private const float MIN_RAYCAST_DISTANCE = 0.1f;
    private const float MAX_RAYCAST_DISTANCE = 4.2f;

    public Transform RayTransform { get { return this.transform;} }
    public bool IsRightHandedTool { get; set; }
    public OVRHand _hand;
    private ForceStateModule _forceStateModule = new ForceStateModule();
    public TextMeshProUGUI _text;
    private bool _currIsPinching = false;
    private bool _prevIsPinching = false;

    private List<TargetSphere> _prevTargetsHit = new List<TargetSphere>();
    private RaycastHit[] _raycastHits = new RaycastHit[NUM_MAX_HITS];
    protected TargetSphere _grabbedObj = null;
    protected TargetSphere _prevGrabbedObj = null;
    protected Vector3 _grabbedObjPosOff;
    
    // Start is called before the first frame update
    void Start()
    {
        if(_parentTransform == null)
        {
            _parentTransform = gameObject.transform;
        }
        IsRightHandedTool = true;   // TODO: Connect to tool creator later
        _text = GameObject.Find("Canvas/InteractionToolState").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_expSceneManager._isTimeout)
        {
            ForceEnd();
            return;   
        }

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

        _prevIsPinching = _currIsPinching;
        _currIsPinching = _forceStateModule.IsPinching;

        if(_grabbedObj == null)
        {
            FindTargetSphere();
        }

        CheckForGrabOrRelease(_prevIsPinching, _currIsPinching);

        _rayVisualizer.SetRayState(RayInputState);
        if(_currIsPinching)
        {
            _rayVisualizer.SetRayLength(_rayLength);
        } else
        {
            _rayVisualizer.SetRayLength(MAX_RAYCAST_DISTANCE);
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

    protected void CheckForGrabOrRelease(bool prevIsPinching, bool currIsPinching)
    {
        if (!prevIsPinching && currIsPinching)
        {
            GrabBegin();
        } else if (prevIsPinching && !currIsPinching)
        {
            GrabEnd();
        }

    }

    protected void GrabBegin()
    {
        _grabbedObj = FindTargetSphere();
        if(_grabbedObj != null && _grabbedObj.IsStartingSphere)
        {
            _expSceneManager.StartTrial();
            _grabbedObj.GrabEnd();
            // _grabbedObj.transform.parent = null;
            _grabbedObj = null;
            _prevTargetsHit.Clear();
        } else if(_grabbedObj != null && _grabbedObj.IsExpTarget)
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
            // _grabbedObj.transform.parent = null;
            if(_grabbedObj.IsInGoal)
            {
                _expSceneManager.EndTrial();
            }
            _grabbedObj = null;
            _prevTargetsHit.Clear();
        }
        _prevGrabbedObj = _grabbedObj;
    }

    private void ForceEnd()
    {
        if(_grabbedObj != null)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
        }
        _expSceneManager.EndTrial();
        _prevTargetsHit.Clear();
    }
}
