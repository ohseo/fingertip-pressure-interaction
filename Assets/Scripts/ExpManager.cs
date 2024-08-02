using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class ExpManager : MonoBehaviour
{
    public int _participantNum = 0;
    public string _expCondition = "A";
    public int _raycastingMode = 0;
    public int _targetDepthCondition = 1;
    public int _targetSizeCondition = 1;
    public int _trialNum = 0;
    public bool _isRightHanded = true;
    public RaycastingTool _baselineRaycastingTool = null;
    public RaycastingTool _rayModifyingTool = null;
    public ForceLevelManager _forceLevelManager = null;
    public ExpSceneManager _expSceneManager = null;

    [HideInInspector] public OVRHand _hand = null;

    void Awake()
    {
        if(_raycastingMode == 0)
        {
            StartCoroutine(AttachToolToHand(_baselineRaycastingTool, _isRightHanded));
        } else
        {
            StartCoroutine(AttachToolToHand(_rayModifyingTool, _isRightHanded));
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator AttachToolToHand(RaycastingTool tool, bool isRightHanded)
    {
        HandsManager handsManagerObj = null;
        while ((handsManagerObj = HandsManager.Instance) == null || !handsManagerObj.IsInitialized())
        {
            yield return null;
        }

        _hand = _isRightHanded ? HandsManager.Instance.RightHand : HandsManager.Instance.LeftHand;

        RaycastingTool obj = Instantiate(tool);
        obj.name = "RaycastingTool";
        obj.transform.parent = _hand.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.IsRightHandedTool = isRightHanded;
        // obj.GetComponent<RaycastingTool>().IsRightHandedTool = isRightHanded; 
        obj.SetForceLevelManager(_forceLevelManager);
        obj.SetExpSceneManager(_expSceneManager);
        obj.SetHand(_hand);
    }
}
