using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using System;
using Random=UnityEngine.Random;
using UnityEngine.Events;


public class CubeSceneManager : ExpSceneManager
{
    private const float GOAL_POSITION_DEG = 10f;
    private const float TARGET_POSITION_DEG = 10f;
    private const float VERTICAL_CENTER_OFFSET = -10.0f;
    private GameObject _target;
    private GameObject _goal;
    private Vector3 _goalOffset;
    protected UnityAction _goalInPreTrigger;
    protected UnityAction _goalOutPreTrigger;
    protected UnityAction<float> _goalInTrigger;
    protected UnityAction<float> _goalOutTrigger;

    protected override void Update()
    {
        base.Update();

        if(_isInTrial)
        {
            _goalOffset = _goal.transform.position - _target.transform.position;
            // Debug.Log("Logger: goal offset: "+_goalOffset.x.ToString()+_goalOffset.y.ToString()+_goalOffset.z.ToString());
        }
    }

    public override void StartTrial()
    {
        DestroyCenter();
        GenerateGoalCube();
        GenerateTargets();
        _trialDuration = 0f;
        _isTimeout = false;
        _isInTrial = true;
        _startTrialTrigger.Invoke(_currentSet, _currentTrial, 0, _goal.transform.position);
    }

    public override void EndTrial()
    {
        _endTrialTrigger.Invoke(_trialDuration, _isTimeout.ToString());
        Destroy(_target.gameObject);
        Destroy(_goal.gameObject);
        _target = null;
        _goal = null;
        _currentTrial++;
        _isTimeout = false;
        _isInTrial = false;
        if(_currentTrial > MAX_TRIAL_NUM)
        {
            _text.text = "Set Finished";
            EndSet();
            return;
        }
        LoadNewScene();
    }

    public Vector3 GetGoalPosition()
    {
        return _goal.transform.position;
    }

    public Vector3 GetGoalOffset()
    {
        return _goalOffset;
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
        _target.transform.localScale *= TARGET_SIZE;
        _target.GetComponent<TargetSphere>().IsExpTarget = true;
    }

    private void GenerateGoalCube()
    {
        float x = Mathf.Sin(GOAL_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;
        float y = Mathf.Sin(VERTICAL_CENTER_OFFSET * Mathf.Deg2Rad)*_targetDepth;
        float z = Mathf.Cos(GOAL_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth;
        Vector3 pos = new Vector3(x, y, z);
        GameObject cube = Instantiate(goalCubePrefab, pos, Quaternion.identity);
        cube.transform.localScale *= TARGET_SIZE;
        _goal = cube;
        _goalInPreTrigger = new UnityAction(OnGoalIn);
        _goalOutPreTrigger = new UnityAction(OnGoalOut);
        _goal.GetComponent<GoalCube>().TriggerEnter += _goalInPreTrigger;
        _goal.GetComponent<GoalCube>().TriggerExit += _goalOutPreTrigger;
    }

    public void OnGoalIn()
    {
        _goalInTrigger.Invoke(_trialDuration);
    }

    public void OnGoalOut()
    {
        _goalOutTrigger.Invoke(_trialDuration);
    }

    public void RegisterForGoalInEvent(UnityAction<float> action)
    {
        _goalInTrigger += action;
    }
    public void RegisterForGoalOutEvent(UnityAction<float> action)
    {
        _goalOutTrigger += action;
    }
}
