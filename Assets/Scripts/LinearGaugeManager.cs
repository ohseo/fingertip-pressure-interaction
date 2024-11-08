﻿using System.Collections;
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
            // handForce = myo.internalMyo.emgFilter.emg_weightedSum;
            handForce = myo.internalMyo.emgFilter.emg_totalRMS;

            if (cube != null)
            {
                cube.transform.localScale = new Vector3(handForce * 0.2f, handForce * 0.2f, handForce * 0.2f);
            }
            if(_text != null)
            {
                _text.text = handForce.ToString();
            }
        }
    }

    public float GetHandForce()
    {
        // return 1f;
        return handForce;
    }

}
