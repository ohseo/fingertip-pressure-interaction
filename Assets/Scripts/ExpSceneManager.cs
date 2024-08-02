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
    public float _targetSize = 0.01f;
    public Vector3 _numGrid = new Vector3(3, 3, 3);
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;
    public RayModifyingTool rayModifyingTool;
    public BaselineRaycastingTool baselineRaycastingTool;
    private int _trialNum = 0;
    private int _currentTrial = 0;

    private const int TARGET_NUM = 20;
    private const float DEPTH_RANGE = 0.05f;
    private const float HORIZONTAL_RANGE_DEG = 5.0f;
    private const float HORIZONTAL_RANGE_M = 0.2f;
    private const float VERTICAL_RANGE_DEG = 5.0f;
    private const float VERTICAL_RANGE_M = 0.2f;
    private const float DEPTH_RANGE_M = 0.1f;
    private const float GOAL_POSITION_DEG = 20.0f;
    private const float TARGET_POSITION_DEG = 20.0f;
    private const float TARGET_GRID_MARGIN = 0.0075f;
    private const float VERTICAL_CENTER_OFFSET = 0.0f;
    private const float TIME_OUT_THRESHOLD = 10.0f;
    private List<GameObject> _targets = new List<GameObject>();
    private List<GameObject> _internalTargets = new List<GameObject>();
    private GameObject _goal;
    private GameObject _center;
    private float _trialDuration;
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

    public void StartTrial()
    {
        DestroyCenter();
        GenerateGoalCube();
        // GenerateTargets();
        GenerateTargetsByGrid();
        // GenerateMinMax();
        _trialDuration = 0f;
        _isTimeout = false;
        _isInTrial = true;
    }

    public void EndTrial()
    {
        foreach(GameObject target in _targets)
        {
            Destroy(target);
        }
        _targets.Clear();
        Destroy(_goal.gameObject);
        _goal = null;
        LoadNewScene();
        _isTimeout = false;
        _isInTrial = false;
    }

    private void LoadNewScene()
    {
        GenerateCenter();
    }

    private void GenerateTargets()
    {
        if(_targets.Count != 0)
        {
            _targets.Clear();
        }
        List<float> xs = GenerateRandomNumbers(-HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG, HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG, TARGET_NUM);
        List<float> ys = GenerateRandomNumbers(-VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET, VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET, TARGET_NUM);
        List<float> zs = GenerateRandomNumbers(-DEPTH_RANGE, DEPTH_RANGE, TARGET_NUM);
        Shuffle(xs);
        Shuffle(ys);
        Shuffle(zs);


        for(int i=0; i<TARGET_NUM; i++)
        {
            float x = Mathf.Sin(xs[i]*Mathf.Deg2Rad)*(_targetDepth+zs[i]);
            float y = Mathf.Sin(ys[i]*Mathf.Deg2Rad)*(_targetDepth+zs[i]);
            float z = Mathf.Cos(ys[i]*Mathf.Deg2Rad)*(_targetDepth+zs[i]);
            Vector3 pos = new Vector3(x, y, z);
            GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
            sphere.transform.localScale *= _targetSize;
            if(i==0)
            {
                sphere.GetComponent<TargetSphere>().IsExpTarget = true;
            }
            _targets.Add(sphere);
        }
    }

    private void GenerateTargetsByGrid()
    {
        if(_targets.Count != 0)
        {
            _targets.Clear();
        }
        if(_internalTargets.Count != 0)
        {
            _internalTargets.Clear();
        }
        // float areaSize = 0.2f;
        // int numGridX = 5;
        // int numGridY = 3;
        // int numGridZ = 3;
        Vector3 areaSize = new Vector3(HORIZONTAL_RANGE_M, VERTICAL_RANGE_M, DEPTH_RANGE_M);
        // Vector3 numGrid = new Vector3(3, 3, 3);
        Vector3 targetPosition = new Vector3();
        targetPosition.x = Mathf.Sin(-TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth-areaSize.x/2f;
        targetPosition.y = Mathf.Sin(VERTICAL_CENTER_OFFSET * Mathf.Deg2Rad)*_targetDepth-areaSize.y/2f;
        targetPosition.z = Mathf.Cos(TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth-areaSize.z/2f;

        // float gridSize = areaSize.x/numGrid.x;
        Vector3 gridSize = new Vector3(areaSize.x/_numGrid.x, areaSize.y/_numGrid.y, areaSize.z/_numGrid.z);

        for (int i=0; i<_numGrid.x; i++)
        {
            for (int j=0; j<_numGrid.y; j++)
            {
                for (int k=0; k<_numGrid.z; k++)
                {        
                    float x = Random.Range(TARGET_GRID_MARGIN, gridSize.x-TARGET_GRID_MARGIN);
                    float y = Random.Range(TARGET_GRID_MARGIN, gridSize.y-TARGET_GRID_MARGIN);
                    float z = Random.Range(TARGET_GRID_MARGIN, gridSize.z-TARGET_GRID_MARGIN);
                    Vector3 pos = new Vector3(x+i*gridSize.x+targetPosition.x, y+j*gridSize.y+targetPosition.y, z+k*gridSize.z+targetPosition.z);
                    GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
                    sphere.transform.localScale *= _targetSize;
                    _targets.Add(sphere);

                    if(i>0 && i<_numGrid.x-1 && j>0 && j<_numGrid.y-1 && k>0 && k<_numGrid.z-1)
                    {
                        _internalTargets.Add(sphere);
                    }
                }
            }
        }

        int r = Random.Range(0, _internalTargets.Count);
        _internalTargets[r].GetComponent<TargetSphere>().IsExpTarget = true;        
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

    private void GenerateCenter()
    {
        Vector3 pos = new Vector3(0, 0, _targetDepth);
        GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
        sphere.transform.localScale *= _targetSize*2;
        _center = sphere;
        _center.gameObject.GetComponent<TargetSphere>().IsStartingSphere = true;
    }

    private void DestroyCenter()
    {
        if(_center != null)
        {
            Destroy(_center.gameObject);
        }
        _center = null;
    }

    private List<float> GenerateRandomNumbers(float min, float max, int count)
    {
        List<float> numbers = new List<float>();
        float rangeSize = (max-min)/(float)count;

        for (int i=0; i<count; i++)
        {
            float rangeMin = (min + i*rangeSize);
            float rangeMax = (min + (i+1)*rangeSize);

            float randomNum = Random.Range(rangeMin, rangeMax);
            numbers.Add(randomNum);
        }
        return numbers;
    }


    private void GenerateMinMax()
    {
        float xmin = Mathf.Sin((-HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float xmax = Mathf.Sin((HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float ymin = Mathf.Sin((-VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float ymax = Mathf.Sin((VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float zmin = Mathf.Cos(-VERTICAL_RANGE_DEG*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);

        float xminf = Mathf.Sin((-HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
        float xmaxf = Mathf.Sin((HORIZONTAL_RANGE_DEG-TARGET_POSITION_DEG)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
        float yminf = Mathf.Sin((-VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
        float ymaxf = Mathf.Sin((VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
        float zmax = Mathf.Cos(VERTICAL_RANGE_DEG*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);

        List<Vector3> vectors = new List<Vector3>();
        vectors.Add(new Vector3(xmin, ymin, zmin));
        vectors.Add(new Vector3(xmin, ymax, zmin));
        vectors.Add(new Vector3(xmax, ymin, zmin));
        vectors.Add(new Vector3(xmax, ymax, zmin));
        vectors.Add(new Vector3(xminf, yminf, zmax));
        vectors.Add(new Vector3(xminf, ymaxf, zmax));
        vectors.Add(new Vector3(xmaxf, yminf, zmax));
        vectors.Add(new Vector3(xmaxf, ymaxf, zmax));

        foreach (Vector3 vector in vectors)
        {
            GameObject sphere = Instantiate(targetSpherePrefab, vector, Quaternion.identity);
            sphere.transform.localScale *= _targetSize;
        }
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
