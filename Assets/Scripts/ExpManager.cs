using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class ExpManager : MonoBehaviour
{
    public int _participantNum = 0;
    public int _taskNum = 1;
    public int _raycastingMode = 0; //0: Baseline, 1: CDHand, 2: CDDirection, 3: ForceCtrl
    public int _targetDepthCondition = 1; //1: 1.0f, 2: 2.0f
    public int _targetDensityCondition = 1; //1: sparse, 2: dense
    public bool _isRightHanded = true;
    public RaycastingTool _baselineRaycastingTool = null;
    public RayModifyingTool _rayModifyingTool = null;
    public ForceLevelManager _forceLevelManager = null;
    [HideInInspector] public ExpSceneManager _expSceneManager = null;
    private GameObject _expSceneManagerGO;

    [HideInInspector] public OVRHand _hand = null;
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;

    public bool _forceSetAndTrial = false;
    public int _forceSetNum = 0;
    public int _forceTrialNum = 0;

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

        GameObject go = new GameObject("ExpSceneManager");

        if(_taskNum == 1)
        {
            SelectionSceneManager ssm = go.AddComponent<SelectionSceneManager>();
            _expSceneManager = (ExpSceneManager)ssm;
        } else
        {
            CubeSceneManager csm = go.AddComponent<CubeSceneManager>();
            _expSceneManager = (ExpSceneManager)csm;
        }

        _expSceneManager.SetPrefabs(targetSpherePrefab, goalCubePrefab);
        _expSceneManager.SetExpConditions(_targetDepthCondition, _targetDensityCondition);
        if(_forceSetAndTrial)
        {
            _expSceneManager.ForceSetAndTrial(_forceSetAndTrial, _forceSetNum, _forceTrialNum);
        }
        _expSceneManager.Init();
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
