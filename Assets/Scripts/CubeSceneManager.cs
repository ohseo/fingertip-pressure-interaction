using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using System;
using Random=UnityEngine.Random;


public class CubeSceneManager : ExpSceneManager
{
    private const float GOAL_POSITION_DEG = 10.0f;
    private const float TARGET_POSITION_DEG = 10.0f;
    private const float VERTICAL_CENTER_OFFSET = -5.0f;
    private GameObject _target;
    private GameObject _goal;

    public override void StartTrial()
    {
        DestroyCenter();
        GenerateGoalCube();
        GenerateTargets();
        _trialDuration = 0f;
        _isTimeout = false;
        _isInTrial = true;
    }

    public override void EndTrial()
    {
        Destroy(_target.gameObject);
        Destroy(_goal.gameObject);
        _target = null;
        _goal = null;
        LoadNewScene();
        _isTimeout = false;
        _isInTrial = false;
    }

    protected override void GenerateTargets()
    {
        if (_target != null)
        {
            Destroy(_target.gameObject);
            _target = null;
        }

        Vector3 targetPosition = new Vector3();
        targetPosition.x = Mathf.Sin(-TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;
        targetPosition.y = Mathf.Sin(VERTICAL_CENTER_OFFSET * Mathf.Deg2Rad)*_targetDepth;
        targetPosition.z = Mathf.Cos(TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;

        _target = Instantiate(targetSpherePrefab, targetPosition, Quaternion.identity);
        _target.transform.localScale *= _targetSize;
        _target.GetComponent<TargetSphere>().IsExpTarget = true;
    }

    private void GenerateGoalCube()
    {
        float x = Mathf.Sin(GOAL_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;
        float y = Mathf.Sin(VERTICAL_CENTER_OFFSET * Mathf.Deg2Rad)*_targetDepth;
        float z = Mathf.Cos(GOAL_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;
        Vector3 pos = new Vector3(x, y, z);
        GameObject cube = Instantiate(goalCubePrefab, pos, Quaternion.identity);
        cube.transform.localScale *= _targetSize;
        _goal = cube;
    }
}
