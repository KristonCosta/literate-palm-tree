using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab = default;

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    private Transform[] points;


    [SerializeField]
    private FunctionLibrary.Func function = FunctionLibrary.Func.Wave;
    
    private void Awake()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab);
            point.SetParent(transform, false);
            point.localScale = scale;
            points[i] = point;
        }
    }

    private void Update()
    {
        FunctionLibrary.Function func = FunctionLibrary.GetFunction(this.function);
        float step = 2f / resolution;
        float time = Time.time;
        float v = (0.5f) * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = func(u, v, time);
        }
    }
}


