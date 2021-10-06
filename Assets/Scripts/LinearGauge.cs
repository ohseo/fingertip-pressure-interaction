using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinearGauge : MonoBehaviour
{

    // Use this for initialization
    [Range(0.0f, 1.0f)]
    public float value = 0;
    
    [SerializeField]
    Image linearGauge;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (myo != null) value = myo.internalMyo.emgFilter.emg_weightedSum;
        if (linearGauge != null) linearGauge.fillAmount = value;
    }
}
