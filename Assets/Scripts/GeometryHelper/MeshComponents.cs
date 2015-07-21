using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class extends the functionality of the mesh-class
/// </summary>
public class MeshComponents {

  /// <summary>
  /// Storing the mesh data
  /// </summary>
  private List<Vector3> vertices;
  private List<int> triangles;

  public MeshComponents()
  {
    vertices = new List<Vector3>();
    triangles = new List<int>();
  }

  /// <summary>
  /// Returning the index of the specified vector 
  /// </summary>
  /// <param name="vertex">the vector to get the index of</param>
  /// <returns>The index of the vector</returns>
  public int GetVertexIndex(Vector3 vertex)
  {
    return vertices.IndexOf(vertex);
  }

  /// <summary>
  /// Adding triangle to a mesh
  /// </summary>
  /// <param name="vertices">A List of points, which build the triangle mesh</param>
  public void AddTriangles(List<Vector3> vertices)
  {
    for (int i = 0; i < vertices.Count; i++)
    {
      this.vertices.Add(vertices[i]);
      triangles.Add(this.vertices.Count - 1);
    }
  }

  /// <summary>
  /// Setting up the initial mesh
  /// </summary>
  /// <param name="vertices">Intial triangle points</param>
  /// <param name="triangles">inital triangle indizes</param>
  public void SetInitialTriangles(Vector3[] vertices, int[] triangles)
  {
    this.vertices.AddRange(vertices);
    this.triangles.AddRange(triangles);
  }
  
  /// <summary>
  /// Getting the vertices 
  /// </summary>
  /// <returns>the vertices of this component</returns>
  public List<Vector3> GetVertices()
  {
    return vertices;
  }
  
  /// <summary>
  /// Getting an list of indexes which define the triangles of this component
  /// </summary>
  /// <returns>a list of indexes which point to the vertices which are building the trianles of this meshcomponent</returns>
  public List<int> GetTriangles()
  {
    return triangles;
  }

  /// <summary>
  /// This method converts all vertices of this meshcomponents into worldspace coordniates
  /// </summary>
  /// <param name="transform">the transform of the current coordinate system to be able to transform the vertices</param>
  public void ConvertWorldToLocalVertices(Transform transform)
  {
    for(int i = 0; i < vertices.Count; i++)
    {
      vertices[i] = transform.InverseTransformPoint(vertices[i]);
    }
  }

  /// <summary>
  /// Calculating the midpoint of the polygonVertices
  /// </summary>
  /// <param name="polygonVertices">The points to calculate the midpoint of.</param>
  /// <returns>The calculated midpoint of the specified point-array</returns>
  public static Vector3 GetVerticesMidpoint(Vector3[] polygonVertices)
  {
    Vector3 polygonMidPoint = Vector3.zero;
    Vector3 verticesSum = Vector3.zero;
    int verticesCount = polygonVertices.Length;
    HashSet<Vector3> pointAddedYet = new HashSet<Vector3>();
    if (verticesCount > 0)
    {
      // summerizing all meshpoints
      foreach (Vector3 vertex in polygonVertices)
      {
        if (!pointAddedYet.Contains(vertex))
        {
          verticesSum += vertex;
          pointAddedYet.Add(vertex);
        }
      }
      // dividing the meshpoint sum by the number of points  to get the mean value
      polygonMidPoint = verticesSum / pointAddedYet.Count;
    }
    return polygonMidPoint;
  }

  /// <summary>
  /// calculating the midpoint of this object
  /// </summary>
  /// <returns>The midpoint of this object</returns>
  public Vector3 GetMidPoint()
  {
    Vector3 polygonMidPoint = Vector3.zero;
    if (triangles.Count > 0)
    {
      Vector3 currVertex;
      Vector3 verticesSum = Vector3.zero;
      HashSet<Vector3> usedVertices = new HashSet<Vector3>();
      
      // Summerizing the meshpoints
      foreach (int triangle in triangles)
      {
        currVertex = vertices[triangle];
        if (!usedVertices.Contains(currVertex))
        {
          usedVertices.Add(currVertex);
          verticesSum += currVertex;
        }
      }
      // Dividing the meshpoints by the number of points  to get the mean value
      polygonMidPoint = verticesSum / usedVertices.Count;
    }
    return polygonMidPoint;
  }

  /// <summary>
  /// Calculating the midpoint of the mesh
  /// </summary>
  /// <param name="mesh">the mesh to calculate the midpoint of</param>
  /// <returns>The midpoint of the mesh.</returns>
  public static Vector3 GetVerticesMidPoint(Mesh mesh)
  {
    int[] triangles = mesh.triangles;
    Vector3[] vertices = mesh.vertices;
    Vector3 polygonMidPoint = Vector3.zero;
    if (triangles.Length > 0)
    {
      Vector3 currVertex;
      Vector3 verticesSum = Vector3.zero;
      HashSet<Vector3> usedVertices = new HashSet<Vector3>();

      // Summerizing the mesh-points
      foreach (int triangle in triangles)
      {
        currVertex = vertices[triangle];
        if (!usedVertices.Contains(currVertex))
        {
          usedVertices.Add(currVertex);
          verticesSum += currVertex;
        }
      }
      // Deviding the sum by the number of points to get the mean value
      polygonMidPoint = verticesSum / usedVertices.Count;
    }
    return polygonMidPoint;
  }
}
