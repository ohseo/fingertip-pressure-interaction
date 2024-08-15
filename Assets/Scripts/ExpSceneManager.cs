using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using System;
using Random=UnityEngine.Random;
using TMPro;


public class ExpSceneManager : MonoBehaviour
{
    public int _expCondition = 0;
    protected float _targetDepth = 1.0f;
    protected const float TARGET_SIZE = 0.035f;
    protected Vector3 _numGrid = new Vector3(6, 6, 1);
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;
    protected bool _forceSetAndTrial = false;
    protected int _currentTrial = 1;
    protected int _currentSet = 1;

    protected const float TIME_OUT_THRESHOLD = 20.0f;
    protected const int MAX_TRIAL_NUM = 2;
    protected const int MAX_SET_NUM = 3;
    protected GameObject _center;
    protected float _trialDuration;
    [HideInInspector]
    public bool _isTimeout = false;
    [HideInInspector]
    public bool _isInTrial = false;
    [HideInInspector]
    public bool _isInSet = false;
    protected TextMeshProUGUI _text;
    protected UnityAction<int, int, int, Vector3[]> _startTrialTrigger;
    protected UnityAction<float, string, Vector3> _endTrialTrigger;

    // Start is called before the first frame update
    void Start()
    {
        // OVRManager.display.RecenterPose();
        // LoadNewScene();
        // _text = GameObject.Find("Canvas/TrialState").GetComponent<TextMeshProUGUI>();
    }

    public void Init()
    {
        _text = GameObject.Find("Canvas/TrialState").GetComponent<TextMeshProUGUI>();
        OVRManager.display.RecenterPose();
        if(!_forceSetAndTrial)
        {
            _currentSet = 1;
            _currentTrial = 1;
        }
        StartNewSet();
        // LoadNewScene();
    }

    public void SetPrefabs(GameObject sphere, GameObject cube)
    {
        targetSpherePrefab = sphere;
        goalCubePrefab = cube;
    }

    public void SetExpConditions(int targetDepth)
    {
        if(targetDepth == 1)
        {
            _targetDepth = 1.0f;
        } else if(targetDepth == 2)
        {
            _targetDepth = 2.0f;
        }
    }

    public void ForceSetAndTrial(bool b, int setNum, int trialNum)
    {
        _forceSetAndTrial = b;
        _currentSet = setNum;
        _currentTrial = trialNum;
    }

    protected void StartNewSet()
    {
        if(_currentSet > MAX_SET_NUM)
        {
            return;
        }
        _isInSet = true;
        LoadNewScene();
    }

    protected void EndSet()
    {
        _isInSet = false;
        _currentSet++;
        _currentTrial = 1;
        if(_currentSet > MAX_SET_NUM)
        {
            _text.text = "Finished";
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(_isInTrial)
        {
            _trialDuration += Time.deltaTime;

            if(_trialDuration >= TIME_OUT_THRESHOLD)
            {
                _isTimeout = true;
                // EndTrial();
            }
        } else
        {
            if(!_isInSet)
            {
                if(Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
                {
                    StartNewSet();
                }
            }
        }
    }

    public virtual void StartTrial() {}
    public virtual void EndTrial() {}
    protected virtual void GenerateTargets() {}

    protected void LoadNewScene()
    {
        GenerateCenter();
    }

    protected void GenerateCenter()
    {
        Vector3 pos = new Vector3(0, 0, _targetDepth);
        GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
        sphere.transform.localScale *= TARGET_SIZE*2;
        _center = sphere;
        _center.gameObject.GetComponent<TargetSphere>().IsStartingSphere = true;
        _text.text = "Start";
    }

    protected void DestroyCenter()
    {
        if(_center != null)
        {
            Destroy(_center.gameObject);
        }
        _center = null;
        _text.text = $"Trial {_currentTrial}/{MAX_TRIAL_NUM}";
    }

    protected void EndScene()
    {

    }

    public float GetTrialDuration()
    {
        return _trialDuration;
    }

    public void RegisterForStartEvent(UnityAction<int, int, int, Vector3[]> action)
    {
        _startTrialTrigger += action;
    }

    public void RegisterForEndEvent(UnityAction<float, string, Vector3> action)
    {
        _endTrialTrigger += action;
    }
}
