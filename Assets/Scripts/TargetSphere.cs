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

    public bool IsGrabbed = false;
    // Start is called before the first frame update
    void Start()
    {
        sphereRenderer = gameObject.GetComponent<Renderer>();
        sphereRenderer.material.color = defaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Highlight()
    {
        sphereRenderer.material.color = highlightedColor;
    }

    public void Mute()
    {
        sphereRenderer.material.color = defaultColor;
    }

    public void GrabBegin()
    {
        sphereRenderer.material.color = selectedColor;
        IsGrabbed = true;
    }

    public void GrabEnd()
    {
        sphereRenderer.material.color = Color.white;
        IsGrabbed = false;
    }

    public void GoalIn()
    {
        sphereRenderer.material.color = goalinColor;
    }

    public void GoalOut()
    {
        if(IsGrabbed)
        {
            sphereRenderer.material.color = selectedColor;
        } else
        {
            sphereRenderer.material.color = Color.red;   
        }
    }
}
