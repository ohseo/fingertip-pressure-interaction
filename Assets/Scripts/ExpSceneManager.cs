using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using System;
using Random=UnityEngine.Random;


public class ExpSceneManager : MonoBehaviour
{
    public int _expCondition = 0;
    protected float _targetDepth = 1.0f;
    protected const float TARGET_SIZE = 0.035f;
    protected Vector3 _numGrid = new Vector3(6, 6, 1);
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;
    protected int _currentTrial = 0;

    protected const float TIME_OUT_THRESHOLD = 20.0f;
    protected const int MAX_TRIAL_NUM = 10;
    protected GameObject _center;
    protected float _trialDuration;
    [HideInInspector]
    public bool _isTimeout = false;
    [HideInInspector]
    public bool _isInTrial = false;

    // Start is called before the first frame update
    void Start()
    {
        OVRManager.display.RecenterPose();
        LoadNewScene();
    }

    public void Init()
    {
        OVRManager.display.RecenterPose();
        LoadNewScene();
    }

    public void SetPrefabs(GameObject sphere, GameObject cube)
    {
        targetSpherePrefab = sphere;
        goalCubePrefab = cube;
    }

    public void SetExpConditions(int targetDepth, int targetDensity)
    {
        if(targetDepth == 1)
        {
            _targetDepth = 1.0f;
        } else if(targetDepth == 2)
        {
            _targetDepth = 2.0f;
        }

        if(targetDensity == 1)
        {
            _numGrid = new Vector3(4, 4, 1);
        } else if(targetDensity == 2)
        {
            _numGrid = new Vector3(6, 6, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_isInTrial)
        {
            _trialDuration += Time.deltaTime;

            if(_trialDuration >= TIME_OUT_THRESHOLD)
            {
                _isTimeout = true;
                // EndTrial();
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
    }

    protected void DestroyCenter()
    {
        if(_center != null)
        {
            Destroy(_center.gameObject);
        }
        _center = null;
    }
}
