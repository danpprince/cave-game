using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using Common.Unity.Drawing;

using PathCreation;

using MarchingCubesProject;
using Unity.AI.Navigation;

public enum MARCHING_MODE { CUBES, TETRAHEDRON };

public class MarchingCubesTunnelGenerator : MonoBehaviour
{

    public Material material;

    public MARCHING_MODE mode = MARCHING_MODE.CUBES;

    public int seed = 0;

    public bool smoothNormals = false;

    public bool drawNormals = false;

    private NormalRenderer normalRenderer;

    private VoxelArray voxels;

    public int axisResolution;
    private int width, height, depth;

    public float worldLimits;
    public float surfaceThreshold;
    public float insideColliderPositiveIntensity;
    public float insideColliderNegativeIntensity;
    public float floorDistance;  // Distance from point on curve to "floor" intensity calculation point
    public float noiseAmount;
    public int offsetRadiusSteps;

    private GameObject terrainObject;

    // Clear terrain before repopulating voxels to prevent previous colliders from being used in
    // voxel intensity calculation
    public void ClearTerrainObject()
    {
        if (Application.isEditor)
        {
            DestroyImmediate(terrainObject);
        }
        else
        {
            Destroy(terrainObject);
        }
    }

    public void PopulateVoxels()
    {
        width = axisResolution;
        height = axisResolution;
        depth = axisResolution;

        voxels = new VoxelArray(width, height, depth);

        // Initialize voxels to 0
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    voxels[x, y, z] = 0;
                }
            }
        }

        PathCreator[] pathCreators = GetComponentsInChildren<PathCreator>();
        foreach (PathCreator pathCreator in pathCreators)
        {
            PopulateVoxelsForPath(pathCreator);
        }

        VoxelColliderProperties[] voxelColliderProperties =
            GetComponentsInChildren<VoxelColliderProperties>();
        // First populate all positive intensity, then negative so they can subtract from the former
        foreach (bool populatePositiveIntensity in new []{true, false})
        {
            foreach (VoxelColliderProperties vcp in voxelColliderProperties)
            {
                PopulateVoxelsForCollider(vcp, populatePositiveIntensity);
            }
        }
    }

    void PopulateVoxelsForPath(PathCreator pathCreator)
    { 
        // Mark voxels with increasing intensity the closer they are to the path
        float stepSize = 0.5f;  // Step size in world units  
        int numSteps = (int)Mathf.Ceil(pathCreator.path.length / stepSize);
        for (int stepIndex = 0; stepIndex < numSteps; stepIndex++)
        {
            // "Time" is the point on the curve in the range (0, 1)
            float t = ((float)stepIndex) / numSteps;
            Vector3 pointOnCurve = pathCreator.path.GetPointAtTime(t);
            Vector3 pointOnFloor = pointOnCurve + new Vector3(0, -floorDistance, 0);
            Vector3Int curvePointVoxelIndices = WorldPointToVoxelIndices(pointOnCurve);

            for (int xOffset = -offsetRadiusSteps; xOffset <= offsetRadiusSteps; xOffset++)
            {
                for (int yOffset = -offsetRadiusSteps; yOffset <= offsetRadiusSteps; yOffset++)
                {
                    for (int zOffset = -offsetRadiusSteps; zOffset <= offsetRadiusSteps; zOffset++)
                    {
                        List<int> offsetVoxelIndices = new List<int>
                        {
                            xOffset + curvePointVoxelIndices.x,
                            yOffset + curvePointVoxelIndices.y,
                            zOffset + curvePointVoxelIndices.z
                        };

                        Vector3 offsetWorldPoint = VoxelIndicesToWorldPoint(offsetVoxelIndices);

                        // Use distance to center to make tunnel circular
                        float distanceToCenterIntensity = 1 / Vector3.Distance(pointOnCurve, offsetWorldPoint);
                        // Use distance to "floor" point to make bottom of tunnel flatter
                        float distanceToFloorIntensity = 0.5f / Vector3.Distance(pointOnFloor, offsetWorldPoint);
                        // Use random intensity to make tunnel more jagged
                        float noiseIntensity = Random.Range(-noiseAmount, noiseAmount);

                        voxels[offsetVoxelIndices[0], offsetVoxelIndices[1], offsetVoxelIndices[2]]
                            += distanceToCenterIntensity + distanceToFloorIntensity + noiseIntensity;
                    }
                }
            }

        }
    }

    void PopulateVoxelsForCollider(VoxelColliderProperties vcp, bool populatePositiveIntensity)
    {
        GameObject attachedObject = vcp.gameObject;
        Collider collider = attachedObject.GetComponent<Collider>();

        int margin = 1;
        Vector3Int marginVector = Vector3Int.one * margin;
        Vector3 minPoint = collider.bounds.min;
        Vector3 maxPoint = collider.bounds.max;

        Debug.Log($"Populating voxels for collider {collider.name} with min {minPoint}, max {maxPoint}");

        Vector3Int minVoxelIndices = WorldPointToVoxelIndices(minPoint) - marginVector;
        Vector3Int maxVoxelIndices = WorldPointToVoxelIndices(maxPoint) + marginVector;

        for (int x = minVoxelIndices.x; x <= maxVoxelIndices.x; x++)
        {
            for (int y = minVoxelIndices.y; y <= maxVoxelIndices.y; y++)
            {
                for (int z = minVoxelIndices.z; z <= maxVoxelIndices.z; z++)
                {
                    Vector3 worldPoint = VoxelIndicesToWorldPoint(x, y, z);
                    float sphereRadius = 0.1f;

                    Collider[] overlappingColliders = Physics.OverlapSphere(worldPoint, sphereRadius);
                    foreach (Collider overlapCollider in overlappingColliders)
                    {
                        if (overlapCollider == collider)
                        {
                            float noiseIntensity = Random.Range(-noiseAmount, noiseAmount);
                            if (populatePositiveIntensity && vcp.isPositiveIntensity)
                            {
                                voxels[x, y, z] += insideColliderPositiveIntensity + noiseIntensity;
                            }
                            if (!populatePositiveIntensity && !vcp.isPositiveIntensity)
                            {
                                voxels[x, y, z] = insideColliderNegativeIntensity + noiseIntensity;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    public void GenerateMesh()
    {
        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        Marching marching = null;
        if (mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingTertrahedron();
        else
            marching = new MarchingCubes();

        //Surface is the value that represents the surface of mesh
        marching.Surface = surfaceThreshold;

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels.Voxels, verts, indices);

        for (int vertIndex = 0; vertIndex < verts.Count; vertIndex++)
        {
            verts[vertIndex] = VoxelIndicesToWorldPoint(verts[vertIndex]);
        }

        //Create the normals from the voxel.

        if (smoothNormals)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                //Presumes the vertex is in local space where
                //the min value is 0 and max is width/height/depth.
                Vector3 p = verts[i];

                float u = p.x / (width - 1.0f);
                float v = p.y / (height - 1.0f);
                float w = p.z / (depth - 1.0f);

                Vector3 n = -1 * voxels.GetNormal(u, v, w);

                normals.Add(n);
            }

            normalRenderer = new NormalRenderer();
            normalRenderer.DefaultColor = Color.red;
            normalRenderer.Length = 0.25f;
            normalRenderer.Load(verts, normals);
        }

        var position = new Vector3(0, 0, 0);
        CreateMeshGameObject(verts, normals, indices, position);
    }

    private void CreateMeshGameObject(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
    {
        Mesh mesh = new Mesh();
        mesh.name = "GeneratedMesh";
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);

        if (normals.Count > 0)
            mesh.SetNormals(normals);
        else
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        if (Application.isEditor) {
            DestroyImmediate(terrainObject);
        } else {
            Destroy(terrainObject);
        }

        terrainObject = new GameObject("GeneratedTerrain");
        terrainObject.transform.parent = transform;
        terrainObject.AddComponent<MeshFilter>();
        terrainObject.AddComponent<MeshRenderer>();
        terrainObject.AddComponent<NavMeshSurface>();
        terrainObject.layer = LayerMask.NameToLayer("Terrain");
        terrainObject.GetComponent<NavMeshSurface>().layerMask = LayerMask.GetMask("Terrain");
        var flags = StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;
        GameObjectUtility.SetStaticEditorFlags(terrainObject, flags);
        terrainObject.GetComponent<Renderer>().material = material;
        terrainObject.GetComponent<MeshFilter>().mesh = mesh;
        terrainObject.transform.localPosition = position;

        MeshCollider collider = terrainObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        // Save generated data to generated directory so it does not inflate scene files
        Utils.SaveGameObjectAsPrefab(terrainObject, "GeneratedTerrain");
        AssetDatabase.CreateAsset(mesh, "Assets/Generated/GeneratedMesh.asset");
    }

    private Vector3Int WorldPointToVoxelIndices(Vector3 point)
    {
        return new Vector3Int(
            (int)Mathf.Round(Remap(point.x, -worldLimits, worldLimits, 0, axisResolution)),
            (int)Mathf.Round(Remap(point.y, -worldLimits, worldLimits, 0, axisResolution)),
            (int)Mathf.Round(Remap(point.z, -worldLimits, worldLimits, 0, axisResolution))
        );
    }

    /// <summary>
    /// Convert array indices to a 3D world coordinate
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <param name="zIndex"></param>
    /// <returns></returns>
    private Vector3 VoxelIndicesToWorldPoint(List<int> indices)
    {
        return VoxelIndicesToWorldPoint((float)indices[0], (float)indices[1], (float)indices[2]);
    }

    /// <summary>
    /// Convert floating point indices to a 3D world coordinate
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <param name="zIndex"></param>
    /// <returns></returns>
    private Vector3 VoxelIndicesToWorldPoint(float xIndex, float yIndex, float zIndex)
    {
        Vector3 worldPoint = new Vector3(
            Remap(xIndex, 0, axisResolution, -worldLimits, worldLimits),
            Remap(yIndex, 0, axisResolution, -worldLimits, worldLimits),
            Remap(zIndex, 0, axisResolution, -worldLimits, worldLimits)
        );
        return worldPoint;
    }

    /// <summary>
    /// Convert a floating point Vector3 to 3D world coordinate, useful for marching cubes mesh vertices
    /// </summary>
    /// <param name="indices"></param>
    /// <returns></returns>
    private Vector3 VoxelIndicesToWorldPoint(Vector3 indices)
    {
        return VoxelIndicesToWorldPoint(indices.x, indices.y, indices.z);
    }

    static float Remap(float value, float inMinimum, float inMaximum, float outMinimum, float outMaximum)
    {
        float t = (value - inMinimum) / (inMaximum - inMinimum);
        float output = outMinimum + (outMaximum - outMinimum) * t;
        return output;
    }

    void OnDrawGizmosSelected()
    {
        // Don't draw gizmos if the voxels have not been generated
        if (voxels == null)
        {
            return;
        }

        int step = 2;
        float sphereRadius = 0.1f;
        for (int x = 0; x < width; x += step)
        {
            for (int y = 0; y < height; y += step)
            {
                for (int z = 0; z < depth; z += step)
                {
                    float value = voxels[x, y, z];
                    if (value > 0.5)
                    {
                        Gizmos.color = new Color(0, 0, value / surfaceThreshold);
                        var position = VoxelIndicesToWorldPoint(x, y, z);
                        Gizmos.DrawSphere(position, sphereRadius);
                    }
                }
            }
        }
    }
}
