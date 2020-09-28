
using System;
using UnityEngine;
using static UnityEngine.Mathf;
public static class FunctionLibrary 
{
    
    [System.Serializable]
    public enum Func
    {
        Wave, 
        MultiWave, 
        Ripple, 
        Sphere, 
        Torus
    }

    public delegate Vector3 Function(float u, float v, float t);

    public static Function GetFunction(Func function)
    {
        switch (function)
        {
            case Func.Wave:
                return Wave;
            case Func.MultiWave:
                return MultiWave;
            case Func.Ripple:
                return Ripple;
            case Func.Sphere:
                return Sphere;
            case Func.Torus:
                return Torus;
            default:
                return Wave;
        }
    }
    
    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * PI * (v + t));
        p.y *= (2f / 3f);
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t));
        p.y /= (1f + 10f * d);
        p.z = v;
        return p;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        Vector3 p;
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
        float s = r * Cos(0.5f * PI * v);
        p.x = s * Sin(PI * u);
        p.y = r * Sin(PI * 0.5f * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    public static Vector3 Torus(float u, float v, float t)
    {
        Vector3 p;
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos( PI * v);

        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);

        return p;
    }
}
