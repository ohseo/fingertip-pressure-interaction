using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class ExpManager : MonoBehaviour
{
    public int _participantNum = 0;
    public int _taskNum = 1;
    public int _raycastingMode = 0;
    public int _targetDepthCondition = 1;
    public int _targetDensityCondition = 1;
    public int _trialNum = 0;
    public bool _isRightHanded = true;
    public RaycastingTool _baselineRaycastingTool = null;
    public RayModifyingTool _rayModifyingTool = null;
    public ForceLevelManager _forceLevelManager = null;
    [HideInInspector] public ExpSceneManager _expSceneManager = null;

    [HideInInspector] public OVRHand _hand = null;
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;

    void Awake()
    {
        if(_raycastingMode == 0)
        {
            StartCoroutine(AttachToolToHand(_baselineRaycastingTool, _isRightHanded));
        } else
        {
            StartCoroutine(AttachToolToHand(_rayModifyingTool, _isRightHanded));
            _rayModifyingTool._raycastingMode = _raycastingMode;
        }

        if(_taskNum == 1)
        {
            SelectionSceneManager ssm = new SelectionSceneManager();
            _expSceneManager = (ExpSceneManager)ssm;
            _expSceneManager.SetPrefabs(targetSpherePrefab, goalCubePrefab);
            _expSceneManager.Init();
        } else
        {
            CubeSceneManager csm = new CubeSceneManager();
            _expSceneManager = (ExpSceneManager)csm;
            _expSceneManager.SetPrefabs(targetSpherePrefab, goalCubePrefab);
            _expSceneManager.Init();
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
        obj.transform.parent = _hand.transform.parent;
        obj.IsRightHandedTool = isRightHanded;
        obj.SetForceLevelManager(_forceLevelManager);
        obj.SetExpManager(this);
        obj.SetExpSceneManager(_expSceneManager);
        obj.SetHand(_hand);
    }
}
