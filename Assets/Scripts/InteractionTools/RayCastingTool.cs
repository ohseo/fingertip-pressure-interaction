using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OculusSampleFramework;
using UnityEditor;

public class RaycastingTool : MonoBehaviour
{
    [SerializeField] protected RayVisualizer _rayVisualizer = null;
    [SerializeField] protected float _rayLength = 4.2f; // 6.13f;
    [SerializeField] protected ForceLevelManager _forceLevelManager = null;
    [SerializeField] protected ExpSceneManager _expSceneManager = null;
    [SerializeField] protected Transform _parentTransform;

    public enum RayState
    {
        Pointing = 0,
        Dragging
    }

    public RayState RayInputState
    {
        get
        {
            if(_currIsPinching)
            {
                return RayState.Dragging;
            } else
            {
                return RayState.Pointing;
            }
        }
    }

    protected const int NUM_MAX_HITS = 10;
    protected const float MIN_RAYCAST_DISTANCE = 0.1f;
    protected const float MAX_RAYCAST_DISTANCE = 4.2f;

    private Transform pointer;
    public bool IsRightHandedTool { get; set; }

    public OVRHand _hand { get; set; }
    protected ForceStateModule _forceStateModule = new ForceStateModule();
    public TextMeshProUGUI _text;

    protected bool _currIsPinching = false;
    protected bool _prevIsPinching = false;

    protected List<TargetSphere> _prevTargetsHit = new List<TargetSphere>();
    protected RaycastHit[] _raycastHits = new RaycastHit[NUM_MAX_HITS];
    protected TargetSphere _grabbedObj = null;
    protected TargetSphere _prevGrabbedObj = null;
    protected Vector3 _grabbedObjPosOff;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        // if(_parentTransform == null)
        // {
            _parentTransform = gameObject.transform;
        // }
        IsRightHandedTool = true;   // TODO: Connect to tool creator later
        // _rayVisualizer._rayCastingTool = this;
        _text = GameObject.Find("Canvas/InteractionToolState").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!HandsManager.Instance || !HandsManager.Instance.IsInitialized())
        {
            return;
        }

        pointer = _hand.PointerPose;

        transform.position = pointer.position;
        transform.rotation = pointer.rotation;

        _forceStateModule.UpdateState(_hand, _forceLevelManager.forceLevel);
        _text.text = _forceStateModule.currentForceState.ToString();

        _prevIsPinching = _currIsPinching;
        _currIsPinching = _forceStateModule.IsPinching;
    }

    protected TargetSphere FindTargetSphere()
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

    protected Vector3 GetRaycastOrigin()
    {
        return transform.position + MIN_RAYCAST_DISTANCE * transform.forward;
    }

    protected virtual void CheckForGrabOrRelease(bool prevIsPinching, bool currIsPinching)
    {
        if (!prevIsPinching && currIsPinching)
        {
            GrabBegin();
        } else if (prevIsPinching && !currIsPinching)
        {
            GrabEnd();
        }

    }

    protected virtual void CheckForSelection(bool prevIsPinching, bool currIsPinching)
    {
        if (!prevIsPinching && currIsPinching)
        {
            SelectionStart();
        } else if (prevIsPinching && !currIsPinching)
        {
            SelectionEnd();
        }
    }

    protected void SelectionStart()
    {
        _grabbedObj = FindTargetSphere();
        if(_grabbedObj != null && _grabbedObj.IsStartingSphere)
        {
            _expSceneManager.StartTrial();
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
            _prevTargetsHit.Clear();
        } else if(_grabbedObj != null && _grabbedObj.IsExpTarget)
        {
            _grabbedObj.GrabBegin();
        } else if(_grabbedObj != null && !_grabbedObj.IsExpTarget)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
            _prevTargetsHit.Clear();
        }
        _prevGrabbedObj = _grabbedObj;
    }

    protected void SelectionEnd()
    {
        if(_grabbedObj != null && _grabbedObj.IsExpTarget)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
            _expSceneManager.NextTarget();
        }
        _prevTargetsHit.Clear();
        _prevGrabbedObj = _grabbedObj;
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
        } else if(_grabbedObj != null && !_grabbedObj.IsExpTarget)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
            _prevTargetsHit.Clear();
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
        }
        _prevTargetsHit.Clear();
        _prevGrabbedObj = _grabbedObj;
    }

    protected void ForceEnd()
    {
        if(_grabbedObj != null)
        {
            _grabbedObj.GrabEnd();
            _grabbedObj = null;
        }
        _prevTargetsHit.Clear();
        _expSceneManager.EndTrial();
    }

    public void SetForceLevelManager(ForceLevelManager flm)
    {
        _forceLevelManager = flm;
    }

    public void SetExpSceneManager(ExpSceneManager esm)
    {
        _expSceneManager = esm;
    }

    public void SetHand(OVRHand hand)
    {
        _hand = hand;
    }
}
