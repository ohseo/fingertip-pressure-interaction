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
    public float _targetDepth = 1.0f;
    public float _targetSize = 0.035f;
    public Vector3 _numGrid = new Vector3(4, 4, 4);
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;
    private int _trialNum = 0;
    private int _currentTrial = 0;

    private const float TIME_OUT_THRESHOLD = 20.0f;
    private GameObject _center;
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
        Debug.Log("ExpSceneManager Init");
        OVRManager.display.RecenterPose();
        LoadNewScene();
    }

    public void SetPrefabs(GameObject sphere, GameObject cube)
    {
        targetSpherePrefab = sphere;
        goalCubePrefab = cube;
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
        sphere.transform.localScale *= _targetSize*2;
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
