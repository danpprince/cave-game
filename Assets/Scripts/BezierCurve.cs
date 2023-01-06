using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    public Vector3[] Points;

    public BezierCurve()
    {
        Points = new Vector3[3];
    }
    
    public BezierCurve(Vector3[] Points)
    {
        this.Points = Points;
    }

    public Vector3 StartPosition {get { return Points[0]; }}
    public Vector3 EndPosition {get { return Points[2];}}
    public Vector3 GetSegment(float Time)
    {
        Time = Mathf.Clamp01(Time);
        float recipricalTime = 1 - Time;
        return (recipricalTime * recipricalTime * Points[0]) + (2 * recipricalTime * Time * Points[1] + (Time * Time * Points[2]));
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
