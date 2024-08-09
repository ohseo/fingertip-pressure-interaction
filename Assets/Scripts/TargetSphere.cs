using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSphere : MonoBehaviour
{
    private Renderer sphereRenderer;
    private Color defaultColor = Color.gray;
    private Color selectedColor = Color.blue;
    private Color highlightedColor = Color.cyan;
    private Color goalinColor = Color.green;
    private Color expTargetColor = Color.yellow;

    public bool IsGrabbed = false;
    public bool IsExpTarget = false;
    public bool IsStartingSphere = false;
    public bool IsInGoal = false;
    // Start is called before the first frame update
    void Start()
    {
        sphereRenderer = gameObject.GetComponent<Renderer>();
        defaultColor.a = 0.5f;
        selectedColor.a = 0.5f;
        highlightedColor.a = 0.5f;
        goalinColor.a = 0.5f;
        expTargetColor.a = 0.5f;

        sphereRenderer.material.color = IsExpTarget ? expTargetColor : defaultColor;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeExpTarget()
    {
        IsExpTarget = true;
        sphereRenderer.material.color = expTargetColor;
    }

    public void Highlight()
    {
        sphereRenderer.material.color = highlightedColor;
    }

    public void Mute()
    {
        sphereRenderer.material.color = IsExpTarget ? expTargetColor : defaultColor;
    }

    public void GrabBegin()
    {
        sphereRenderer.material.color = selectedColor;
        IsGrabbed = true;
    }

    public void GrabEnd()
    {
        IsGrabbed = false;
        this.transform.parent = null;
        sphereRenderer.material.color = defaultColor;
    }

    public void GoalIn()
    {
        sphereRenderer.material.color = goalinColor;
        IsInGoal = true;
    }

    public void GoalOut()
    {
        if(IsGrabbed)
        {
            sphereRenderer.material.color = selectedColor;
        }
        IsInGoal = false;
    }
}
