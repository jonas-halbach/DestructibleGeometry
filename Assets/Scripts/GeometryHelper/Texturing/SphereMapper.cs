using UnityEngine;
using System.Collections;

/// <summary>
/// This uv-mapper is for sphere-shaped objects-
/// </summary>
public class SphereMapper : UVMapper {

  /// <summary>
  /// Calculating the uv-coordinates
  /// </summary>
  /// <param name="vertices">the mesh-vertices</param>
  /// <returns>the uv-coordinates for every mesh-point</returns>
  public override Vector2[] Map(Vector3[] vertices)
  {
    Vector2[] mapping = new Vector2[vertices.Length];

    //Taking the x and z value of every point and converting it to a uv-coordinate
    for (int i = 0; i < vertices.Length; i++)
    {
      mapping[i] = new Vector2(vertices[i].x, vertices[i].z);
    }

    return mapping;
  }
}
