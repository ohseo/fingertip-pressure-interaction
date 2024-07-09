using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSphere : MonoBehaviour
{
    private Renderer sphereRenderer;
    private Color defaultColor = Color.blue;
    private Color selectedColor = Color.gray;
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

    public void GrabBegin()
    {
        sphereRenderer.material.color = selectedColor;
    }

    public void GrabEnd()
    {
        sphereRenderer.material.color = defaultColor;
    }
}
