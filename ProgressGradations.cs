using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressGradations : MonoBehaviour
{
    public int Max = 630;
    public int Min = 0;
    public Color GradationColor = new Color(50, 120, 42);
    public int Count = 100;
    public float Thickness = 2;

    private GameObject _welcomeBackScreen;
    private Material _lineMaterial;
    private RectTransform _rt;
    private float _width;
    private float _height;
    private bool _isRenderingEnabled = true;

    private void Start()
    {
        _rt = GetComponent<RectTransform>();
        _welcomeBackScreen = FindObjectOfType<WelcomeBackView>().gameObject.transform.GetChild(0).gameObject;
        CreateLineMaterial();
    }

    private void CreateLineMaterial()
    {
        if (!_lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader);
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            _lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)CullMode.Front);
            _lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private void Update()
    {
        // TODO event check
        _isRenderingEnabled = !_welcomeBackScreen.activeSelf;
    }


    public void OnRenderObject()
    {
        if (!_isRenderingEnabled)
            return;

        _width = _rt.rect.width;
        _height = _rt.rect.height;

        var halfWidth = _width / 2;
        var spacing = _height / Count;
        var centerCompX = _width / 2;
        var centerCompY = _height / 2;

        _lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.QUADS);
        for (int i = 0; i < Count + 1; ++i)
        {
            var x0 = 0 - centerCompX;
            var x1 = halfWidth;

            if (i % 10 == 0)
                x1 = _width;

            x1 -= centerCompX;
            var y = i * spacing - centerCompY;

            var yShift = Thickness / 2;

            GL.Color(GradationColor);
            GL.Vertex3(x0, y + yShift, 0);
            GL.Vertex3(x0, y - yShift, 0);
            GL.Vertex3(x1, y - yShift, 0);
            GL.Vertex3(x1, y + yShift, 0);
        }
        GL.End();
        GL.PopMatrix();
    }

    
}
