using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    public Vector3[] Points;

    public BezierCurve()
    {
        Points = new Vector3[4];
    }
    
    public BezierCurve(Vector3[] Points)
    {
        this.Points = Points;
    }

    public Vector3 StartPosition {get { return Points[0]; }}
    public Vector3 EndPosition {get { return Points[3];}}
    public Vector3 GetSegment(float Time)
    {
        Time = Mathf.Clamp01(Time);
        float time = 1 - Time;
        return (time * time * time * Points[0]) 
             + (3 * time * time * time * Points[1])
             + (3 * time * time * time * Points[2]) 
             + (Time * Time * Time * Points[3]);
    }

    public Vector3[] GetSegments(int subDivisions)
    {
        Vector3[] segments = new Vector3[subDivisions];
        float time;
        for(int i = 0; i < subDivisions; i++)
        {
            time = (float)i / subDivisions;
            segments[i] = GetSegment(time);
        }
        return segments;
    }



}
