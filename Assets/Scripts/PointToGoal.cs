using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToGoal : MonoBehaviour
{
    public GameObject goal;

    void Update()
    {
        transform.LookAt(goal.transform);
    }
}
