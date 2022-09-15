using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LinearGaugeManager : MonoBehaviour
{

    public ThalmicMyo myo;
    public float handForce;

    // public LinearGauge linearGauge;
    public GameObject cube;
    public TextMeshProUGUI _text;

    void Start()
    {
        LoadCalibrationData();
    }

    private void LoadCalibrationData()
    {
        if (myo != null && myo.internalMyo != null)
        {
            EMGDataSaveSystem.LoadData(myo.internalMyo.emgFilter);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (myo != null && myo.internalMyo != null)
        {
            handForce = myo.internalMyo.emgFilter.emg_weightedSum;

            if (cube != null)
            {
                cube.transform.localScale = new Vector3(handForce * 0.1f, handForce * 0.1f, handForce * 0.1f);
            }
            if(_text != null)
            {
                _text.text = handForce.ToString();
            }
            // if (linearGauge != null)
            // {
            //     linearGauge.value = handForce;
            // }
        }
    }

    public float GetHandForce()
    {
        return handForce;
    }

}
