using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

public class ExpSceneManager : MonoBehaviour
{
    public float _targetDepth = 0.5f;
    public float _targetSize = 0.01f;
    public GameObject targetSpherePrefab;
    public GameObject goalCubePrefab;
    private int _trialNum = 0;

    private const int TARGET_NUM = 10;
    private const float DEPTH_RANGE = 0.10f;
    private const float HORIZONTAL_RANGE_DEG = 10.0f;
    private const float VERTICAL_RANGE_DEG = 5.0f;
    private const float GOAL_POSITION_DEG = 20.0f;
    private const float HORIZONTAL_BOUNDARY_OFFSET = 10.0f;
    private const float VERTICAL_CENTER_OFFSET = 0.0f;
    private List<GameObject> _targets = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        OVRManager.display.RecenterPose();
        GenerateGoalCube();
        GenerateTargets();
        GenerateMinMax();
        // GenerateCenter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateTargets()
    {
        if(_targets.Count != 0)
        {
            _targets.Clear();
        }
        List<float> xs = GenerateRandomNumbers(-HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f, HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f, TARGET_NUM);
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
        }
    }

    public void GenerateGoalCube()
    {
        float x = Mathf.Sin((HORIZONTAL_BOUNDARY_OFFSET+HORIZONTAL_RANGE_DEG) * Mathf.Deg2Rad)*_targetDepth;
        float y = Mathf.Sin(VERTICAL_RANGE_DEG *0.2f * Mathf.Deg2Rad)*_targetDepth;
        float z = Mathf.Cos(HORIZONTAL_BOUNDARY_OFFSET * Mathf.Deg2Rad)*_targetDepth;
        Vector3 pos = new Vector3(x, y, z);
        GameObject cube = Instantiate(goalCubePrefab, pos, Quaternion.identity);
        cube.transform.localScale *= _targetSize;
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

    private void GenerateCenter()
    {
        Vector3 pos = new Vector3(0, 0, _targetDepth);
        GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
        sphere.transform.localScale *= _targetSize*2;
    }

    private void GenerateMinMax()
    {
        float xmin = Mathf.Sin((-HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float xmax = Mathf.Sin((HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float ymin = Mathf.Sin((-VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float ymax = Mathf.Sin((VERTICAL_RANGE_DEG+VERTICAL_CENTER_OFFSET)*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);
        float zmin = Mathf.Cos(-VERTICAL_RANGE_DEG*Mathf.Deg2Rad)*(_targetDepth-DEPTH_RANGE);

        float xminf = Mathf.Sin((-HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
        float xmaxf = Mathf.Sin((HORIZONTAL_RANGE_DEG-HORIZONTAL_BOUNDARY_OFFSET/2f)*Mathf.Deg2Rad)*(_targetDepth+DEPTH_RANGE);
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
