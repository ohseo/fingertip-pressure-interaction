using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGauge : MonoBehaviour
{

    public float value = 0;
    public float min = 0;
    public float max = 1;

    [SerializeField]
    RectTransform gaugeRectTransform;

    [SerializeField]
    RectTransform minGaugeRectTransform;

    [SerializeField]
    RectTransform maxGaugeRectTransform;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (value < 0) value = 0;
        if (value > 1) value = 1;
        if (gaugeRectTransform != null)
            gaugeRectTransform.offsetMax = new Vector2(-(1 - value) * GetComponent<RectTransform>().rect.width, 0);

        if (minGaugeRectTransform != null)
            minGaugeRectTransform.offsetMax = new Vector2(-(1 - min) * GetComponent<RectTransform>().rect.width, 0);

        if (maxGaugeRectTransform != null)
            maxGaugeRectTransform.offsetMin = new Vector2(max * GetComponent<RectTransform>().rect.width, 0);
    }
}
