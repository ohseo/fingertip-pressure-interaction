using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereVisualizer : MonoBehaviour
{
    public enum SelectionState
    {
        Off = 0,
        Selected,
        Highlighted
    }

    [SerializeField] private MeshRenderer _sphereMeshRenderer = null;

    private static int _colorId = Shader.PropertyToID("_Color");
    private Material[] _sphereMaterials;
    private Color[] _defaultSphereColors = null, _highlightColors = null;

    private SelectionState _currSelectionState = SelectionState.Off;

    public SelectionState CurrSelectionState
    {
        get { return _currSelectionState; }
        set
        {
            var oldState = _currSelectionState;
            _currSelectionState = value;

            if (oldState != _currSelectionState)
            {
                if (_currSelectionState > SelectionState.Off)
                {
                    _sphereMeshRenderer.enabled = true;
                    ChangeSphereColor(_currSelectionState == SelectionState.Selected
                        ? _defaultSphereColors
                        : _highlightColors);
                }
                else
                {
                    _sphereMeshRenderer.enabled = false;
                }
            }
        }
    }

    private void Awake()
    {
        _sphereMaterials = _sphereMeshRenderer.materials;
        int numColors = _sphereMaterials.Length;
        _defaultSphereColors = new Color[numColors];
        _highlightColors = new Color[numColors];
        for (int i = 0; i < numColors; i++)
        {
            _defaultSphereColors[i] = _sphereMaterials[i].GetColor(_colorId);
            _highlightColors[i] = new Color(1.0f, 1.0f, 1.0f, _defaultSphereColors[i].a);
        }

        CurrSelectionState = SelectionState.Off;
    }

    private void OnDestroy()
    {
        if (_sphereMaterials != null)
        {
            foreach(var sphereMaterial in _sphereMaterials)
            {
                if (sphereMaterial != null)
                {
                    Destroy(sphereMaterial);
                }
            }
        }
    }

    private void ChangeSphereColor(Color[] newColors)
    {
        for (int i = 0; i < _sphereMaterials.Length; i++)
        {
            _sphereMaterials[i].SetColor(_colorId, newColors[i]);
        }
    }

}
