using UnityEngine;
using System.Collections;

/// <summary>
/// This class is a simple uv-mapper for box-shaped objects
/// </summary>
public class BoxMapper : UVMapper {

  /// <summary>
  /// 
  /// </summary>
  /// <param name="vertices">the mesh-vertices</param>
  /// <returns>the uv-coordinates</returns>
  public override Vector2[] Map(Vector3[] vertices)
  {
    Vector3 v1;
    Vector3 v2;
    Vector3 n;

    Quaternion rotation;

    Vector3 rotated1;
    Vector3 rotated2;
    Vector3 rotated3;

    Vector2[] textureCoordinates = new Vector2[vertices.Length];
    
    //Bringing the corrdinates for every triangles into a 2d-position and transforming it into uv-coordinates.
    for (int i = 0; i < vertices.Length - Destructor.triangleVertexCount + 1; i += Destructor.triangleVertexCount)
    {
      v1 = vertices[i] - vertices[i + 1];
      v2 = vertices[i] - vertices[i + 2];
      n = Vector3.Cross(v1, v2);

      rotation = Quaternion.FromToRotation(n, Vector3.up);

      rotated1 = rotation * vertices[i];
      rotated2 = rotation * vertices[i + 1];
      rotated3 = rotation * vertices[i + 2];

      textureCoordinates[i] = new Vector2(rotated1.x, rotated1.z);
      textureCoordinates[i + 1] = new Vector2(rotated2.x, rotated2.z);
      textureCoordinates[i + 2] = new Vector2(rotated3.x, rotated3.z);

    }
    return textureCoordinates;
  }
}
