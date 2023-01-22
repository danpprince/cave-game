using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    public void Start()
    {
        Bake();
    }
    public void Bake()
    {
        NavMeshSurface surface = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        NavMesh.RemoveAllNavMeshData();
        surface.BuildNavMesh();
    }
}
