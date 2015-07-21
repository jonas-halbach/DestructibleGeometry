using UnityEngine;
using System.Collections;

/// <summary>
/// This interface needs to be implemented to get the texture-coordinates for a gameobject-mesh,
/// after using the slicing algorithm
/// </summary>
public interface IUVMapper {

  /// <summary>
  /// Calculating the uv-oordiantes for texturemapping
  /// </summary>
  /// <param name="vertices">The vertices of the mesh</param>
  /// <returns>the resulting uv-coordinates</returns>
  Vector2[] Map(Vector3[] vertices);
}
