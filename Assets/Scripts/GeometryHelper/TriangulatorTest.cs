using UnityEngine;


/// <summary>
/// "Usage" from Unity3d-Wiki!
/// URL:
/// http://wiki.unity3d.com/index.php?title=Triangulator
/// 
/// Author: runevision
/// 
/// This class is used to test triangulators.
/// </summary>
public class TriangulatorTest : MonoBehaviour
{
  void Start()
  {
    // Create Vector2 vertices
    Vector3[] vertices3D = new Vector3[] {
            new Vector3(5, 10, 0),
            new Vector3(10, -5, 0),
            new Vector3(-7, -3, 0),
            new Vector3(-2, -8, 0),
            new Vector3(9, -10, 0),
        };

    // Use the triangulator to get indices for creating triangles
    MidpointTriangulator tr = new MidpointTriangulator(vertices3D);

    Vector3[] vertices = tr.Triangulate().ToArray();
    int[] indices = new int[vertices.Length];

    // Create the Vector3 vertices
    for (int i = 0; i < vertices.Length; i++)
    {
      indices[i] = i;
    }

    // Create the mesh
    Mesh msh = new Mesh();
    msh.vertices = vertices;
    msh.triangles = indices;
    msh.RecalculateNormals();
    msh.RecalculateBounds();

    // Set up game object with mesh;
    MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
    filter.mesh = msh;
  }
}